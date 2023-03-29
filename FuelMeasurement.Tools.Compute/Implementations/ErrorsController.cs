using FuelMeasurement.Tools.Compute.Interfaces;
using FuelMeasurement.Tools.CalculationData.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using FuelMeasurement.Tools.ComputeModule.Helpers;

namespace FuelMeasurement.Tools.Compute.Implementations
{
    /// <summary>
    /// Контроллер вычислений графиков
    /// </summary>
    public class ErrorsController : IErrorsController
    {
        private static ILogger _logger;

        #region Вспомогательные данные
        private const double Delta = 0.01; // Допустимая погрешность при вычислении шага крена и тангажа

        /// <summary>
        /// Текущий проект
        /// </summary>
        public CalculationModel Model { get; set; }
        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        public ErrorsController(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Блок методов вычисления МПИ в обычном режиме
        /// <summary>
        /// Метод формирования списка кривых МПИ для общего режима
        /// </summary>
        /// <param name="project">Проект</param>
        /// <param name="listX">Список массивов оси абсцисс (объем)</param>
        /// <param name="listY">Список массивов оси ординат (погрешность в %)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="sensorsNo">Количество активных датчиков во всех топливных баках</param>
        /// <param name="volume">Суммарный объем топливных баков</param>
        public void ComputeTankErrors(CalculationModel model)
        {
            if (model == null) return;

            Model = model;

            Model.ListSerieX.Clear();
            Model.ListSerieY.Clear();

            Model.ListOfTanksList.Clear();
            Model.ListOfTanksList.Add(Model.SelectedTanks);

            Model.ActiveSensorsInCurrentTanks = 0;
            foreach(var t in Model.SelectedTanks)
            {
                foreach(var s in t.Sensors)
                {
                    Model.ActiveSensorsInCurrentTanks++;
                }
            }

            try
            {
                // Запускаем цикл по всему полю углов
                foreach (AnglesPair angles in Model.AngleFields)
                {
                    // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                    // На один датчик - 2 точки
                    // На один топливный бак - 2 точки
                    // Плюс одна точка для подскока (скачка)
                    double[] arrX = new double[(Model.ActiveSensorsInCurrentTanks * 3) + (Model.SelectedTanks.Count * 2)];
                    double[] arrY = new double[(Model.ActiveSensorsInCurrentTanks * 3) + (Model.SelectedTanks.Count * 2)];

                    // Вызываем метод формирования массивов для МПИ
                    ComputeErrorArrays(ref arrX, ref arrY, angles.Pitch, angles.Roll, Model.ListOfTanksList[^1], Model.SumVolumeOfCurrentTanks);

                    Model.ListSerieX.Add(arrX);
                    Model.ListSerieY.Add(arrY);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка (Расчет МПИ - общий): {ex}");
            }
        }

        /// <summary>
        /// Метод обновления МПИ для общего режима
        /// </summary>
        /// <param name="listY">Список массивов оси абсцисс (объем)</param>
        /// <param name="listX">Список массивов оси ординат (погрешность в %)</param>
        /// <param name="tanks">Список топливных баков</param>
        public void UpdateTankErrors()
        {
            if(Model == null) return;

            var angles = Model.AngleFields;

            try
            {
                for (int i = 0; i < angles.Count; i++)
                {
                    var arrY = Model.ListSerieY[i];
                    var arrX = Model.ListSerieX[i];
                    ComputeErrorArrays(ref arrX, ref arrY, angles[i].Pitch, angles[i].Roll, Model.ListOfTanksList[^1], Model.SumVolumeOfCurrentTanks);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка (Расчет МПИ - общий): {ex}");
            }
        }
        #endregion

        #region Блок вычисления МПИ для режима заправки
        /// <summary>
        /// Метод формирования массивов МПИ для режима заправки
        /// </summary>
        /// <param name="listY">Список массивов оси абсцисс (объем)</param>
        /// <param name="listX">Список массивов оси ординат (погрешность в %)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="sensorsNo">Количество активных датчиков во всех топливных баках</param>
        /// <param name="volume">Суммарный объем топливных баков</param>
        public void ComputeTankFuelInErrors(CalculationModel model)
        {
            if (model == null) return;

            Model = model;

            Model.ListSerieX.Clear();
            Model.ListSerieY.Clear();

            try
            {
                //TODO Здесь метод формирования массивов МПИ для режима заправки самолета
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
                    if (order != arrangedTanksGroup[index].SelectedTankGroupInModel.GroupNumber)
                    {
                        order = arrangedTanksGroup[index].SelectedTankGroupInModel.GroupNumber;
                        List<TankModel> TanksList = new();
                        while (order == arrangedTanksGroup[index].SelectedTankGroupInModel.GroupNumber)
                        {
                            TanksList.Add(arrangedTanksGroup[index]);
                            var sensorsQnty = arrangedTanksGroup[index].Sensors.Count(s => s.IsActiveSensor);
                            sensorsAccumulator += sensorsQnty;
                            index++;
                            if (index == Model.SelectedTanks.Count) break;
                        }
                        Model.ListOfTanksList.Add(TanksList);
                        Model.SensorsInGroup.Add(sensorsAccumulator);
                    }
                }

                // Запускаем цикл по всему полю углов
                foreach (var angles in Model.AngleFields)
                {
                    // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                    // На один датчик - 2 точки
                    // На один топливный бак - 2 точки
                    // Плюс одна точка для подскока (скачка)
                    double[] arrX = new double[Model.ActiveSensorsInCurrentTanks * 3 + Model.SelectedTanks.Count * 2];
                    double[] arrY = new double[Model.ActiveSensorsInCurrentTanks * 3 + Model.SelectedTanks.Count * 2];
                    int startGroupIndex = 0;
                    // И два накопителя - для объема предыдущей группы баков и для погрешности
                    double volumeAccumulator = 0.0;
                    double errorAccumulator = 0.0;

                    if (Model.ListOfTanksList.Count > 0)
                    {
                        // Запускаем цикл по всем полученным спискам заправляемых баков - для каждой комбинации из поля углов
                        for (int t = 0; t < Model.ListOfTanksList.Count; t++)
                        {
                            // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                            // На один датчик - 2 точки
                            // На один топливный бак - 2 точки
                            // Плюс одна точка для подскока (скачка)
                            int arrayLength = Model.SensorsInGroup[t] * 3 + Model.ListOfTanksList[t].Count * 2;
                            double[] groupArrX = new double[arrayLength];
                            double[] groupArrY = new double[arrayLength];

                            // Вызываем метод формирования массивов для МПИ
                            ComputeErrorArrays(ref groupArrX, ref groupArrY, angles.Pitch, angles.Roll, Model.ListOfTanksList[t], Model.SumVolumeOfCurrentTanks);

                            for (int res = 0; res < arrayLength; res++)
                            {
                                arrX[res + startGroupIndex] = groupArrX[res] + volumeAccumulator;
                                arrY[res + startGroupIndex] = groupArrY[res] + errorAccumulator;
                            }
                            startGroupIndex += arrayLength;
                            volumeAccumulator += groupArrX[^1];
                            errorAccumulator += groupArrY[^1];
                        }
                        Model.ListSerieX.Add(arrX); // Заносим массив оси абсцисс в список
                        Model.ListSerieY.Add(arrY); // Заносим массив оси ординат в список
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка (Расчет МПИ - заправка): {ex}");
            }
        }

        /// <summary>
        /// Метод обновления массивов МПИ для режима заправки
        /// </summary>
        /// <param name="listY">Список массивов оси абсцисс (объем)</param>
        /// <param name="listX">Список массивов оси ординат (погрешность в %)</param>
        public void UpdateTankFuelInErrors()
        {
            if (Model == null) return;

            var angles = Model.AngleFields;

            try
            {
                for (int i = 0; i < angles.Count; i++)
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
                            // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                            // На один датчик - 2 точки
                            // На один топливный бак - 2 точки
                            // Плюс одна точка для подскока (скачка)
                            int arrayLength = Model.SensorsInGroup[t] * 3 + Model.ListOfTanksList[t].Count * 2;
                            double[] groupArrX = new double[arrayLength];
                            double[] groupArrY = new double[arrayLength];

                            // Вызываем метод формирования массивов для МПИ
                            ComputeErrorArrays(ref groupArrX, ref groupArrY, angles[i].Pitch, angles[i].Roll, Model.ListOfTanksList[t], Model.SumVolumeOfCurrentTanks);

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
                _logger.Error($"Ошибка (Расчет МПИ - заправка): {ex}");
            }
        }
        #endregion

        #region Блок вычисления МПИ в режиме выработки
        /// <summary>
        /// Метод формирования массивов МПИ для режима выработки
        /// </summary>
        /// <param name="project">Проект</param>
        /// <param name="listY">Список массивов оси абсцисс (объем)</param>
        /// <param name="listX">Список массивов оси ординат (погрешность в %)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="sensorsNo">Количество активных датчиков во всех топливных баках</param>
        /// <param name="volume">Суммарный объем топливных баков</param>
        public void ComputeTankFuelOutErrors(CalculationModel model)
        {
            if (model == null) return;

            Model = model;

            Model.ListSerieX.Clear();
            Model.ListSerieY.Clear();

            try
            {
                //TODO Здесь метод формирования массивов МПИ для режима заправки самолета
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
                        int sensorsQuantity = arrangedTanksGroup[index].Sensors.Count(sensor => sensor.IsActiveSensor);
                        sensorsAccumulator += sensorsQuantity;
                        index++;
                        if (index == Model.SelectedTanks.Count) break;
                    }
                    Model.ListOfTanksList.Add(tanksList);
                    Model.SensorsInGroup.Add(sensorsAccumulator);
                }

                // Запускаем цикл по всему полю углов
                foreach (var pair in Model.AngleFields)
                {
                    // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                    // На один датчик - 2 точки
                    // На один топливный бак - 2 точки
                    // Плюс одна точка для подскока (скачка)
                    double[] arrX = new double[Model.ActiveSensorsInCurrentTanks * 3 + Model.SelectedTanks.Count * 2];
                    double[] arrY = new double[Model.ActiveSensorsInCurrentTanks * 3 + Model.SelectedTanks.Count * 2];
                    int startGroupIndex = 0;
                    // И два накопителя - для объема предыдущей группы баков и для погрешности
                    double volumeAccumulator = 0.0;
                    double errorAccumulator = 0.0;
                    // Запускаем цикл по всем полученным спискам заправляемых баков - для каждой комбинации из поля углов
                    for (int t = 0; t < Model.ListOfTanksList.Count; t++)
                    {
                        // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                        // На один датчик - 2 точки
                        // На один топливный бак - 2 точки
                        // Плюс одна точка для подскока (скачка)
                        int arrayLength = Model.SensorsInGroup[t] * 3 + Model.ListOfTanksList[t].Count * 2;
                        double[] groupArrX = new double[arrayLength];
                        double[] groupArrY = new double[arrayLength];

                        // Вызываем метод формирования массивов для МПИ
                        ComputeErrorArrays(ref groupArrX, ref groupArrY, pair.Pitch, pair.Roll, Model.ListOfTanksList[t], Model.SumVolumeOfCurrentTanks);

                        for (int res = 0; res < arrayLength; res++)
                        {
                            arrX[res + startGroupIndex] = groupArrX[res] + volumeAccumulator;
                            arrY[res + startGroupIndex] = groupArrY[res] + errorAccumulator;
                        }
                        startGroupIndex += arrayLength;
                        volumeAccumulator += groupArrX[^1];
                        errorAccumulator += groupArrY[^1];
                    }

                    #region Инвертирование знака
                    int len = arrX.Length;
                    // Меняем знак значений по оси абсцисс для отрисовки графика наоборот
                    for (int inv = 0; inv < len; inv++)
                    {
                        arrX[inv] = -arrX[inv];
                    }
                    #endregion

                    Model.ListSerieX.Add(arrX); // Заносим массив оси абсцисс в список
                    Model.ListSerieY.Add(arrY); // Заносим массив оси ординат в список
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка (Расчет МПИ - выработка): {ex}");
            }
        }

        /// <summary>
        /// Метод обновления массивов МПИ для режима выработки
        /// </summary>
        /// <param name="listY">Список массивов оси абсцисс (объем)</param>
        /// <param name="listX">Список массивов оси ординат (погрешность в %)</param>
        public void UpdateTankFuelOutErrors()
        {
            if (Model == null) return;

            var angles = Model.AngleFields;

            try
            {
                for (int i = 0; i < angles.Count; i++)
                {
                    int startGroupIndex = 0;
                    // И два накопителя - для объема предыдущей группы баков и для погрешности
                    double volumeAccumulator = 0.0;
                    double errorAccumulator = 0.0;

                    if (Model.ListOfTanksList.Count <= 0) continue;

                    // Запускаме цикл по всем полученным спискам заправляемых баков - для каждой комбинации из поля углов
                    for (int t = 0; t < Model.ListOfTanksList.Count; t++)
                    {
                        // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                        // На один датчик - 2 точки
                        // На один топливный бак - 2 точки
                        // Плюс одна точка для подскока (скачка)
                        int arrayLength = Model.SensorsInGroup[t] * 3 + Model.ListOfTanksList[t].Count * 2;
                        double[] groupArrX = new double[arrayLength];
                        double[] groupArrY = new double[arrayLength];

                        // Вызываем метод формирования массивов для МПИ
                        ComputeErrorArrays(ref groupArrX, ref groupArrY, angles[i].Pitch, angles[i].Roll, Model.ListOfTanksList[t], Model.SumVolumeOfCurrentTanks);

                        for (int res = 0; res < arrayLength; res++)
                        {
                            groupArrX[res] = -groupArrX[res]; // Инвертирование знака

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
                _logger.Error($"Ошибка (Расчет МПИ - выработка): {ex}");
            }
        }
        #endregion

        #region Блок вычисления массивов МПИ универсальный
        /// <summary>
        /// Метод формирования массивов кривой погрешности для нескольких баков 
        /// </summary>
        /// <param name="project">Проект</param>
        /// <param name="arrX">Расчетный массив оси абсцисс (объем)</param>
        /// <param name="arrY">Расчетный массив оси ординат (погрешность в %)</param>
        /// <param name="pitch">Угол тангажа</param>
        /// <param name="roll">Угол крена</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="volume">Объем топливных баков вышеуказанного списка</param>
        public void ComputeErrorArrays(ref double[] arrX, ref double[] arrY, double pitch, double roll, List<TankModel> tanks, double volume)
        {
            List<double> arrayListX = new();
            List<double> arrayListY = new();

            // Добавление начальных нулей
            arrayListX.Add(0);
            arrayListY.Add(0);

            #region Блок подготовки
            // Найдем индекс заданных углов в поле углов
            int angleIndex = Model.AngleFields.FindIndex(p => Math.Abs(p.Pitch - pitch) < Delta && Math.Abs(p.Roll - roll) < Delta);

            // Определим список датчиков по всем бакам
            List<SensorMeasuresModel> smList = new();
            foreach (TankModel tank in tanks)
            {
                foreach (var s in tank.Sensors)
                {
                    if (!s.IsActiveSensor) continue;

                    SensorMeasuresModel sm = s.SensorMeasuresList.Find(item => Math.Abs(item.Pitch - pitch) < Delta &&
                                                                               Math.Abs(item.Roll - roll) < Delta);

                    if (sm == null) continue;
                    smList.Add(sm);
                }
            }

            List<ValuablePoint> vpList = new();
            // Первый цикл - загоняем данные баков
            foreach (var t in tanks)
            {
                ValuablePoint vp = new()
                {
                    IsSensor = false,
                    BasePoint = t.TarResultList[angleIndex].ResultAxisY[0],
                    NextPoint = t.TarResultList[angleIndex].ResultAxisY[^1],
                    Tank = t
                };
                vpList.Add(vp);

                vp = new ValuablePoint
                {
                    IsSensor = false,
                    BasePoint = t.TarResultList[angleIndex].ResultAxisY[^1],
                    NextPoint = null,
                    Tank = t
                };
                vpList.Add(vp);
            }

            // Второй цикл - загоняем датчики
            foreach (var entity in smList)
            {
                ValuablePoint vp = new()
                {
                    IsSensor = true,
                    BasePoint = entity.Lower,
                    NextPoint = entity.Upper,
                    Tank = entity.TankModel
                };
                vpList.Add(vp);

                vp = new ValuablePoint
                {
                    IsSensor = true,
                    BasePoint = entity.Upper,
                    NextPoint = null,
                    Tank = entity.TankModel
                };
                vpList.Add(vp);
            }
            #endregion

            // И упорядочим сущности по возрастанию низа
            List<ValuablePoint> arrangedValPoints = (from item in vpList orderby item.BasePoint select item).ToList();
            // Объявим счетчик активных датчиков - в каждом баке может быть несколько датчиков
            int[] sensorStack = new int[tanks.Count];
            // Объявим массив последних измеренных объемов (то, что отражает верхнюю точку сущности)
            double[] lastMeasuredVolumes = new double[tanks.Count]; // Изначально нули

            // Цикл по всем сущностям, упорядоченным по возрастанию по оси Y
            for (int entity = 1; entity < arrangedValPoints.Count; entity++)
            {
                if (arrangedValPoints[entity].IsSensor) // Сущность является датчиком
                {
                    if (arrangedValPoints[entity].NextPoint != null) // Датчик включается в работу
                    #region Блок включения датчика в работу
                    {
                        arrangedValPoints[entity].SensorIn(tanks, angleIndex, lastMeasuredVolumes, sensorStack, volume, arrayListX, arrayListY);
                    }
                    #endregion
                    else // Датчик выключается из работы
                    #region Блок выключения датчика из работы
                    {
                        arrangedValPoints[entity].SensorOut(tanks, angleIndex, lastMeasuredVolumes, sensorStack, volume, arrayListX, arrayListY);
                    }
                    #endregion
                }
                else // Сущность является баком
                {
                    if (arrangedValPoints[entity].NextPoint == null) // Верх бака
                    #region Блок отработки сущностей верха баков
                    {
                        arrangedValPoints[entity].TankOut(tanks, angleIndex, lastMeasuredVolumes, sensorStack, volume, arrayListX, arrayListY);
                    }
                    #endregion
                    else // Низ бака
                    #region Блок отработки сущностей низа баков
                    {
                        arrangedValPoints[entity].TankIn(tanks, angleIndex, lastMeasuredVolumes, sensorStack, volume, arrayListX, arrayListY);
                    }
                    #endregion
                }
            }

            if(arrX.Length != arrayListX.Count || arrY.Length != arrayListY.Count)
            {
                _logger.Error("Ошибка в размерности массивов в расчетчике МПИ");
                return;
            }

            // Переносим полученные данные из списков в массивы
            for (int i = 0; i < arrX.Length; i++)
            {
                arrX[i] = arrayListX[i];
                arrY[i] = arrayListY[i];
            }
        }
        #endregion
    }
}
