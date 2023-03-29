using FuelMeasurement.Tools.CalculationData.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FuelMeasurement.Tools.ComputeModule.Helpers
{
    /// <summary>
    /// Вспомогательный класс расчетчика
    /// </summary>
    public static class Calc
    {
        /// <summary>
        /// Количество знаков после запятой
        /// </summary>
        private const int cyphers = 2;

        #region Блок методов для расчета МПИ
        /// <summary>
        /// Расчет МПИ
        /// Метод включения датчика в работу
        /// </summary>
        /// <param name="valuablePoint">Значимая сущность (точка в датчике или баке)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="angleIndex">Индекс поля углов</param>
        /// <param name="lastMeasuredVolumes">Массив верхних измеренных объемов топливных баков</param>
        /// <param name="sensorStack">Массив-счетчик включенных датчиков по бакам</param>
        /// <param name="tanksVolume">Объем переданных топливных баков</param>
        /// <param name="arrayListX">Список вычисляемых объемов</param>
        /// <param name="arrayListY">Список вычисляемых погрешностей</param>
        public static void SensorIn(this ValuablePoint valuablePoint, List<TankModel> tanks,
                                    int angleIndex, double[] lastMeasuredVolumes, int[] sensorStack, double tanksVolume,
                                    List<double> arrayListX, List<double> arrayListY)
        {
            double volumeAccumulator = 0; // Вводим переменную, накапливающую вычисленные объемы
            double errorAccumulator = 0; // Вводим переменную, накапливающую приращение ошибки между срезами сущностей
            double errorJump = 0; // Вводим переменную скачка (подскока) при включении датчика

            // В цикле перебираем все баки - увеличиваем значение стека только для индекса бака, которому принадлежит датчик
            for (int t = 0; t < tanks.Count; t++)
            {
                // Если текущая сущность ниже рассматриваемых баков, то продолжаем (не учитываем объем верхнего бака)
                if (valuablePoint.BasePoint < tanks[t].TarResultList[angleIndex].ResultAxisY[0]) continue;

                if (valuablePoint.BasePoint > tanks[t].TarResultList[angleIndex].ResultAxisY[^1])
                {
                    volumeAccumulator += tanks[t].TankVolume; // Если бак полностью ниже сущности - суммируем его объем
                    errorAccumulator +=
                        tanks[t].TankVolume -
                        lastMeasuredVolumes[t]; // Суммируем с неизмеренным объемом, если таковой будет
                    continue;
                }

                // Если сущность в пределах бака, то считаем Пифагором
                // Получаем полный вычисленный объем текущего среза
                var result = tanks[t].GetVolumeSlice(valuablePoint.BasePoint, tanks[t], angleIndex);

                if (tanks[t] == valuablePoint.Tank) // Если текущая сущность в выбранном баке
                {
                    if (sensorStack[t] == 0) // Активных датчиков еще нет
                    {
                        errorAccumulator +=
                            result - lastMeasuredVolumes[t]; // Заносим неизмеренный объем в начале работы датчика
                        // подскок равен предыдущему измеренному объему
                        errorJump += result - lastMeasuredVolumes[t]; // Заносим значение подскока
                        // Это необходимо до момента, пока все датчики не выключатся в баке
                    }

                    sensorStack[t]++; // Увеличиваем значения стека для бака, которому принадлежит включившийся датчик
                }
                else // Текущая сущность в пределах смежного бака
                {
                    if (sensorStack[t] == 0) // Активных датчиков в точке сущности нет
                    {
                        errorAccumulator += result - lastMeasuredVolumes[t];
                    }
                }

                volumeAccumulator += result; // Суммируем объемы по всем бакам
            }

            arrayListX.Add(Math.Round(volumeAccumulator, cyphers)); // Заносим объем на точку начала действия датчика
            arrayListX.Add(Math.Round(volumeAccumulator, cyphers)); // Заносим объем скачка (подскока)

            arrayListY.Add(Math.Round((0 - errorAccumulator) / tanksVolume, cyphers)); // Заносим погрешность для точки начала действия датчика
            arrayListY.Add(Math.Round((0 - (errorAccumulator - errorJump)) / tanksVolume, cyphers)); // Заносим изменение погрешности для подскока
        }

        /// <summary>
        /// Расчет МПИ
        /// Метод выключения датчика из работы
        /// </summary>
        /// <param name="valuablePoint">Значимая сущность (точка в датчике или баке)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="angleIndex">Индекс поля углов</param>
        /// <param name="lastMeasuredVolumes">Массив верхних измеренных объемов топливных баков</param>
        /// <param name="sensorStack">Массив-счетчик включенных датчиков по бакам</param>
        /// <param name="tanksVolume">Объем переданных топливных баков</param>
        /// <param name="arrayListX">Список вычисляемых объемов</param>
        /// <param name="arrayListY">Список вычисляемых погрешностей</param>
        public static void SensorOut(this ValuablePoint valuablePoint, List<TankModel> tanks,
                                     int angleIndex, double[] lastMeasuredVolumes, int[] sensorStack, double tanksVolume,
                                     List<double> arrayListX, List<double> arrayListY)
        {
            double volumeAccumulator = 0; // Вводим переменную, накапливающую вычисленные объемы
            double errorAccumulator = 0; // Вводим переменную, накапливающую приращение ошибки между срезами сущностей

            // В цикле перебираем все баки - уменьшаем значение стека только для индекса бака, которому принадлежит датчик
            for (int t = 0; t < tanks.Count; t++)
            {
                // Если текущая сущность ниже рассматриваемых баков, то продолжаем (не учитываем объем верхнего бака)
                if (valuablePoint.BasePoint < tanks[t].TarResultList[angleIndex].ResultAxisY[0]) continue;

                if (valuablePoint.BasePoint > tanks[t].TarResultList[angleIndex].ResultAxisY[^1])
                {
                    volumeAccumulator += tanks[t].TankVolume; // Если бак полностью ниже сущности - суммируем его объем
                    errorAccumulator +=
                        tanks[t].TankVolume -
                        lastMeasuredVolumes[t]; // Суммируем с неизмеренным объемом, если таковой будет
                    continue;
                }

                // Если сущность в пределах бака, то считаем Пифагором
                // Получаем полный вычисленный объем текущего среза
                var result = tanks[t].GetVolumeSlice(valuablePoint.BasePoint, tanks[t], angleIndex);

                if (tanks[t] == valuablePoint.Tank) // Если текущая сущность в выбранном баке
                {
                    if (sensorStack[t] > 0) // Проверка, не опустошен ли стек
                    {
                        sensorStack[t]--; // Уменьшаем значения стека для бака, которому принадлежит выключившийся датчик
                    }

                    if (sensorStack[t] == 0) // Если датчиков в пуле больше нет
                    {
                        lastMeasuredVolumes[t] =
                            result; // Заносим по индексу бака последний измеренный объем в пуле датчиков
                    }
                }
                else // Если текущая сущность не принадлежит баку в переборе
                {
                    if (sensorStack[t] == 0) // Если в баке перебора нет активных датчиков
                    {
                        errorAccumulator += result - lastMeasuredVolumes[t];
                    }
                }

                volumeAccumulator += result;
            }

            arrayListX.Add(Math.Round(volumeAccumulator, cyphers)); // Заносим объем на точку окончания зоны действия датчика
            arrayListY.Add(Math.Round((0 - errorAccumulator) / tanksVolume, cyphers)); // Заносим погрешность для точки начала действия датчика
        }

        /// <summary>
        /// Расчет МПИ
        /// Метод включения бака в работу
        /// </summary>
        /// <param name="valuablePoint">Значимая сущность (точка в датчике или баке)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="angleIndex">Индекс поля углов</param>
        /// <param name="lastMeasuredVolumes">Массив верхних измеренных объемов топливных баков</param>
        /// <param name="sensorStack">Массив-счетчик включенных датчиков по бакам</param>
        /// <param name="tanksVolume">Объем переданных топливных баков</param>
        /// <param name="arrayListX">Список вычисляемых объемов</param>
        /// <param name="arrayListY">Список вычисляемых погрешностей</param>
        public static void TankIn(this ValuablePoint valuablePoint, List<TankModel> tanks,
                                  int angleIndex, double[] lastMeasuredVolumes, int[] sensorStack, double tanksVolume,
                                  List<double> arrayListX, List<double> arrayListY)
        {
            double volumeAccumulator = 0; // Вводим переменную, накапливающую вычисленные объемы
            double errorAccumulator  = 0; // Вводим переменную, накапливающую приращение ошибки между срезами сущностей

            // В цикле перебираем все баки
            for (int t = 0; t < tanks.Count; t++)
            {
                if (tanks[t] == valuablePoint.Tank || // Если бак сущности относится в выбранному баку
                    valuablePoint.BasePoint < tanks[t].TarResultList[angleIndex].ResultAxisY[0]
                ) // Если текущая сущность ниже рассматриваемых баков
                {
                    continue; // Продолжаем (переходим к следующему баку)
                }

                if (valuablePoint.BasePoint > tanks[t].TarResultList[angleIndex].ResultAxisY[^1])
                {
                    volumeAccumulator += tanks[t].TankVolume; // Если бак полностью ниже сущности - суммируем его объем
                    errorAccumulator +=
                        tanks[t].TankVolume -
                        lastMeasuredVolumes[t]; // Добавляем накопленную ошибку, если таковая будет
                    continue;
                }

                // Сюда попадаем только, если текущая сущность в пределах перебираемых баков
                // Получаем полный вычисленный объем текущего среза и предыдущего среза
                var result = tanks[t].GetVolumeSlice(valuablePoint.BasePoint, tanks[t], angleIndex);

                if (sensorStack[t] == 0) // Для данного бака не работает датчик (датчики) - можно суммировать
                {
                    errorAccumulator += result - lastMeasuredVolumes[t];
                }

                volumeAccumulator += result;
            }

            arrayListX.Add(Math.Round(volumeAccumulator, cyphers)); // Заносим объем на точку начала зоны действия сущности
            arrayListY.Add(Math.Round((0 - errorAccumulator) / tanksVolume, cyphers)); // Заносим погрешность
        }

        /// <summary>
        /// Расчет МПИ
        /// Метод выключения бака из работы
        /// </summary>
        /// <param name="valuablePoint">Значимая сущность (точка в датчике или баке)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="angleIndex">Индекс поля углов</param>
        /// <param name="lastMeasuredVolumes">Массив верхних измеренных объемов топливных баков</param>
        /// <param name="sensorStack">Массив-счетчик включенных датчиков по бакам</param>
        /// <param name="tanksVolume">Объем переданных топливных баков</param>
        /// <param name="arrayListX">Список вычисляемых объемов</param>
        /// <param name="arrayListY">Список вычисляемых погрешностей</param>
        public static void TankOut(this ValuablePoint valuablePoint, List<TankModel> tanks,
                                   int angleIndex, double[] lastMeasuredVolumes, int[] sensorStack, double tanksVolume,
                                   List<double> arrayListX, List<double> arrayListY)
        {
            double volumeAccumulator = 0; // Вводим переменную, накапливающую вычисленные объемы
            double errorAccumulator = 0; // Вводим переменную, накапливающую приращение ошибки между срезами сущностей

            // В цикле перебираем все баки
            for (int t = 0; t < tanks.Count; t++)
            {
                if (tanks[t] == valuablePoint.Tank) // Если бак сущности относится к выбранному баку
                {
                    volumeAccumulator += tanks[t].TankVolume; // Суммируем его объем
                    errorAccumulator +=
                        tanks[t].TankVolume -
                        lastMeasuredVolumes[t]; // Добавляем накопленную ошибку, если таковая будет
                    continue;
                }

                // Если текущая сущность ниже рассматриваемых баков, то продолжаем (переходим к следующей сущности)
                if (valuablePoint.BasePoint < tanks[t].TarResultList[angleIndex].ResultAxisY[0]) continue;

                // Если текущая сущность выше рассматриваемых баков
                if (valuablePoint.BasePoint > tanks[t].TarResultList[angleIndex].ResultAxisY[^1])
                {
                    volumeAccumulator += tanks[t].TankVolume; // Суммируем его объем
                    errorAccumulator += tanks[t].TankVolume - lastMeasuredVolumes[t]; // Добавляем накопленную ошибку, если таковая будет
                    continue;
                }

                // Если сущность в пределах перебираемого бака, то считаем Пифагором
                // Получаем полный вычисленный объем текущего среза и предыдущего среза
                var result = tanks[t].GetVolumeSlice(valuablePoint.BasePoint, tanks[t], angleIndex);

                if (sensorStack[t] == 0) // Для данного бака не работает датчик (датчики) - можно суммировать
                {
                    errorAccumulator += result - lastMeasuredVolumes[t];
                }

                volumeAccumulator += result;
            }

            arrayListX.Add(Math.Round(volumeAccumulator, cyphers)); // Заносим объем на точку окончания зоны действия датчика
            arrayListY.Add(Math.Round((0 - errorAccumulator) / tanksVolume, cyphers)); // Заносим погрешность
        }
        #endregion

        #region Блок методов для расчета погонной емкости
        /// <summary>
        /// Расчет погонной емкости
        /// Метод включения датчика в работу
        /// </summary>
        /// <param name="valuablePoint">Значимая сущность (точка в датчике или баке)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="angleIndex">Индекс поля углов</param>
        /// <param name="auxiliaryList">Список вспомогательных включенных сущностей (датчиков или баков)</param>
        /// <param name="previousVolume">Массив-хранилище объемов по бакам</param>
        /// <param name="capacityAccumulator">Аккумулятор емкости</param>
        /// <param name="arrayListX">Список вычисляемых объемов</param>
        /// <param name="arrayListY">Список вычисляемых емкостей</param>
        public static void SensorIn(this ValuablePoint valuablePoint, List<TankModel> tanks,
                                    int angleIndex, List<AuxiliaryEntity> auxiliaryList, double[] previousVolume,
                                    ref double capacityAccumulator, List<double> arrayListX, List<double> arrayListY)
        {
            // Датчик включается в работу
            double volumeAccumulator = 0; // Вводим переменную, накапливающую вычисленные объемы

            // В цикле перебираем все топливные баки
            for (int t = 0; t < tanks.Count; t++)
            {
                #region Блок расчета объемов на точках сущностей
                // Если текущая сущность ниже рассматриваемых баков, то продолжаем (не учитываем объем верхнего бака)
                if (valuablePoint.BasePoint < tanks[t].TarResultList[angleIndex].ResultAxisY[0]) continue;

                if (valuablePoint.BasePoint > tanks[t].TarResultList[angleIndex].ResultAxisY[^1])
                {
                    volumeAccumulator += tanks[t].TankVolume; // Если бак полностью ниже сущности - суммируем его объем
                    continue; // Подразумевается, что все датчики нижнего бака уже учтены
                }

                // Если сущность в пределах бака, то считаем Пифагором
                // Получаем полный вычисленный объем текущего среза в перебираемом баке
                double sliceTankVolume = tanks[t].GetVolumeSlice(valuablePoint.BasePoint, tanks[t], angleIndex);
                #endregion

                #region Блок вычисления накопления емкости
                // Идем в цикле по всем датчикам сформированного вспомогательного класса
                capacityAccumulator += auxiliaryList.Where(aux => aux.IntrinsicSensor.FuelTank == tanks[t])
                    .Sum(aux => aux.SensorCapacity * aux.IntrinsicSensor.Length * (sliceTankVolume - previousVolume[t]) / aux.DeltaVolume);

                previousVolume[t] = sliceTankVolume;
                #endregion

                volumeAccumulator += sliceTankVolume;
            }

            // Итак, все предыдущие датчики (буде таковые есть) учтены, надо включить новый датчик
            AuxiliaryEntity auxiliaryEntity = new()
            {
                SensorCapacity = valuablePoint.LinearCapacity,
                IntrinsicSensor = valuablePoint.IntrinsicSensor,
                DeltaVolume =
                    Math.Round(valuablePoint.Tank.GetDeltaVolume(valuablePoint.BasePoint, (double)valuablePoint.NextPoint,
                               valuablePoint.Tank, angleIndex), cyphers)
            };
            auxiliaryList.Add(auxiliaryEntity); // Новый датчик подоспел - включаем

            // В итоге имеем текущее накопление объема (для оси абсцисс графика) и накопленный аккумулятор показаний
            arrayListX.Add(Math.Round(volumeAccumulator, cyphers)); // Добавляем показания объема
            arrayListY.Add(Math.Round(capacityAccumulator, cyphers)); // Добавляем показания емкости

            // Коррекция по добавлению дополнительной точки графика
            // Она совпадает с текущим значением
            arrayListX.Add(Math.Round(volumeAccumulator, cyphers)); // На дополнительную точку включения датчика
            arrayListY.Add(Math.Round(capacityAccumulator, cyphers));
        }

        /// <summary>
        /// Расчет погонной емкости
        /// Метод выключения датчика из работы
        /// </summary>
        /// <param name="valuablePoint">Значимая сущность (точка в датчике или баке)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="angleIndex">Индекс поля углов</param>
        /// <param name="auxiliaryList">Список вспомогательных включенных сущностей (датчиков или баков)</param>
        /// <param name="previousVolume">Массив-хранилище объемов по бакам</param>
        /// <param name="capacityAccumulator">Аккумулятор емкости</param>
        /// <param name="arrayListX">Список вычисляемых объемов</param>
        /// <param name="arrayListY">Список вычисляемых емкостей</param>
        public static void SensorOut(this ValuablePoint valuablePoint, List<TankModel> tanks,
                                     int angleIndex, List<AuxiliaryEntity> auxiliaryList, double[] previousVolume,
                                     ref double capacityAccumulator, List<double> arrayListX, List<double> arrayListY)
        {
            double volumeAccumulator = 0; // Вводим переменную, накапливающую вычисленные объемы
            // В цикле перебираем все топливные баки
            for (int t = 0; t < tanks.Count; t++)
            {
                #region Блок расчета объемов на точках сущностей
                // Если текущая сущность ниже рассматриваемых баков, то продолжаем (не учитываем объем верхнего бака)
                if (valuablePoint.BasePoint < tanks[t].TarResultList[angleIndex].ResultAxisY[0]) continue;

                if (valuablePoint.BasePoint > tanks[t].TarResultList[angleIndex].ResultAxisY[^1])
                {
                    volumeAccumulator += tanks[t].TankVolume; // Если бак полностью ниже сущности - суммируем его объем
                    continue; // Подразумевается, что все датчики нижнего бака уже учтены
                }

                // Если сущность в пределах бака, то считаем Пифагором
                // Получаем полный вычисленный объем текущего среза в перебираемом баке
                double sliceTankVolume = tanks[t].GetVolumeSlice(valuablePoint.BasePoint, tanks[t], angleIndex);
                #endregion

                #region Блок вычисления накопления емкости
                // Идем в цикле по всем датчикам сформированного вспомогательного класса
                capacityAccumulator += auxiliaryList.Where(aux => aux.IntrinsicSensor.FuelTank == tanks[t])
                    .Sum(aux => aux.SensorCapacity * aux.IntrinsicSensor.Length *
                        (sliceTankVolume - previousVolume[t]) / aux.DeltaVolume);

                previousVolume[t] = sliceTankVolume;
                #endregion

                volumeAccumulator += sliceTankVolume;
            }

            // Сначала найдем запись в классе Фиг Знает, соответствующую текущему выключаемому датчику
            int outIndex = auxiliaryList.FindIndex(item => item.IntrinsicSensor == valuablePoint.IntrinsicSensor);
            // И выключим его
            auxiliaryList.RemoveAt(outIndex);

            // В итоге имеем текущее накопление объема (для оси абсцисс графика) и накопленный аккумулятор показаний
            arrayListX.Add(Math.Round(volumeAccumulator, cyphers)); // Добавляем показания объема
            arrayListY.Add(Math.Round(capacityAccumulator, cyphers)); // Добавляем показания емкости
        }

        /// <summary>
        /// Расчет погонной емкости
        /// Метод включения бака в работу
        /// </summary>
        /// <param name="valuablePoint">Значимая сущность (точка в датчике или баке)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="angleIndex">Индекс поля углов</param>
        /// <param name="auxiliaryList">Список вспомогательных включенных сущностей (датчиков или баков)</param>
        /// <param name="previousVolume">Массив-хранилище объемов по бакам</param>
        /// <param name="capacityAccumulator">Аккумулятор емкости</param>
        /// <param name="arrayListX">Список вычисляемых объемов</param>
        /// <param name="arrayListY">Список вычисляемых емкостей</param>
        public static void TankIn(this ValuablePoint valuablePoint, List<TankModel> tanks,
                                  int angleIndex, List<AuxiliaryEntity> auxiliaryList, double[] previousVolume,
                                  ref double capacityAccumulator, List<double> arrayListX, List<double> arrayListY)
        {
            double volumeAccumulator = 0; // Вводим переменную, накапливающую вычисленные объемы

            // В цикле перебираем все баки
            for (int t = 0; t < tanks.Count; t++)
            {
                if (tanks[t] == valuablePoint.Tank || // Если бак сущности относится в выбранному баку
                    valuablePoint.BasePoint < tanks[t].TarResultList[angleIndex].ResultAxisY[0]
                ) // Если текущая сущность ниже рассматриваемых баков
                {
                    continue; // Продолжаем (переходим к следующему баку)
                }

                if (valuablePoint.BasePoint > tanks[t].TarResultList[angleIndex].ResultAxisY[^1])
                {
                    volumeAccumulator += tanks[t].TankVolume; // Если бак полностью ниже сущности - суммируем его объем
                    continue;
                }

                // Сюда попадаем только, если текущая сущность в пределах перебираемых баков
                // Получаем полный вычисленный объем текущего среза и предыдущего среза
                double sliceTankVolume = tanks[t].GetVolumeSlice(valuablePoint.BasePoint, tanks[t], angleIndex);

                volumeAccumulator += sliceTankVolume;

                // Идем в цикле по всем датчикам сформированного вспомогательного класса
                capacityAccumulator += auxiliaryList.Where(aux => aux.IntrinsicSensor.FuelTank == tanks[t])
                    .Sum(aux => aux.SensorCapacity * aux.IntrinsicSensor.Length *
                        (sliceTankVolume - previousVolume[t]) / aux.DeltaVolume);

                previousVolume[t] = sliceTankVolume;
            }

            arrayListX.Add(Math.Round(volumeAccumulator, cyphers)); // Заносим объем на точку начала зоны действия сущности
            arrayListY.Add(Math.Round(capacityAccumulator, cyphers)); // Заносим накопленную емкость (чтобы не нарушать отчетности)
        }

        /// <summary>
        /// Расчет погонной емкости
        /// Метод выключения бака из работы
        /// </summary>
        /// <param name="valuablePoint">Значимая сущность (точка в датчике или баке)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="angleIndex">Индекс поля углов</param>
        /// <param name="auxiliaryList">Список вспомогательных включенных сущностей (датчиков или баков)</param>
        /// <param name="previousVolume">Массив-хранилище объемов по бакам</param>
        /// <param name="capacityAccumulator">Аккумулятор емкости</param>
        /// <param name="arrayListX">Список вычисляемых объемов</param>
        /// <param name="arrayListY">Список вычисляемых емкостей</param>
        public static void TankOut(this ValuablePoint valuablePoint, List<TankModel> tanks,
                                   int angleIndex, List<AuxiliaryEntity> auxiliaryList, double[] previousVolume,
                                   ref double capacityAccumulator, List<double> arrayListX, List<double> arrayListY)
        {
            double volumeAccumulator = 0; // Вводим переменную, накапливающую вычисленные объемы

            // В цикле перебираем все баки
            for (int t = 0; t < tanks.Count; t++)
            {
                if (tanks[t] == valuablePoint.Tank) // Если бак сущности относится к выбранному баку
                {
                    volumeAccumulator += tanks[t].TankVolume; // Суммируем его объем
                    continue;
                }

                // Если текущая сущность ниже рассматриваемых баков, то продолжаем (переходим к следующей сущности)
                if (valuablePoint.BasePoint < tanks[t].TarResultList[angleIndex].ResultAxisY[0]) continue;

                // Если текущая сущность выше рассматриваемых баков
                if (valuablePoint.BasePoint > tanks[t].TarResultList[angleIndex].ResultAxisY[^1])
                {
                    volumeAccumulator += tanks[t].TankVolume; // Суммируем его объем
                    continue;
                }

                // Если сущность в пределах перебираемого бака, то считаем Пифагором
                // Получаем полный вычисленный объем текущего среза и предыдущего среза
                double sliceTankVolume = tanks[t].GetVolumeSlice(valuablePoint.BasePoint, tanks[t], angleIndex);

                volumeAccumulator += sliceTankVolume;

                // Идем в цикле по всем датчикам сформированного вспомогательного класса
                capacityAccumulator += auxiliaryList
                                       .Where(aux => aux.IntrinsicSensor.FuelTank == tanks[t])
                                       .Sum(aux => aux.SensorCapacity * aux.IntrinsicSensor.Length *
                                                      (sliceTankVolume - previousVolume[t]) / aux.DeltaVolume);

                previousVolume[t] = sliceTankVolume;
            }

            arrayListX.Add(Math.Round(volumeAccumulator, cyphers));   // Заносим объем на точку окончания зоны действия датчика
            arrayListY.Add(Math.Round(capacityAccumulator, cyphers)); // Заносим емкость, накопившуюся на точку среза
        }
        #endregion
    }
}
