using FuelMeasurement.Tools.Compute.Interfaces;
using FuelMeasurement.Tools.CalculationData.Models;
using NLog;
using System.Collections.Generic;
using System;
using System.Linq;
using FuelMeasurement.Tools.ComputeModule.Helpers;

namespace FuelMeasurement.Tools.Compute.Implementations
{
    public class CapacityController : ICapacityController
    {
        private static ILogger _logger;

        #region Вспомогательные данные
        /// <summary>
        /// Константа допуска при расчетах
        /// </summary>
        private const double DELTA = 0.01;

        /// <summary>
        /// Модель вычислителя
        /// </summary>
        public CalculationModel Model { get; set; }
        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        public CapacityController(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Блок вычисления кривых погонной емкости для общего режима
        /// <summary>
        /// Метод формирования массивов графика емкостей датчиков для общего режима
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tanks"></param>
        /// <param name="sensorsCounter"></param>
        public void SetSensorsCapacity(CalculationModel model)
        {
            if (model == null) return;

            Model = model;

            Model.ListSerieX.Clear();
            Model.ListSerieY.Clear();
            Model.ListOfTanksList.Clear();
            Model.ListOfTanksList.Add(Model.SelectedTanks);

            try
            {
                // Запускаем цикл по всему полю углов
                foreach (var angles in Model.AngleFields)
                {
                    // Определяем массивы точек графика погонной емкости по количеству датчиков и по количеству топливных баков
                    // На один датчик - 2 точки
                    // На один топливный бак - 2 точки
                    // Плюс одна точка для подскока (скачка)
                    int arrayLength = Model.ActiveSensorsInCurrentTanks * 3 + Model.SelectedTanks.Count * 2;
                    double[] arrX = new double[arrayLength];
                    double[] arrY = new double[arrayLength];

                    // Вызываем метод формирования массивов для емкостных показателей
                    ComputeCapacityArrays(ref arrX, ref arrY, angles.Pitch, angles.Roll, Model.ListOfTanksList[^1]);

                    Model.ListSerieX.Add(arrX); // Заносим массив оси абсцисс в список
                    Model.ListSerieY.Add(arrY); // Заносим массив оси ординат в список
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка в расчетчике емкости (общий режим): {ex}");
            }
        }

        /// <summary>
        /// Метод обновления массивов графика емкостей датчиков для общего режима
        /// </summary>
        /// <param name="tanks"></param>
        /// <param name="sensorsCounter"></param>
        public void UpdateSensorsCapacity()
        {
            if (Model == null) return;

            try
            {
                for (int i = 0; i < Model.AngleFields.Count; i++)
                {
                    var arrX = Model.ListSerieX[i];
                    var arrY = Model.ListSerieY[i];

                    ComputeCapacityArrays(ref arrX, ref arrY, Model.AngleFields[i].Pitch, Model.AngleFields[i].Roll, Model.ListOfTanksList[^1]);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка в расчетчике емкости (общий режим): {ex}");
            }
        }
        #endregion

        #region Блок вычислений кривых погонной емкости для режима заправки
        /// <summary>
        /// Метод формирования массивов погонной емкости для режима заправки
        /// </summary>
        /// <param name="tanks"></param>
        /// <param name="sensorsCounter"></param>
        public void SetSensorsCapacityIn(CalculationModel model)
        {
            if (model == null) return;

            Model = model;

            Model.ListSerieX.Clear();
            Model.ListSerieY.Clear();

            try
            {
                #region Блок подготовки к вычислениям - формирование списка топливных баков по группам заправки
                // Здесь сначала определим количество групп баков по заправке
                var arrangedTanksGroup = Model.SelectedTanks.OrderBy(item => item.SelectedTankGroupInModel.GroupNumber).ToList();
                Model.SensorsInGroup.Clear();

                int order = -1; // Номер группы заправки топливных баков - -1 для начала, чтобы посчитать все баки
                int index = 0;
                Model.ListOfTanksList.Clear();

                // Бежим по всем бакам - формируем список списков топливных баков (последовательность заправки)
                while (index < Model.SelectedTanks.Count)
                {
                    int sensorsAccumulator = 0;
                    if (order == arrangedTanksGroup[index].SelectedTankGroupInModel.GroupNumber) continue;

                    order = arrangedTanksGroup[index].SelectedTankGroupInModel.GroupNumber;
                    List<TankModel> tanksList = new();
                    while (order == arrangedTanksGroup[index].SelectedTankGroupInModel.GroupNumber)
                    {
                        tanksList.Add(arrangedTanksGroup[index]);
                        var sensorsQuantity = arrangedTanksGroup[index].Sensors.Count(s => s.IsActiveSensor);
                        sensorsAccumulator += sensorsQuantity;
                        index++;
                        if (index == Model.SelectedTanks.Count) break;
                    }
                    Model.ListOfTanksList.Add(tanksList);
                    Model.SensorsInGroup.Add(sensorsAccumulator);
                }
                #endregion

                // Запускаем цикл по всему полю углов
                foreach (var angles in Model.AngleFields)
                {
                    // Определяем массивы точек графика погонной емкости по количеству датчиков
                    // На один датчик - 2 точки
                    // На один топливный бак - 2 точки
                    // Плюс одна точка для подскока (скачка)
                    double[] arrX = new double[Model.ActiveSensorsInCurrentTanks * 3 + Model.SelectedTanks.Count * 2];
                    double[] arrY = new double[Model.ActiveSensorsInCurrentTanks * 3 + Model.SelectedTanks.Count * 2];
                    int startGroupIndex = 0;
                    // И два накопителя - для объема предыдущей группы баков и для емкости
                    double volumeAccumulator = 0.0;
                    double capacityAccumulator = 0.0;

                    if (Model.ListOfTanksList.Count > 0)
                    {
                        // Запускаем цикл по всем полученным спискам заправляемых баков - для каждой комбинации из поля углов
                        for (int t = 0; t < Model.ListOfTanksList.Count; t++)
                        {
                            // Определяем массивы точек графика погонной емкости по количеству датчиков
                            // На один датчик - 3 точки, а также 2 точки на каждый бак (верх и низ)
                            int arrayLength = Model.SensorsInGroup[t] * 3 + Model.ListOfTanksList[t].Count * 2;
                            double[] groupArrX = new double[arrayLength];
                            double[] groupArrY = new double[arrayLength];

                            // Вызываем метод формирования массивов для МПИ
                            ComputeCapacityArrays(ref groupArrX, ref groupArrY, angles.Pitch, angles.Roll, Model.ListOfTanksList[t]);

                            for (int res = 0; res < arrayLength; res++)
                            {
                                arrX[res + startGroupIndex] = groupArrX[res] + volumeAccumulator;
                                arrY[res + startGroupIndex] = groupArrY[res] + capacityAccumulator;
                            }
                            startGroupIndex += arrayLength;
                            volumeAccumulator += groupArrX[^1];
                            capacityAccumulator += groupArrY[^1];
                        }
                        Model.ListSerieX.Add(arrX); // Заносим массив оси абсцисс в список
                        Model.ListSerieY.Add(arrY); // Заносим массив оси ординат в список
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка в расчетчике емкости (режим заправки): {ex}");
            }
        }

        /// <summary>
        /// Метод обновления массивов погонной емкости для режима заправки
        /// </summary>
        public void UpdateSensorsCapacityIn()
        {
            if (Model == null) return;

            try
            {
                for (int i = 0; i < Model.AngleFields.Count; i++)
                {
                    int startGroupIndex = 0;
                    // И два накопителя - для объема предыдущей группы баков и для погрешности
                    double volumeAccumulator = 0.0;
                    double errorAccumulator = 0.0;

                    if (Model.ListOfTanksList.Count > 0)
                    {
                        // Запускаме цикл по всем полученным спискам заправляемых баков - для каждой комбинации из поля углов
                        for (int t = 0; t < Model.ListOfTanksList.Count; t++)
                        {
                            // Определяем массивы точек графика погонной емкости по количеству датчиков
                            // На один датчик - 3 точки, а также 2 точки на каждый бак (верх и низ)
                            int arrayLength = Model.SensorsInGroup[t] * 3 + Model.ListOfTanksList[t].Count * 2;
                            double[] groupArrX = new double[arrayLength];
                            double[] groupArrY = new double[arrayLength];

                            // Вызываем метод формирования массивов для МПИ
                            ComputeCapacityArrays(ref groupArrX, ref groupArrY, Model.AngleFields[i].Pitch, Model.AngleFields[i].Roll, Model.ListOfTanksList[t]);

                            for (int res = 0; res < arrayLength; res++)
                            {
                                Model.ListSerieX[i][res + startGroupIndex] = groupArrX[res] + volumeAccumulator;
                                Model.ListSerieY[i][res + startGroupIndex] = groupArrY[res] + errorAccumulator;
                            }
                            startGroupIndex += arrayLength;
                            volumeAccumulator += groupArrX[^1];
                            errorAccumulator += groupArrY[^1];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка в расчетчике емкости (режим заправки): {ex}");
            }
        }
        #endregion

        #region Блок вычислений кривых погонной емкости для режима выработки
        /// <summary>
        /// Метод формирования массивов погонной емкости для режима выработки
        /// </summary>
        /// <param name="tanks"></param>
        /// <param name="sensorsCounter"></param>
        public void SetSensorsCapacityOut(CalculationModel model)
        {
            if (model == null) return;

            Model = model;

            Model.ListSerieX.Clear();
            Model.ListSerieY.Clear();

            try
            {
                #region Блок подготовки к вычислениям - формирование списка топливных баков по группам выработки
                // Здесь сначала определим количество групп баков по заправке
                var arrangedTanksGroup = Model.SelectedTanks.OrderByDescending(item => item.SelectedTankGroupOutModel.GroupNumber).ToList();
                Model.SensorsInGroup.Clear();

                int order = -1; // Номер группы заправки топливных баков - -1 для начала, чтобы посчитать все баки
                int index = 0;
                Model.ListOfTanksList.Clear();
                // Бежим по всем бакам - формируем список списков топливных баков (последовательность заправки)
                while (index < Model.SelectedTanks.Count)
                {
                    int sensorsAccumulator = 0;
                    if (order == arrangedTanksGroup[index].SelectedTankGroupOutModel.GroupNumber) continue;

                    order = arrangedTanksGroup[index].SelectedTankGroupOutModel.GroupNumber;
                    List<TankModel> tanksList = new();
                    while (order == arrangedTanksGroup[index].SelectedTankGroupOutModel.GroupNumber)
                    {
                        tanksList.Add(arrangedTanksGroup[index]);
                        var sensorsQuantity = arrangedTanksGroup[index].Sensors.Count(sensor => sensor.IsActiveSensor);
                        sensorsAccumulator += sensorsQuantity;
                        index++;
                        if (index == Model.SelectedTanks.Count) break;
                    }
                    Model.ListOfTanksList.Add(tanksList);
                    Model.SensorsInGroup.Add(sensorsAccumulator);
                }
                #endregion

                // Запускаем цикл по всему полю углов
                foreach (var pair in Model.AngleFields)
                {
                    // Определяем массивы точек графика погонной емкости по количеству датчиков
                    // На один датчик - 2 точки
                    // На один топливный бак - 2 точки
                    // Плюс одна точка для подскока (скачка)
                    double[] arrX = new double[Model.ActiveSensorsInCurrentTanks * 3 + Model.SelectedTanks.Count * 2];
                    double[] arrY = new double[Model.ActiveSensorsInCurrentTanks * 3 + Model.SelectedTanks.Count * 2];
                    int startGroupIndex = 0;
                    // И два накопителя - для объема предыдущей группы баков и для погрешности
                    double volumeAccumulator = 0.0;
                    double capacityAccumulator = 0.0;
                    // Запускаме цикл по всем полученным спискам заправляемых баков - для каждой комбинации из поля углов
                    for (int t = 0; t < Model.ListOfTanksList.Count; t++)
                    {
                        // Определяем массивы точек графика погонной емкости по количеству датчиков
                        // На один датчик - 3 точки, а также 2 точки на каждый бак (верх и низ)
                        int arrayLength = Model.SensorsInGroup[t] * 3 + Model.ListOfTanksList[t].Count * 2;
                        double[] groupArrX = new double[arrayLength];
                        double[] groupArrY = new double[arrayLength];

                        // Вызываем метод формирования массивов для МПИ
                        ComputeCapacityArrays(ref groupArrX, ref groupArrY, pair.Pitch, pair.Roll, Model.ListOfTanksList[t]);

                        for (int res = 0; res < arrayLength; res++)
                        {
                            arrX[res + startGroupIndex] = groupArrX[res] + volumeAccumulator;
                            arrY[res + startGroupIndex] = groupArrY[res] + capacityAccumulator;
                        }
                        startGroupIndex += arrayLength;
                        volumeAccumulator += groupArrX[^1];
                        capacityAccumulator += groupArrY[^1];
                    }

                    int len = arrX.Length;
                    // Меняем знак значений по оси абсцисс для отрисовки графика наоборот
                    for (int inv = 0; inv < len; inv++)
                    {
                        arrX[inv] = -arrX[inv];
                    }

                    Model.ListSerieX.Add(arrX); // Заносим массив оси абсцисс в список
                    Model.ListSerieY.Add(arrY); // Заносим массив оси ординат в список
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка в расчетчике емкости (режим выработки): {ex}");
            }
        }

        /// <summary>
        /// Метод обновления массивов погонной емкости для режима выработки
        /// </summary>
        public void UpdateSensorsCapacityOut()
        {
            if (Model == null) return;

            try
            {
                for (int i = 0; i < Model.AngleFields.Count; i++)
                {
                    int startGroupIndex = 0;
                    // И два накопителя - для объема предыдущей группы баков и для погрешности
                    double volumeAccumulator = 0.0;
                    double errorAccumulator = 0.0;

                    if (Model.ListOfTanksList.Count <= 0) continue;

                    // Запускаме цикл по всем полученным спискам заправляемых баков - для каждой комбинации из поля углов
                    for (int t = 0; t < Model.ListOfTanksList.Count; t++)
                    {
                        // Определяем массивы точек графика погонной емкости по количеству датчиков
                        // На один датчик - 2 точки, а также одна точка на низушку и одна - на верхушку
                        int arrayLength = Model.SensorsInGroup[t] * 3 + Model.ListOfTanksList[t].Count * 2;
                        double[] groupArrX = new double[arrayLength];
                        double[] groupArrY = new double[arrayLength];

                        // Вызываем метод формирования массивов для МПИ
                        ComputeCapacityArrays(ref groupArrX, ref groupArrY, Model.AngleFields[i].Pitch, Model.AngleFields[i].Roll, Model.ListOfTanksList[t]);

                        #region Замена знака
                        int len = groupArrX.Length;
                        // Меняем знак значений по оси абсцисс для отрисовки графика наоборот
                        for (int inv = 0; inv < len; inv++)
                        {
                            groupArrX[inv] = -groupArrX[inv];
                        }
                        #endregion

                        for (int res = 0; res < arrayLength; res++)
                        {
                            Model.ListSerieX[i][res + startGroupIndex] = groupArrX[res] + volumeAccumulator;
                            Model.ListSerieY[i][res + startGroupIndex] = groupArrY[res] + errorAccumulator;
                        }
                        startGroupIndex += arrayLength;
                        volumeAccumulator += groupArrX[^1];
                        errorAccumulator += groupArrY[^1];
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка в расчетчике емкости (режим выработки): {ex}");
            }
        }
        #endregion

        #region Универсальный блок вычисления массивов погонной емкости - для всех режимов
        /// <summary>
        /// Метод формирования массивов погонной емкости для датчиков
        /// </summary>
        /// <param name="arrX">Массив по оси абсцисс (объем)</param>
        /// <param name="arrY">Массив по оси ординат (емкость в пФ)</param>
        /// <param name="pitch">Угол тангажа</param>
        /// <param name="roll">Угол крена</param>
        /// <param name="tanks">Список топливных баков</param>
        public void ComputeCapacityArrays(ref double[] arrX, ref double[] arrY, double pitch, double roll, List<TankModel> tanks)
        {
            List<double> arrayListX = new();
            List<double> arrayListY = new();

            arrayListX.Add(0);
            arrayListY.Add(0);

            #region Блок подготовки
            // Найдем индекс заданных углов в поле углов
            int angleIndex = Model.AngleFields.FindIndex(p => Math.Abs(p.Pitch - pitch) < DELTA && Math.Abs(p.Roll - roll) < DELTA);

            // Сначала необходимо упорядочить датчики по возрастанию нижней кромки
            // Определим список датчиков по всем бакам
            List<SensorMeasuresModel> smList = new();

            // Бежим по всем топливным бакам
            foreach (TankModel tank in tanks)
            {
                foreach (var sensor in tank.Sensors)
                {
                    if (!sensor.IsActiveSensor) continue;

                    SensorMeasuresModel sm = sensor.SensorMeasuresList.Find(item => Math.Abs(item.Pitch - pitch) < DELTA && Math.Abs(item.Roll - roll) < DELTA);

                    if (sm == null) continue;
                    smList.Add(sm);
                }
            }

            // Вспомогательный список значимых сущностей
            List<ValuablePoint> vpList = new();
            // В цикле загоняем топливные баки
            foreach (var t in tanks)
            {
                ValuablePoint vp = new()
                {
                    IsSensor = false,
                    BasePoint = t.TarResultList[angleIndex].ResultAxisY[0],
                    NextPoint = t.TarResultList[angleIndex].ResultAxisY[^1],
                    Tank = t,
                };
                vpList.Add(vp);

                vp = new()
                {
                    IsSensor = false,
                    BasePoint = t.TarResultList[angleIndex].ResultAxisY[^1],
                    NextPoint = null,
                    Tank = t,
                };
                vpList.Add(vp);
            }

            // В цикле загоняем датчики
            foreach (var entity in smList)
            {
                ValuablePoint vp = new()
                {
                    IsSensor = true,
                    BasePoint = entity.Lower, // Показания по оси ординат в нижней точке датчика для текущей комбинации углов
                    NextPoint = entity.Upper, // Показания по оси ординат в верхней точке датчика для текущей комбинации углов
                    Tank = entity.TankModel,
                    LinearCapacity = entity.IntrinsicSensor.SensorLinearCapacity,
                    IntrinsicSensor = entity.IntrinsicSensor
                };
                vpList.Add(vp);

                vp = new ValuablePoint
                {
                    IsSensor = true,
                    BasePoint = entity.Upper, // Показания по оси ординат в верхней точке датчика для текущей комбинации углов
                    NextPoint = null,            // Это была верхняя точка датчика - выше не бывает
                    Tank = entity.TankModel,
                    LinearCapacity = entity.IntrinsicSensor.SensorLinearCapacity, // Возможно, это свойство формировать не надо
                    IntrinsicSensor = entity.IntrinsicSensor
                };
                vpList.Add(vp);
            }

            // И упорядочим сущности по возрастанию низа
            List<ValuablePoint> arrangedValPoints = (from item in vpList orderby item.BasePoint select item).ToList();
            #endregion

            // Вводим вспомогательную переменную - аккумулятор удельной емкости
            double capacityAccumulator = 0;
            // Объявляем промежуточное хранилище объема по каждому баку
            double[] previousVolume = new double[tanks.Count];

            List<AuxiliaryEntity> auxiliaryList = new();

            #region Рабочий цикл формирования кривых погонной емкости
            for (int entity = 1; entity < arrangedValPoints.Count; entity++) // Считаем со второй позиции, так как первая - низ самого нижнего бака
            {
                if (arrangedValPoints[entity].IsSensor) // Проверка - датчик или бак
                {
                    // Проверка, включается ли датчик в работу
                    if (arrangedValPoints[entity].NextPoint != null)
                    #region Блок отработки включения датчика в работу
                    {
                        arrangedValPoints[entity].SensorIn(tanks, angleIndex, auxiliaryList, previousVolume, ref capacityAccumulator, arrayListX, arrayListY);
                    }
                    #endregion
                    else // Датчик выключается из работы
                    #region Блок отработки выключения датчика из работы
                    {
                        arrangedValPoints[entity].SensorOut(tanks, angleIndex, auxiliaryList, previousVolume, ref capacityAccumulator, arrayListX, arrayListY);
                    }
                    #endregion
                }
                else
                {
                    if (arrangedValPoints[entity].NextPoint != null) // Проверка - верх или них бака
                    #region Блок отработки сущности - низа бака
                    {
                        arrangedValPoints[entity].TankIn(tanks, angleIndex, auxiliaryList, previousVolume, ref capacityAccumulator, arrayListX, arrayListY);
                    }
                    #endregion
                    else
                    #region Блок отработки сущности - верха бака
                    {
                        arrangedValPoints[entity].TankOut(tanks, angleIndex, auxiliaryList, previousVolume, ref capacityAccumulator, arrayListX, arrayListY);
                    }
                    #endregion
                }
            }
            #endregion

            if (arrX.Length != arrayListX.Count || arrY.Length != arrayListY.Count)
            {
                _logger.Error("Ошибка в размерности массивов в расчетчике погонной емкости");
                return;
            }

            // Переносим полученные данные из списков в массивы
            for (int i = 0; i < arrayListX.Count; i++)
            {
                arrX[i] = arrayListX[i];
                arrY[i] = arrayListY[i];
            }
        }
        #endregion

    }
}
