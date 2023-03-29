using FuelMeasurement.Tools.Compute.Interfaces;
using FuelMeasurement.Tools.CalculationData.Models;
using NLog;
using System.Collections.Generic;
using System;
using System.Linq;
using FuelMeasurement.Tools.ComputeModule.Interfaces;

namespace FuelMeasurement.Tools.Compute.Implementations
{
    /// <summary>
    /// Расчет МПИ по опорным углам
    /// </summary>
    public class ReferencedController : IReferencedController
    {
        private static ILogger _logger;

        #region Вспомогательные данные
        private CalculationModel Model { get; set; }
        private ICapacityController _capacityController; // Контроллер погонной емкости
        private IErrorsController _errorsController;   // Контроллер МПИ

        /// <summary>
        /// Константа допуска при расчетах
        /// </summary>
        private const double DOUBLE_DELTA = 0.001;

        #region Массивы МПИ для опорных углов
        /// <summary>
        /// Массив объемов для опорных углов
        /// </summary>
        private double[] _errorsReferencedX;
        /// <summary>
        /// Массив погрешностей для опорных углов
        /// </summary>
        private double[] _errorsReferencedY;
        #endregion

        #region Массивы погонной емкости для опорных углов
        /// <summary>
        /// Массив объемов для опорных углов
        /// </summary>
        private double[] _referencedArrX;
        /// <summary>
        /// Массив погонной емкости для опорных углов
        /// </summary>
        private double[] _referencedArrY;
        #endregion
        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        public ReferencedController(ILogger logger)
        {
            _logger = logger;
        }

