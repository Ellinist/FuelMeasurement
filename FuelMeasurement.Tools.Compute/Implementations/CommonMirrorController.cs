using FuelMeasurement.Tools.Compute.Interfaces;
using FuelMeasurement.Tools.CalculationData.Models;
using NLog;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FuelMeasurement.Tools.Compute.Implementations
{
    /// <summary>
    /// Расчет МПИ по общему зеркалу топлива
    /// </summary>
    public class CommonMirrorController : ICommonMirrorController
    {
        private const double DELTA = 0.01;
        private static ILogger _logger;

        /// <summary>
        /// Текущий проект
        /// </summary>
        private CalculationModel Model { get; set; }

        public CommonMirrorController(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Метод вычисления массивов МПИ для режима общего зеркала топлива (общий режим или режим заправки)
        /// </summary>
        /// <param name="project">Текущий проект</param>
        /// <param name="listX">Список массивов оси абсцисс (объем)</param>
        /// <param name="listY">Список массивов оси ординат (погрешность в %)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="sensorsNo">Количество активных датчиков во всех топливных баках</param>
        /// <param name="volume">Суммарный объем топливных баков</param>
        public void ComputeCommonMirrorErrors(CalculationModel model)
        {
            if(model == null) return;

            Model = model;
            Model.ListSerieX.Clear();
            Model.ListSerieY.Clear();
            var angles = Model.AngleFields;

            try
            {
                // Запускаем цикл по всему полю углов
                for (int i = 0; i < angles.Count; i++)
                {
                    // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                    // На один датчик - 2 точки
                    // На все топливные баки - 2 точки (низ самого нижнего и верх самого верхнего)
                    // Плюс одна точка для подскока (скачка) - для каждого датчика
                    double[] arrX = new double[Model.ActiveSensorsInCurrentTanks * 3 + 2];
                    double[] arrY = new double[Model.ActiveSensorsInCurrentTanks * 3 + 2];

                    // Вызываем метод формирования массивов для МПИ (по общему зеркалу топлива)
                    ComputeCommonMirrorErrorsArrays(ref arrX, ref arrY, angles[i].Pitch, angles[i].Roll, Model.SelectedTanks);

                    Model.ListSerieX.Add(arrX); // Заносим массив оси абсцисс в список
                    Model.ListSerieY.Add(arrY); // Заносим массив оси ординат в список
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка в расчетчике по общему зеркалу топлива (режимы: общий или заправка): {ex}");
            }
        }

        /// <summary>
        /// метод обновления МПИ по общему зеркалу топлива (общий режим или режим заправки)
        /// </summary>
        /// <param name="listX">Список массивов оси абсцисс (объем)</param>
        /// <param name="listY">Список массивов оси ординат (погрешность в %)</param>
        /// <param name="tanks">Список топливных баков</param>
        public void UpdateCommonMirrorErrors()
        {
            if (Model == null) return;

            var angles = Model.AngleFields;

            try
            {
                for (int i = 0; i < angles.Count; i++)
                {
                    var arrY = Model.ListSerieY[i];
                    var arrX = Model.ListSerieX[i];
                    ComputeCommonMirrorErrorsArrays(ref arrX, ref arrY, angles[i].Pitch, angles[i].Roll, Model.SelectedTanks);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка в расчетчике по общему зеркалу топлива (режимы: общий или заправка): {ex}");
            }
        }

        /// <summary>
        /// Метод вычисления массивов МПИ для режима общего зеркала топлива (режим выработки)
        /// </summary>
        /// <param name="project">Текущий проект</param>
        /// <param name="listX">Список массивов оси абсцисс (объем)</param>
        /// <param name="listY">Список массивов оси ординат (погрешность в %)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="sensorsNo">Количество активных датчиков во всех топливных баках</param>
        /// <param name="volume">Суммарный объем топливных баков</param>
        public void ComputeBackCommonMirrorErrors(CalculationModel model)
        {
            if (model == null) return;

            Model = model;
            Model.ListSerieX.Clear();
            Model.ListSerieY.Clear();
            var angles = Model.AngleFields;

            try
            {
                // Запускаем цикл по всему полю углов
                for (int i = 0; i < angles.Count; i++)
                {
                    // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                    // На один датчик - 2 точки
                    // На все топливные баки - 2 точки (низ самого нижнего и верх самого верхнего)
                    // Плюс одна точка для подскока (скачка) - для каждого датчика
                    double[] arrX = new double[Model.ActiveSensorsInCurrentTanks * 3 + 2];
                    double[] arrY = new double[Model.ActiveSensorsInCurrentTanks * 3 + 2];

                    // Вызываем метод формирования массивов для МПИ (по общему зеркалу топлива)
                    ComputeCommonMirrorErrorsArrays(ref arrX, ref arrY, angles[i].Pitch, angles[i].Roll, Model.SelectedTanks);

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
                _logger.Error($"Ошибка в расчетчике по общему зеркалу топлива (режим выработки): {ex}");
            }
        }

        /// <summary>
        /// Метод обновления массивов МПИ для режима общего зеркала топлива (режим выработки)
        /// </summary>
        /// <param name="listX">Список массивов оси абсцисс (объем)</param>
        /// <param name="listY">Список массивов оси ординат (погрешность в %)</param>
        /// <param name="tanks">Список топливных баков</param>
        public void UpdateBackCommonMirrorErrors()
        {
            if (Model == null) return;

            var angles = Model.AngleFields;

            try
            {
                for (int i = 0; i < angles.Count; i++)
                {
                    var arrY = Model.ListSerieY[i];
                    var arrX = Model.ListSerieX[i];
                    ComputeCommonMirrorErrorsArrays(ref arrX, ref arrY, angles[i].Pitch, angles[i].Roll, Model.SelectedTanks);

                    int len = arrX.Length;
                    // Меняем знак значений по оси абсцисс для отрисовки графика наоборот
                    for (int inv = 0; inv < len; inv++)
                    {
                        arrX[inv] = -arrX[inv];
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Метод вычисления массивов по общему зеркалу топлива для конкретных углов
        /// </summary>
        /// <param name="arrX">Массив для оси абсцисс (объем)</param>
        /// <param name="arrY">Массив для оси ординат (погрешность в %)</param>
        /// <param name="pitch">Угол тангажа</param>
        /// <param name="roll">Угол крена</param>
        /// <param name="tanks">Список топливных баков</param>
        private void ComputeCommonMirrorErrorsArrays(ref double[] arrX, ref double[] arrY, double pitch, double roll, List<TankModel> tanks)
        {
            List<double> arrayListX = new();
            List<double> arrayListY = new();

            arrayListX.Add(0);
            arrayListY.Add(0);

            #region Блок подготовки
            // Найдем индекс заданных углов в поле углов
            int angleIndex = Model.AngleFields.FindIndex(p => Math.Abs(p.Pitch - pitch) < DELTA && Math.Abs(p.Roll - roll) < DELTA);

            // Определим список датчиков по всем бакам
            List<SensorMeasuresModel> smList = new();
            foreach (TankModel tank in tanks)
            {
                for (int s = 0; s < tank.Sensors.Count; s++)
                {
                    if (!tank.Sensors[s].IsActiveSensor) continue;

                    SensorMeasuresModel sm = tank.Sensors[s].SensorMeasuresList.Find(item =>
                                             Math.Abs(item.Pitch - pitch) < DELTA && Math.Abs(item.Roll - roll) < DELTA);

                    if (sm == null) continue;
                    smList.Add(sm);
                }
            }

            List<ValuablePoint> vpList = new();
            // Цикл - загоняем датчики в класс сущностей
            for (int i = 0; i < smList.Count; i++)
            {
                ValuablePoint vp = new()
                {
                    IsSensor = true,
                    BasePoint = smList[i].Lower,
                    NextPoint = smList[i].Upper,
                    Tank = smList[i].TankModel
                };
                vpList.Add(vp);

                vp = new ValuablePoint
                {
                    IsSensor = true,
                    BasePoint = smList[i].Upper,
                    NextPoint = null,
                    Tank = smList[i].TankModel
                };
                vpList.Add(vp);
            }
            #endregion

            // И упорядочим сущности по возрастанию низа - в данном случае, сущности - это точки датчиков
            List<ValuablePoint> arrangedValPoints = (from item in vpList orderby item.BasePoint select item).ToList();
            // Объявим счетчик активных датчиков
            int sensorStack = 0;
            // Объявим массив последних измеренных объемов (то, что отражает верхнюю точку сущности)
            double lastMeasuredVolume = 0;

            // Цикл по всем сущностям, упорядоченным по возрастанию по оси Y
            for (int entity = 0; entity < arrangedValPoints.Count; entity++)
            {
                if (arrangedValPoints[entity].NextPoint != null) // Датчик включается в работу
                #region Блок включения датчика в работу
                {
                    double volumeAccumulator = 0; // Вводим переменную, накапливающую вычисленные объемы
                    double errorAccumulator = 0; // Вводим переменную, накапливающую приращение ошибки между срезами сущностей

                    // Проверяем, есть ли активные датчики - если есть, то погрешность равна нулю (бак виртуально единый)
                    if (sensorStack > 0)
                    {
                        // Считаем только объем для сущности
                        // В цикле перебираем все баки
                        for (int t = 0; t < tanks.Count; t++)
                        {
                            // Если текущая сущность ниже перебираемых баков, то продолжаем (не учитываем объем верхнего бака)
                            if (arrangedValPoints[entity].BasePoint < tanks[t].TarResultList[angleIndex].ResultAxisY[0]) continue;

                            if (arrangedValPoints[entity].BasePoint > tanks[t].TarResultList[angleIndex].ResultAxisY[^1])
                            {
                                volumeAccumulator += tanks[t].TankVolume; // Если бак полностью ниже сущности - суммируем его объем
                                continue;
                            }

                            // Если сущность в пределах бака, то считаем Пифагором
                            // Получаем полный вычисленный объем текущего среза
                            var result = tanks[t].GetVolumeSlice(arrangedValPoints[entity].BasePoint, tanks[t], angleIndex);

                            volumeAccumulator += result; // Суммируем объемы при проходе по всем бакам
                        }
                    }
                    else // Датчиков нет - необходимо считать набежавшую погрешность
                    {
                        // В цикле перебираем все баки
                        for (int t = 0; t < tanks.Count; t++)
                        {
                            // Если текущая сущность ниже перебираемых баков, то продолжаем (не учитываем объем верхнего бака)
                            if (arrangedValPoints[entity].BasePoint < tanks[t].TarResultList[angleIndex].ResultAxisY[0]) continue;

                            if (arrangedValPoints[entity].BasePoint > tanks[t].TarResultList[angleIndex].ResultAxisY[^1])
                            {
                                volumeAccumulator += tanks[t].TankVolume; // Если бак полностью ниже сущности - суммируем его объем
                                errorAccumulator += tanks[t].TankVolume; // Суммируем с неизмеренным объемом, если таковой будет
                                continue;
                            }

                            // Если сущность в пределах бака, то считаем Пифагором
                            // Получаем полный вычисленный объем текущего среза
                            var result = tanks[t].GetVolumeSlice(arrangedValPoints[entity].BasePoint, tanks[t], angleIndex);

                            errorAccumulator += result; // Заносим неизмеренный объем в начале работы датчика
                            volumeAccumulator += result; // Суммируем объемы при проходе по всем бакам
                        }
                    }

                    arrayListX.Add(volumeAccumulator); // Заносим объем на точку начала действия датчика
                    arrayListX.Add(volumeAccumulator); // Заносим объем скачка (подскока)

                    arrayListY.Add((0 - errorAccumulator + lastMeasuredVolume) / Model.SumVolumeOfCurrentTanks); // Заносим погрешность для точки начала действия датчика
                    arrayListY.Add(0); // Заносим изменение погрешности для подскока (в случае общего зеркала топлива всегда ноль)

                    sensorStack++; // Увеличиваем значения стека, так как датчик включается в работу
                    lastMeasuredVolume = 0;
                }
                #endregion
                #region Блок выключения датчика из работы
                else // Датчик выключается из работы
                {
                    double volumeAccumulator = 0; // Вводим переменную, накапливающую вычисленные объемы

                    // Проверяем, есть ли активные датчики - если есть, то погрешность равна нулю (бак виртуально единый)
                    if (sensorStack > 0)
                    {
                        // Считаем только объем для сущности
                        // В цикле перебираем все баки
                        for (int t = 0; t < tanks.Count; t++)
                        {
                            // Если текущая сущность ниже рассматриваемых баков, то продолжаем (не учитываем объем верхнего бака)
                            if (arrangedValPoints[entity].BasePoint < tanks[t].TarResultList[angleIndex].ResultAxisY[0]) continue;

                            if (arrangedValPoints[entity].BasePoint > tanks[t].TarResultList[angleIndex].ResultAxisY[^1])
                            {
                                volumeAccumulator += tanks[t].TankVolume; // Если бак полностью ниже сущности - суммируем его объем
                                continue;
                            }

                            // Если сущность в пределах бака, то считаем Пифагором
                            // Получаем полный вычисленный объем текущего среза
                            var result = tanks[t].GetVolumeSlice(arrangedValPoints[entity].BasePoint, tanks[t], angleIndex);

                            volumeAccumulator += result;
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Сюда не должны были попасть!"); // Ай-яй! Датчиков нет, а надо их кол-во уменьшить
                    }

                    sensorStack--; // Уменьшаем значения стека при выключении датчика из работы

                    if (sensorStack == 0) // Отвалился последний датчик - запоминаем объем по срезу данной сущности
                    {
                        lastMeasuredVolume = volumeAccumulator;
                    }

                    arrayListX.Add(volumeAccumulator); // Заносим объем на точку окончания зоны действия датчика
                    arrayListY.Add(0); // Заносим погрешность для точки конца действия датчика
                }
                #endregion
            }

            // И формируем последний выдох господина ПэЖэ - финальный аккорд - точка окончания графика
            arrayListX.Add(Model.SumVolumeOfCurrentTanks);
            arrayListY.Add((0 - Model.SumVolumeOfCurrentTanks + lastMeasuredVolume) / Model.SumVolumeOfCurrentTanks);

            if (arrX.Length != arrayListX.Count || arrY.Length != arrayListY.Count)
            {
                _logger.Error("Ошибка в размерности массивов в расчетчике МПИ по общему зеркалу топлива");
                return;
            }

            // Переносим полученные данные из списков в массивы
            for (int i = 0; i < arrX.Length; i++)
            {
                arrX[i] = arrayListX[i];
                arrY[i] = arrayListY[i];
            }
        }
    }
}