        #region Блок вычислений МПИ по опорным углам для общего режима
        /// <summary>
        /// Метод формирования массивов МПИ по опорным углам
        /// </summary>
        /// <param name="referencedErrorsListSerieX"></param>
        /// <param name="referencedErrorsListSerieY"></param>
        /// <param name="tanks"></param>
        /// <param name="sensorsCounter"></param>
        /// <param name="sumVolume"></param>
        /// <param name="plane"></param>
        /// <param name="errorsController"></param>
        /// <param name="capacityController"></param>
        public void SetReferencedErrors(CalculationModel model, IErrorsController errorsController, ICapacityController capacityController)
        {
            if (model == null) return;

            Model = model;
            _capacityController = capacityController;
            _errorsController   = errorsController;

            Model.ListSerieX.Clear();
            Model.ListSerieY.Clear();

            Model.ListOfTanksList.Clear();
            Model.ListOfTanksList.Add(Model.SelectedTanks);
            // Определяем массивы точек графиков МПИ и погонной емкости по количеству датчиков и по количеству топливных баков
            // На один датчик - 2 точки
            // На один топливный бак - 2 точки
            // Плюс одна точка для подскока (скачка)
            int arrayLength = Model.ActiveSensorsInCurrentTanks * 3 + Model.SelectedTanks.Count * 2;
            // Определим массивы МПИ для опорных углов
            _errorsReferencedX = new double[arrayLength];
            _errorsReferencedY = new double[arrayLength];
            // Определим массивы погонной емкости для опорных углов
            _referencedArrX = new double[arrayLength];
            _referencedArrY = new double[arrayLength];

            // Метод состоит из четырех частей:
            // 1 - вычисление МПИ для опорных углов с помещением результата в переменные
            // 2 - вычисление погонной емкости для опорных углов
            // 3 - вычисление разницы между измеренными и опорными объемами для емкостей датчиков
            // 4 - суммирование результата с МПИ для опорных углов
            try
            {
                #region Часть 1: Вычисление МПИ для опорных углов
                // Вызовем метод расчета погонной емкости для опорных углов
                _errorsController.ComputeErrorArrays(ref _errorsReferencedX, ref _errorsReferencedY,
                                                     Model.CurrentBranch.AnglesModel.ReferencedPitch, Model.CurrentBranch.AnglesModel.ReferencedRoll,
                                                     Model.ListOfTanksList[^1], Model.SumVolumeOfCurrentTanks);
                // Возвращенные массивы
                // ErrorsReferencedX
                // ErrorsReferencedY
                // содержат в себе данные МПИ для опорных углов
                #endregion

                #region Часть 2: Вычисление погонной емкости для опорных углов
                _capacityController.ComputeCapacityArrays(ref _referencedArrX, ref _referencedArrY,
                                                          Model.CurrentBranch.AnglesModel.ReferencedPitch, Model.CurrentBranch.AnglesModel.ReferencedRoll,
                                                          Model.SelectedTanks);
                // Возвращенные массивы
                // ReferencedArrX
                // ReferencedArrY
                // содержат в себе данные погонной емкости для опорных углов
                #endregion

                // Запускаем цикл по всему полю углов
                foreach (AnglesPair angles in Model.AngleFields)
                {
                    // Определим массивы, для сохранения итогов
                    double[] resultArrayX = new double[arrayLength];
                    double[] resultArrayY = new double[arrayLength];

                    // Определим массивы для вычисления разницы объемов между опорным и текущим режимом углов
                    double[] arrX = new double[arrayLength];
                    double[] arrY = new double[arrayLength];

                    #region Часть 3: Вычисление разницы объемов между опорным и текущим режимами
                    // Получаем массивы погонной емкости для текущих углов
                    _capacityController.ComputeCapacityArrays(ref arrX, ref arrY, angles.Pitch, angles.Roll, Model.SelectedTanks);

                    // Вычисление разницы - результат в массивах: resultArrayX, resultArrayY
                    CountReferencedErrors(arrX, arrY, resultArrayX, resultArrayY, Model.SumVolumeOfCurrentTanks);
                    #endregion

                    #region Часть 4: Прибавление к МПИ по опорным углам дельты по текущим углам
                    for (int z = 0; z < resultArrayX.Length; z++)
                    {
                        resultArrayY[z] += _errorsReferencedY[z];
                    }
                    #endregion

                    Model.ListSerieX.Add(resultArrayX); // Заносим массив оси абсцисс в список
                    Model.ListSerieY.Add(resultArrayY); // Заносим массив оси ординат в список
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка вычислений по опорным углам в общем режиме: {ex}");
            }
        }

        /// <summary>
        /// Метод обновления массивов кривых МПИ для опорных углов в общем режиме
        /// </summary>
        /// <param name="referencedErrorsListSerieX"></param>
        /// <param name="referencedErrorsListSerieY"></param>
        /// <param name="tanks"></param>
        /// <param name="sensorsCounter"></param>
        public void UpdateReferencedErrors()
        {
            if (Model == null) return;

            try
            {
                // Определяем массивы точек графиков МПИ и погонной емкости по количеству датчиков и по количеству топливных баков
                // На один датчик - 2 точки
                // На один топливный бак - 2 точки
                // Плюс одна точка для подскока (скачка)
                int arrayLength = Model.ActiveSensorsInCurrentTanks * 3 + Model.SelectedTanks.Count * 2;

                // Запускаем цикл по всему полю углов
                for (int i = 0; i < Model.AngleFields.Count; i++)
                {
                    // Определим массивы, для сохранения итогов
                    double[] arrX = new double[arrayLength];
                    double[] arrY = new double[arrayLength];

                    var resultArrayX = Model.ListSerieX[i];
                    var resultArrayY = Model.ListSerieY[i];

                    #region Часть 3: Вычисление разницы объемов между опорным и текущим режимами
                    // Получаем массивы погонной емкости для текущих углов
                    _capacityController.ComputeCapacityArrays(ref arrX, ref arrY, Model.AngleFields[i].Pitch, Model.AngleFields[i].Roll, Model.ListOfTanksList[^1]);

                    // Вычисление разницы - результат в массивах: resultArrayX, resultArrayY
                    CountReferencedErrors(arrX, arrY, resultArrayX, resultArrayY, Model.SumVolumeOfCurrentTanks);
                    #endregion

                    #region Часть 4: Прибавление к МПИ по опорным углам дельты по текущим углам
                    for (int z = 0; z < resultArrayX.Length; z++)
                    {
                        resultArrayY[z] += _errorsReferencedY[z];
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка вычислений по опорным углам в общем режиме: {ex}");
            }
        }
        #endregion

        #region Блок вычислений МПИ по опорным углам для режима заправки
        /// <summary>
        /// Метод вычисления МПИ по опорным углам для режима заправки
        /// </summary>
        /// <param name="referencedErrorsListSerieX"></param>
        /// <param name="referencedErrorsListSerieY"></param>
        /// <param name="tanks"></param>
        /// <param name="sensorsCounter"></param>
        /// <param name="sumVolume"></param>
        /// <param name="plane"></param>
        /// <param name="errorsController"></param>
        /// <param name="capacityController"></param>
        public void SetReferencedErrorsIn(CalculationModel model, IErrorsController errorsController, ICapacityController capacityController)
        {
            if(model == null) return;

            Model = model;
            _capacityController = capacityController;
            _errorsController = errorsController;

            try
            {
                // Здесь сначала определим количество групп баков по заправке
                List<TankModel> arrangedTanksGroup = Model.SelectedTanks.OrderBy(item => item.SelectedTankGroupInModel.GroupNumber).ToList();
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
                        int sensorsQuantity = arrangedTanksGroup[index].Sensors.Count(s => s.IsActiveSensor);
                        sensorsAccumulator += sensorsQuantity;
                        index++;
                        if (index == Model.SelectedTanks.Count) break;
                    }
                    Model.ListOfTanksList.Add(tanksList);
                    Model.SensorsInGroup.Add(sensorsAccumulator);
                }

                // Запускаем цикл по всему полю углов - грустно и долго, но задатчиком индексов для списков является поле углов
                foreach (var angles in Model.AngleFields)
                {
                    int arrayLength = Model.ActiveSensorsInCurrentTanks * 3 + Model.SelectedTanks.Count * 2;
                    double[] resultX = new double[arrayLength];
                    double[] resultY = new double[arrayLength];

                    int startGroupIndex = 0;
                    // И два накопителя - для объема предыдущей группы баков и для емкости
                    double volumeAccumulator = 0.0;
                    double errorAccumulator = 0.0;

                    // Запускаем цикл по всем полученным спискам заправляемых баков - для каждой комбинации из поля углов
                    for (int t = 0; t < Model.ListOfTanksList.Count; t++)
                    {
                        // Вычисляем объема баков в группе заправки
                        double groupVolume = Model.ListOfTanksList[t].Aggregate(0.0, (current, tt) => current + tt.TankVolume);

                        // Определяем массивы точек графиков МПИ и погонной емкости по количеству датчиков и по количеству топливных баков
                        // На один датчик - 2 точки
                        // На один топливный бак - 2 точки
                        // Плюс одна точка для подскока (скачка)
                        int groupLength = Model.SensorsInGroup[t] * 3 + Model.ListOfTanksList[t].Count * 2;
                        // Определим массивы МПИ для опорных углов
                        _errorsReferencedX = new double[groupLength];
                        _errorsReferencedY = new double[groupLength];
                        // Определим массивы погонной емкости для опорных углов
                        _referencedArrX = new double[groupLength];
                        _referencedArrY = new double[groupLength];

                        // Метод состоит из четырех частей:
                        // 1 - вычисление МПИ для опорных углов с помещением результата в переменные
                        // 2 - вычисление погонной емкости для опорных углов
                        // 3 - вычисление разницы между измеренными и опорными объемами для емкостей датчиков
                        // 4 - суммирование результата с МПИ для опорных углов

                        #region Часть 1: Вычисление МПИ для опорных углов выбранной группы
                        // Вызовем метод расчета погонной емкости для опорных углов
                        _errorsController.ComputeErrorArrays(ref _errorsReferencedX, ref _errorsReferencedY,
                                                             Model.CurrentBranch.AnglesModel.ReferencedPitch, Model.CurrentBranch.AnglesModel.ReferencedRoll,
                                                             Model.ListOfTanksList[t], groupVolume);
                        // Возвращенные массивы
                        // ErrorsReferencedX
                        // ErrorsReferencedY
                        // содержат в себе данные МПИ для опорных углов
                        #endregion

                        #region Часть 2: Вычисление погонной емкости для опорных углов выбранной группы
                        _capacityController.ComputeCapacityArrays(ref _referencedArrX, ref _referencedArrY,
                                                                  Model.CurrentBranch.AnglesModel.ReferencedPitch, Model.CurrentBranch.AnglesModel.ReferencedRoll,
                                                                  Model.ListOfTanksList[t]);
                        // Возвращенные массивы
                        // ReferencedArrX
                        // ReferencedArrY
                        // содержат в себе данные погонной емкости для опорных углов
                        #endregion

                        // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                        // На один датчик - 2 точки
                        // На один топливный бак - 2 точки
                        // Плюс одна точка для подскока (скачка)
                        double[] arrX = new double[groupLength];
                        double[] arrY = new double[groupLength];

                        // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                        double[] groupArrX = new double[groupLength];
                        double[] groupArrY = new double[groupLength];

                        #region Часть 3: Вычисление разницы объемов между опорным и текущим режимами
                        // Получаем массивы погонной емкости для текущих углов
                        _capacityController.ComputeCapacityArrays(ref arrX, ref arrY, angles.Pitch, angles.Roll, Model.ListOfTanksList[t]);

                        // Вычисление разницы - результат в массивах: groupArrX, groupArrY
                        CountReferencedErrors(arrX, arrY, groupArrX, groupArrY, groupVolume);
                        #endregion

                        #region Часть 4: Прибавление к МПИ по опорным углам дельты по текущим углам
                        for (int res = 0; res < groupLength; res++)
                        {
                            groupArrY[res] += _errorsReferencedY[res];

                            resultX[res + startGroupIndex] = groupArrX[res] + volumeAccumulator;
                            resultY[res + startGroupIndex] = groupArrY[res] + errorAccumulator;
                        }
                        startGroupIndex += groupLength;
                        volumeAccumulator += groupArrX[^1];
                        errorAccumulator += groupArrY[^1];
                        #endregion
                    }

                    Model.ListSerieX.Add(resultX); // Заносим массив оси абсцисс в список
                    Model.ListSerieY.Add(resultY); // Заносим массив оси ординат в список
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка вычислений по опорным углам в режиме заправки: {ex}");
            }
        }

        /// <summary>
        /// Метод обновления МПИ по опорным углам для режима заправки
        /// </summary>
        /// <param name="referencedErrorsListSerieX"></param>
        /// <param name="referencedErrorsListSerieY"></param>
        /// <param name="tanks"></param>
        /// <param name="sensorsCounter"></param>
        public void UpdateReferencedErrorsIn()
        {
            if (Model == null) return;

            try
            {
                // Здесь сначала определим количество групп баков по заправке
                List<TankModel> arrangedTanksGroup = Model.SelectedTanks.OrderBy(item => item.SelectedTankGroupInModel.GroupNumber).ToList();
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
                        int sensorsQuantity = arrangedTanksGroup[index].Sensors.Count(t => t.IsActiveSensor);
                        sensorsAccumulator += sensorsQuantity;
                        index++;
                        if (index == Model.SelectedTanks.Count) break;
                    }
                    Model.ListOfTanksList.Add(tanksList);
                    Model.SensorsInGroup.Add(sensorsAccumulator);
                }

                // Запускаем цикл по всему полю углов
                for (int i = 0; i < Model.AngleFields.Count; i++)
                {
                    int startGroupIndex = 0;
                    // И два накопителя - для объема предыдущей группы баков и для емкости
                    double volumeAccumulator = 0.0;
                    double errorAccumulator = 0.0;

                    if (Model.ListOfTanksList.Count > 0)
                    {
                        // Запускаме цикл по всем полученным спискам заправляемых баков - для каждой комбинации из поля углов
                        for (int t = 0; t < Model.ListOfTanksList.Count; t++)
                        {
                            // Вычисляем объема баков в группе заправки
                            double groupVolume = Model.ListOfTanksList[t].Aggregate(0.0, (current, tt) => current + tt.TankVolume);

                            // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                            // На один датчик - 2 точки
                            // На один топливный бак - 2 точки
                            // Плюс одна точка для подскока (скачка)
                            int groupLength = Model.SensorsInGroup[t] * 3 + Model.ListOfTanksList[t].Count * 2;
                            double[] groupArrX = new double[groupLength];
                            double[] groupArrY = new double[groupLength];

                            // Определим массивы, для сохранения итогов
                            double[] arrX = new double[groupLength];
                            double[] arrY = new double[groupLength];

                            // Определим массивы МПИ для опорных углов
                            _errorsReferencedX = new double[groupLength];
                            _errorsReferencedY = new double[groupLength];
                            // Определим массивы погонной емкости для опорных углов
                            _referencedArrX = new double[groupLength];
                            _referencedArrY = new double[groupLength];

                            #region Часть 1: Вычисление МПИ для опорных углов выбранной группы
                            // Вызовем метод расчета погонной емкости для опорных углов
                            _errorsController.ComputeErrorArrays(ref _errorsReferencedX, ref _errorsReferencedY,
                                                             Model.CurrentBranch.AnglesModel.ReferencedPitch, Model.CurrentBranch.AnglesModel.ReferencedRoll,
                                                             Model.ListOfTanksList[t], groupVolume);
                            // Возвращенные массивы
                            // ErrorsReferencedX
                            // ErrorsReferencedY
                            // содержат в себе данные МПИ для опорных углов
                            #endregion

                            #region Часть 2: Вычисление погонной емкости для опорных углов выбранной группы
                            _capacityController.ComputeCapacityArrays(ref _referencedArrX, ref _referencedArrY,
                                                                  Model.CurrentBranch.AnglesModel.ReferencedPitch, Model.CurrentBranch.AnglesModel.ReferencedRoll,
                                                                  Model.ListOfTanksList[t]);
                            // Возвращенные массивы
                            // ReferencedArrX
                            // ReferencedArrY
                            // содержат в себе данные погонной емкости для опорных углов
                            #endregion

                            #region Часть 3: Вычисление разницы объемов между опорным и текущим режимами
                            // Получаем массивы погонной емкости для текущих углов
                            _capacityController.ComputeCapacityArrays(ref arrX, ref arrY, Model.AngleFields[i].Pitch, Model.AngleFields[i].Roll, Model.ListOfTanksList[t]);

                            // Вычисление разницы - результат в массивах: groupArrX, groupArrY
                            CountReferencedErrors(arrX, arrY, groupArrX, groupArrY, groupVolume);
                            #endregion

                            #region Часть 4: Прибавление к МПИ по опорным углам дельты по текущим углам
                            for (int res = 0; res < groupLength; res++)
                            {
                                groupArrY[res] += _errorsReferencedY[res];

                                Model.ListSerieX[i][res + startGroupIndex] = groupArrX[res] + volumeAccumulator;
                                Model.ListSerieY[i][res + startGroupIndex] = groupArrY[res] + errorAccumulator;
                            }
                            startGroupIndex += groupLength;
                            volumeAccumulator += groupArrX[^1];
                            errorAccumulator += groupArrY[^1];
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка вычислений по опорным углам в режиме заправки: {ex}");
            }
        }
        #endregion

        #region Блок вычислений МПИ по опорным углам для режима выработки
        /// <summary>
        /// Метод вычисления МПИ по опорным углам для режима выработки
        /// </summary>
        /// <param name="referencedErrorsListSerieX"></param>
        /// <param name="referencedErrorsListSerieY"></param>
        /// <param name="tanks"></param>
        /// <param name="sensorsCounter"></param>
        /// <param name="sumVolume"></param>
        /// <param name="plane"></param>
        /// <param name="errorsController"></param>
        /// <param name="capacityController"></param>
        public void SetReferencedErrorsOut(CalculationModel model, IErrorsController errorsController, ICapacityController capacityController)
        {
            if (model == null) return;

            Model = model;
            _capacityController = capacityController;
            _errorsController = errorsController;

            try
            {
                // Здесь сначала определим количество групп баков по заправке
                List<TankModel> arrangedTanksGroup = Model.SelectedTanks.OrderBy(item => item.SelectedTankGroupInModel.GroupNumber).ToList();
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
                        int sensorsQuantity = arrangedTanksGroup[index].Sensors.Count(s => s.IsActiveSensor);
                        sensorsAccumulator += sensorsQuantity;
                        index++;
                        if (index == Model.SelectedTanks.Count) break;
                    }
                    Model.ListOfTanksList.Add(tanksList);
                    Model.SensorsInGroup.Add(sensorsAccumulator);
                }

                // Запускаем цикл по всему полю углов - грустно и долго, но задатчиком индексов для списков является поле углов
                foreach (AnglesPair angle in Model.AngleFields)
                {
                    int arrayLength = Model.ActiveSensorsInCurrentTanks * 3 + Model.SelectedTanks.Count * 2;
                    double[] resultX = new double[arrayLength];
                    double[] resultY = new double[arrayLength];

                    int startGroupIndex = 0;
                    // И два накопителя - для объема предыдущей группы баков и для емкости
                    double volumeAccumulator = 0.0;
                    double errorAccumulator = 0.0;

                    // Запускаем цикл по всем полученным спискам заправляемых баков - для каждой комбинации из поля углов
                    for (int t = 0; t < Model.ListOfTanksList.Count; t++)
                    {
                        // Вычисляем объема баков в группе заправки
                        double groupVolume = Model.ListOfTanksList[t].Aggregate(0.0, (current, tt) => current + tt.TankVolume);

                        // Определяем массивы точек графиков МПИ и погонной емкости по количеству датчиков и по количеству топливных баков
                        // На один датчик - 2 точки
                        // На один топливный бак - 2 точки
                        // Плюс одна точка для подскока (скачка)
                        int groupLength = Model.SensorsInGroup[t] * 3 + Model.ListOfTanksList[t].Count * 2;
                        // Определим массивы МПИ для опорных углов
                        _errorsReferencedX = new double[groupLength];
                        _errorsReferencedY = new double[groupLength];
                        // Определим массивы погонной емкости для опорных углов
                        _referencedArrX = new double[groupLength];
                        _referencedArrY = new double[groupLength];

                        // Метод состоит из четырех частей:
                        // 1 - вычисление МПИ для опорных углов с помещением результата в переменные
                        // 2 - вычисление погонной емкости для опорных углов
                        // 3 - вычисление разницы между измеренными и опорными объемами для емкостей датчиков
                        // 4 - суммирование результата с МПИ для опорных углов

                        #region Часть 1: Вычисление МПИ для опорных углов выбранной группы
                        // Вызовем метод расчета погонной емкости для опорных углов
                        _errorsController.ComputeErrorArrays(ref _errorsReferencedX, ref _errorsReferencedY,
                                                             Model.CurrentBranch.AnglesModel.ReferencedPitch, Model.CurrentBranch.AnglesModel.ReferencedRoll,
                                                             Model.ListOfTanksList[t], groupVolume);
                        // Возвращенные массивы
                        // ErrorsReferencedX
                        // ErrorsReferencedY
                        // содержат в себе данные МПИ для опорных углов
                        #endregion

                        #region Часть 2: Вычисление погонной емкости для опорных углов выбранной группы
                        _capacityController.ComputeCapacityArrays(ref _referencedArrX, ref _referencedArrY,
                                                                  Model.CurrentBranch.AnglesModel.ReferencedPitch, Model.CurrentBranch.AnglesModel.ReferencedRoll,
                                                                  Model.ListOfTanksList[t]);
                        // Возвращенные массивы
                        // ReferencedArrX
                        // ReferencedArrY
                        // содержат в себе данные погонной емкости для опорных углов
                        #endregion

                        // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                        // На один датчик - 2 точки
                        // На один топливный бак - 2 точки
                        // Плюс одна точка для подскока (скачка)
                        double[] arrX = new double[groupLength];
                        double[] arrY = new double[groupLength];

                        // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                        double[] groupArrX = new double[groupLength];
                        double[] groupArrY = new double[groupLength];

                        #region Часть 3: Вычисление разницы объемов между опорным и текущим режимами
                        // Получаем массивы погонной емкости для текущих углов
                        _capacityController.ComputeCapacityArrays(ref arrX, ref arrY, angle.Pitch, angle.Roll, Model.ListOfTanksList[t]);

                        // Вычисление разницы - результат в массивах: groupArrX, groupArrY
                        CountReferencedErrors(arrX, arrY, groupArrX, groupArrY, groupVolume);
                        #endregion

                        #region Часть 4: Прибавление к МПИ по опорным углам дельты по текущим углам
                        for (int res = 0; res < groupLength; res++)
                        {
                            groupArrY[res] += _errorsReferencedY[res];

                            resultX[res + startGroupIndex] = groupArrX[res] + volumeAccumulator;
                            resultY[res + startGroupIndex] = groupArrY[res] + errorAccumulator;
                        }
                        startGroupIndex += groupLength;
                        volumeAccumulator += groupArrX[^1];
                        errorAccumulator += groupArrY[^1];
                        #endregion
                    }

                    #region Смена знака в массиве
                    int len = resultX.Length;
                    // Меняем знак значений по оси абсцисс для отрисовки графика наоборот
                    for (int inv = 0; inv < len; inv++)
                    {
                        resultX[inv] = -resultX[inv];
                    }
                    #endregion

                    Model.ListSerieX.Add(resultX); // Заносим массив оси абсцисс в список
                    Model.ListSerieY.Add(resultY); // Заносим массив оси ординат в список
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка вычислений по опорным углам в режиме выработки: {ex}");
            }
        }

        /// <summary>
        /// Метод обновления массивов МПИ по опорным углам в режиме выработки
        /// </summary>
        public void UpdateReferencedErrorsOut()
        {
            if (Model == null) return;

            try
            {
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
                        int sensorsQuantity = 0;
                        foreach (var s in arrangedTanksGroup[index].Sensors)
                        {
                            if (s.IsActiveSensor) sensorsQuantity++;
                        }
                        sensorsAccumulator += sensorsQuantity;
                        index++;
                        if (index == Model.SelectedTanks.Count) break;
                    }
                    Model.ListOfTanksList.Add(tanksList);
                    Model.SensorsInGroup.Add(sensorsAccumulator);
                }

                // Запускаем цикл по всему полю углов
                for (int i = 0; i < Model.AngleFields.Count; i++)
                {
                    int startGroupIndex = 0;
                    // И два накопителя - для объема предыдущей группы баков и для емкости
                    double volumeAccumulator = 0.0;
                    double errorAccumulator = 0.0;

                    if (Model.ListOfTanksList.Count > 0)
                    {
                        // Запускаме цикл по всем полученным спискам заправляемых баков - для каждой комбинации из поля углов
                        for (int t = 0; t < Model.ListOfTanksList.Count; t++)
                        {
                            // Вычисляем объема баков в группе заправки
                            double groupVolume = Model.ListOfTanksList[t].Aggregate(0.0, (current, tt) => current + tt.TankVolume);

                            // Определяем массивы точек графика МПИ по количеству датчиков и по количеству топливных баков
                            // На один датчик - 2 точки
                            // На один топливный бак - 2 точки
                            // Плюс одна точка для подскока (скачка)
                            int groupLength = Model.SensorsInGroup[t] * 3 + Model.ListOfTanksList[t].Count * 2;
                            double[] groupArrX = new double[groupLength];
                            double[] groupArrY = new double[groupLength];

                            // Определим массивы, для сохранения итогов
                            double[] arrX = new double[groupLength];
                            double[] arrY = new double[groupLength];

                            // Определим массивы МПИ для опорных углов
                            _errorsReferencedX = new double[groupLength];
                            _errorsReferencedY = new double[groupLength];
                            // Определим массивы погонной емкости для опорных углов
                            _referencedArrX = new double[groupLength];
                            _referencedArrY = new double[groupLength];

                            #region Часть 1: Вычисление МПИ для опорных углов выбранной группы
                            // Вызовем метод расчета погонной емкости для опорных углов
                            _errorsController.ComputeErrorArrays(ref _errorsReferencedX, ref _errorsReferencedY,
                                                                 Model.CurrentBranch.AnglesModel.ReferencedPitch, Model.CurrentBranch.AnglesModel.ReferencedRoll,
                                                                 Model.ListOfTanksList[t], groupVolume);
                            // Возвращенные массивы
                            // ErrorsReferencedX
                            // ErrorsReferencedY
                            // содержат в себе данные МПИ для опорных углов
                            #endregion

                            #region Часть 2: Вычисление погонной емкости для опорных углов выбранной группы
                            _capacityController.ComputeCapacityArrays(ref _referencedArrX, ref _referencedArrY,
                                                                      Model.CurrentBranch.AnglesModel.ReferencedPitch, Model.CurrentBranch.AnglesModel.ReferencedRoll,
                                                                      Model.ListOfTanksList[t]);
                            // Возвращенные массивы
                            // ReferencedArrX
                            // ReferencedArrY
                            // содержат в себе данные погонной емкости для опорных углов
                            #endregion

                            #region Часть 3: Вычисление разницы объемов между опорным и текущим режимами
                            // Получаем массивы погонной емкости для текущих углов
                            _capacityController.ComputeCapacityArrays(ref arrX, ref arrY, Model.AngleFields[i].Pitch, Model.AngleFields[i].Roll, Model.ListOfTanksList[t]);

                            // Вычисление разницы - результат в массивах: groupArrX, groupArrY
                            CountReferencedErrors(arrX, arrY, groupArrX, groupArrY, groupVolume);
                            #endregion

                            #region Часть 4: Прибавление к МПИ по опорным углам дельты по текущим углам
                            int len = groupArrX.Length;
                            // Меняем знак значений по оси абсцисс для отрисовки графика наоборот
                            for (int inv = 0; inv < len; inv++)
                            {
                                groupArrX[inv] = -groupArrX[inv];
                            }

                            for (int res = 0; res < groupLength; res++)
                            {
                                groupArrY[res] += _errorsReferencedY[res];

                                Model.ListSerieX[i][res + startGroupIndex] = groupArrX[res] + volumeAccumulator;
                                Model.ListSerieY[i][res + startGroupIndex] = groupArrY[res] + errorAccumulator;
                            }
                            startGroupIndex += groupLength;
                            volumeAccumulator += groupArrX[^1];
                            errorAccumulator += groupArrY[^1];
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка вычислений по опорным углам в режиме выработки: {ex}");
            }
        }
        #endregion

        /// <summary>
        /// Метод вычисления разницы в объемах и приведение к погрешности
        /// </summary>
        /// <param name="arrX">Массив по оси абсцисс (объем) для текущих углов</param>
        /// <param name="arrY">Массив по оси ординат (емкость) для текущих углов</param>
        /// <param name="resultArrayX">Результирующий массив по оси абсцисс (объем)</param>
        /// <param name="resultArrayY">Результирующий массив по оси ординат (погрешность)</param>
        /// <param name="tanksVolume">Объем топливных баков</param>
        private void CountReferencedErrors(double[] arrX, double[] arrY, double[] resultArrayX, double[] resultArrayY, double tanksVolume)
        {
            // Имеем:
            // 1 - массивы arrX и arrY - погонная емкость для текущих углов - измеренная сущность
            // 2 - массивы ReferencedArrX и ReferencedArrY - погонная емкость для опорных углов - опорная сущность

            // В цикле бежим по измеренным сущностям
            for (int s = 1; s < arrX.Length; s++) // Начинаем с первого индекса, так как нулевой всегда ноль
            {
                if (Math.Abs(arrY[s] - _referencedArrY[s]) < DOUBLE_DELTA) // В случае, когда измеренная емкость равна опорной
                {
                    resultArrayX[s] = arrX[s]; // В результат записываем измеренный объем
                    resultArrayY[s] = (arrX[s] - _referencedArrX[s]) / tanksVolume; // В дельту записываем разность между измеренным и опорным объемами в %
                }
                else if (arrY[s] > _referencedArrY[s]) // Измеренная емкость больше опорной
                {
                    // Нужно найти две точки на опорной кривой:
                    // 1 - точка с емкостью больше измеренной (первая, которая больше)
                    // 2 - точка с емкостью меньше измеренной (первая, которая меньше)
                    // Так как измеренная емкость больше опорной, то ищем сначала верхнюю (большую)
                    // Для этого в цикле перебираем опорный массив, начиная с текущего индекса
                    int upIndex = _referencedArrY.Length - 2;
                    int downIndex = 1;
                    for (int idx = s + 1; idx < _referencedArrY.Length; idx++) // Ищем верхний индекс
                    {
                        if (!(_referencedArrY[idx] > arrY[s])) continue;

                        upIndex = idx; // Индекс верхней точки опоры
                        break; // Прерываем цикл
                    }

                    for (int idx = upIndex; idx > 0; idx--) // Ищем нижний индекс
                    {
                        if (!(_referencedArrY[idx] < arrY[s])) continue;

                        downIndex = idx; // Индекс нижней опоры
                        break; // Прерываем цикл
                    }

                    // Теперь сосчитаем коэффициент
                    double k = (arrY[s] - _referencedArrY[downIndex]) / (_referencedArrY[upIndex] - _referencedArrY[downIndex]);
                    // Получим дельту
                    double delta = (_referencedArrX[upIndex] - _referencedArrX[downIndex]) * k;
                    // Получим значение опорного объема
                    double referencedVolume = _referencedArrX[downIndex] + delta;

                    // Значение разности между измеренным и опорным режимами будет следующее (%)
                    resultArrayY[s] = (arrX[s] - referencedVolume) / tanksVolume;
                    resultArrayX[s] = arrX[s]; // А объем равен измеренному
                }
                else // Измеренная емкость меньше опорной
                {
                    // Нужно найти две точки на опорной кривой:
                    // 1 - точка с емкостью меньше измеренной (первая, которая меньше)
                    // 2 - точка с емкостью больше измеренной (первая, которая больше)
                    // Так как измеренная емкость меньше опорной, то ищем сначала нижнюю (меньшую)
                    // Для этого в цикле перебираем опорный массив, начиная с текущего индекса
                    int upIndex = _referencedArrY.Length - 2;
                    int downIndex = 1;
                    for (int idx = s - 1; idx > 0; idx--)
                    {
                        if (!(_referencedArrY[idx] < arrY[s])) continue;

                        downIndex = idx; // Индекс нижней опоры
                        break; // Прерываем цикл
                    }

                    for (int idx = downIndex; idx < _referencedArrY.Length; idx++)
                    {
                        if (!(_referencedArrY[idx] > arrY[s])) continue;

                        upIndex = idx; // Индекс верхней точки опоры
                        break; // Прерываем цикл
                    }

                    // Теперь сосчитаем коэффициент
                    double k = (arrY[s] - _referencedArrY[downIndex]) / (_referencedArrY[upIndex] - _referencedArrY[downIndex]);
                    // Получим дельту
                    double delta = (_referencedArrX[upIndex] - _referencedArrX[downIndex]) * k;
                    // Получим значение опорного объема
                    double referencedVolume = _referencedArrX[downIndex] + delta;

                    // Значение разности между измеренным и опорным режимами будет следующее (%)
                    resultArrayY[s] = (arrX[s] - referencedVolume) / tanksVolume;
                    resultArrayX[s] = arrX[s]; // А объем равен измеренному
                }
            }
        }
    }
}
