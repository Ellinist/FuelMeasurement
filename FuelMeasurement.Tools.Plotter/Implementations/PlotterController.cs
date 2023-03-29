using FuelMeasurement.Tools.CalculationData.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using FuelMeasurement.Tools.Plotter.Interfaces;
using FuelMeasurement.Tools.Plotter.Graph;
using FuelMeasurement.Tools.Plotter.Graph.Plot;
using Prism.Mvvm;
using ScottPlot;
using NLog;
using FuelMeasurement.Tools.Compute.Interfaces;
using ScottPlot.Plottable;

namespace FuelMeasurement.Tools.Plotter.Implementations
{
    /// <summary>
    /// Класс, реализующий построение кривых самых разнообразных графиков
    /// </summary>
    public class PlotterController : BindableBase, IPlotterController
    {
        private const double Delta = 0.01; // Допустимая погрешность
        private bool _relative;
        private readonly ILogger _logger;
        private readonly IMeasureController _measureController;
        private readonly IErrorsController _errorsController;
        private readonly ICapacityController _capacityController;

        /// <summary>
        /// Текущая кривая (по выбранным углам) МПИ
        /// </summary>
        public ScatterPlot CurrentErrorSerie { get; set; }
        /// <summary>
        /// Текущая кривая погонной емкости (по выбранным углам)
        /// </summary>
        public ScatterPlot CurrentCapacitySerie { get; set; }

        /// <summary>
        /// Класс тарировочной кривой
        /// </summary>
        private CalibrationGraph CalibrationGraph { get; set; } = new CalibrationGraph();

        /// <summary>
        /// Свойство - результирующий массив - для графика суммарной тарировочной кривой
        /// [0, i] - ордината;
        /// [1, i] - абсцисса
        /// </summary>
        private double[,] SummaryArray { get; set; }

        /// <summary>
        /// Ширина датчика на графике
        /// По умолчанию задаю 20 пикселей - потом включить в справочник (настройки)
        /// </summary>
        public double SensorWidth { get; set; } = 24;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="measureController"></param>
        /// <param name="errorsController"></param>
        public PlotterController(ILogger logger, IMeasureController measureController,
                                 IErrorsController errorsController, ICapacityController capacityController
                                 )
        {
            _logger             = logger ?? throw new ArgumentNullException(nameof(logger));
            _measureController  = measureController ?? throw new ArgumentNullException(nameof(measureController));
            _errorsController   = errorsController ?? throw new ArgumentNullException(nameof(errorsController));
            _capacityController = capacityController ?? throw new ArgumentNullException(nameof(capacityController));
        }

        /// <summary>
        /// Метод формирования (начального) графика МПИ
        /// Вызывается при добавлении или удалении датчика, при переключении вкладок графиков
        /// </summary>
        /// <param name="model"></param>
        /// <param name="plot"></param>
        /// <param name="tanks"></param>
        public void SetErrorsCurves(CalculationModel model, WpfPlot plot, List<TankModel> tanks, double pitch, double roll) // Здесь добавить тип графика - заправка выработка ...
        {
            plot.Plot.Clear(); // Очистка графика ScottPlot
            // Измерение неизмеряемых и измеряемых объемов
            _measureController.CalcSensorsMeasures(model.SelectedTanks);

            _errorsController.Model = model;

            // Вот этот свитч будет выбирать, какой тип графика отрисовывается
            switch (true)
            {
                case true:
                    plot.Plot.XAxis.TickLabelNotation(invertSign: false);
                    _ = plot.Plot.YAxis.Label("МПИ для общего режима", color: System.Drawing.Color.Black, size: 16, fontName: "Arial", bold: true);
                    _errorsController.ComputeTankErrors(model);
                    break;
                case false:
                    break;
            }

            // Блок подготовки к изменению цвета линий МПИ
            double pitchDelta;
            if (Math.Abs(model.CurrentBranch.AnglesModel.MinPitch) > Math.Abs(model.CurrentBranch.AnglesModel.MaxPitch))
            {
                pitchDelta = 255 / Math.Abs(model.CurrentBranch.AnglesModel.MinPitch);
            }
            else
            {
                pitchDelta = 255 / Math.Abs(model.CurrentBranch.AnglesModel.MaxPitch);
            }

            double rollDelta;
            if (Math.Abs(model.CurrentBranch.AnglesModel.MinRoll) > Math.Abs(model.CurrentBranch.AnglesModel.MaxRoll))
            {
                rollDelta = 255 / Math.Abs(model.CurrentBranch.AnglesModel.MinRoll);
            }
            else
            {
                rollDelta = 255 / Math.Abs(model.CurrentBranch.AnglesModel.MaxRoll);
            }

            for (var i = 0; i < model.AngleFields.Count; i++)
            {
                #region Этот блок очень странный предмет - то он есть, то его нет!!!
                int parameter =
                  (int)(Math.Abs(model.AngleFields[i].Pitch) * pitchDelta +
                        Math.Abs(model.AngleFields[i].Roll) * rollDelta);
                int deltaG = 255 - parameter / 2; // Приращение по зеленому цвету
                int deltaB = deltaG < 155 ? 155 - deltaG : 0; // Приращение по синему цвету
                #endregion

                plot.Plot.AddScatter(model.ListSerieX[i], model.ListSerieY[i], System.Drawing.Color.FromArgb(0, deltaG, deltaB), 0.01f, markerShape: MarkerShape.none);
                if (Math.Abs(model.AngleFields[i].Pitch - pitch) < Delta && Math.Abs(model.AngleFields[i].Roll - roll) < Delta)
                {
                    CurrentErrorSerie = new ScatterPlot(model.ListSerieX[^1], model.ListSerieY[^1]);
                }
            }

            _ = plot.Plot.XAxis.Label(@"Объем бака/баков, (литров)", color: System.Drawing.Color.Indigo, size: 16, fontName: "Arial", bold: true);
            plot.Plot.XAxis.MajorGrid(enable: true, color: System.Drawing.Color.Black, lineStyle: LineStyle.Dot);
            plot.Plot.YAxis.MajorGrid(enable: true, color: System.Drawing.Color.Black, lineStyle: LineStyle.Dot);
            plot.Plot.YAxis.TickLabelFormat("P2", dateTimeFormat: false);

            // Определяем индекс записи по заданным углам
            // Это необходимо, чтобы отрисовать одну единственную кривую, соответствующую выбранным углам
            int angleIndex = model.AngleFields.FindIndex(p => Math.Abs(p.Pitch - pitch) < Delta && Math.Abs(p.Roll - roll) < Delta);

            if (angleIndex == -1) return;

            // Выводим линию оси абсцисс
            plot.Plot.AddHorizontalLine(0, System.Drawing.Color.Black, 0.1F, LineStyle.Solid);

            plot.Plot.Remove(CurrentErrorSerie); // Удаляем предыдущий вариант выбранной серии
            CurrentErrorSerie = plot.Plot.AddScatter(model.ListSerieX[angleIndex], model.ListSerieY[angleIndex]);
            CurrentErrorSerie.Color = System.Drawing.Color.Red;
            CurrentErrorSerie.LineWidth = 3;
            CurrentErrorSerie.MarkerSize = 4;
            CurrentErrorSerie.MarkerShape = MarkerShape.openCircle;

            plot.Plot.AxisAuto(); // Если включен режим "Нет масштабам" - то не масштабируем
            plot.Render();
        }

        /// <summary>
        /// Получение ближайших точек к текущей кривой МПИ
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseY"></param>
        /// <returns></returns>
        public (double pointX, double pointY, int pointIndex) GetErrorNearest(double mouseX, double mouseY) => CurrentErrorSerie.GetPointNearest(mouseX, mouseY);

        /// <summary>
        /// Получение значений объема и ошибки для выбранной точки
        /// </summary>
        /// <param name="pointIndex"></param>
        /// <returns></returns>
        public (double nearestVolume, double nearestError) GetErrorSeriePoints(int pointIndex) => (CurrentErrorSerie.Xs[pointIndex], CurrentErrorSerie.Ys[pointIndex]);

        /// <summary>
        /// Получение ближайших точек к текущей кривой погонной емкости
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseY"></param>
        /// <returns></returns>
        public (double pointX, double pointY, int pointIndex) GetCapacityNearest(double mouseX, double mouseY) => CurrentCapacitySerie.GetPointNearest(mouseX, mouseY);

        /// <summary>
        /// Получение значений объема и емкости для выбранной точки
        /// </summary>
        /// <param name="pointIndex"></param>
        /// <returns></returns>
        public (double nearestCapacityVolume, double nearestCapacity) GetCapacitySeriePoints(int pointIndex) => (CurrentCapacitySerie.Xs[pointIndex], CurrentCapacitySerie.Ys[pointIndex]);

        /// <summary>
        /// Метод обновления МПИ
        /// </summary>
        /// <param name="model"></param>
        /// <param name="plot"></param>
        /// <param name="tanks"></param>
        public void UpdateErrorsCurves(CalculationModel model, WpfPlot plot, List<TankModel> tanks, double pitch, double roll)
        {
            // Считаем количество активных (включенных) датчиков
            int sensorsCounter = model.SelectedTanks.Sum(tank => tank.Sensors.Count(sensor => sensor.IsActiveSensor));

            // Измерение неизмеряемых и измеряемых объемов
            _measureController.CalcSensorsMeasures(model.SelectedTanks);

            double sumVolume = 0.0;
            foreach (TankModel tank in model.SelectedTanks)
            {
                sumVolume += tank.TankVolume;
                //model.SelectedTanks.Add(tank);
            }

            _errorsController.Model = model;

            _errorsController.UpdateTankErrors();

            plot.Plot.XAxis.MajorGrid(enable: true, color: System.Drawing.Color.Black, lineStyle: LineStyle.Dot);
            plot.Plot.YAxis.MajorGrid(enable: true, color: System.Drawing.Color.Black, lineStyle: LineStyle.Dot);
            plot.Plot.YAxis.TickLabelFormat("P2", dateTimeFormat: false);

            // Определяем индекс записи по заданным углам
            // Это необходимо, чтобы отрисовать одну единственную кривую, соответствующую выбранным углам
            int angleIndex = model.AngleFields.FindIndex(p => Math.Abs(p.Pitch - pitch) < Delta && Math.Abs(p.Roll - roll) < Delta);

            if (angleIndex == -1) return;

            // Выводим линию оси абсцисс
            plot.Plot.AddHorizontalLine(0, System.Drawing.Color.Black, 0.1F, LineStyle.Solid);

            plot.Plot.Remove(CurrentErrorSerie); // Удаляем предыдущий вариант выбранной серии
            CurrentErrorSerie = plot.Plot.AddScatter(model.ListSerieX[angleIndex], model.ListSerieY[angleIndex]);
            CurrentErrorSerie.Color = System.Drawing.Color.Red;
            CurrentErrorSerie.LineWidth = 3;
            CurrentErrorSerie.MarkerSize = 4;
            CurrentErrorSerie.MarkerShape = MarkerShape.openCircle;

            plot.Plot.AxisAuto(); // Если включен режим "Нет масштабам" - то не масштабируем
            plot.RenderRequest(RenderType.HighQuality);
        }

        public void SetCapacityCurves(CalculationModel model, WpfPlot plot, double pitch, double roll)
        {
            plot.Plot.Clear(); // Очистка графика ScottPlot
            // Измерение неизмеряемых и измеряемых объемов
            _measureController.CalcSensorsMeasures(model.SelectedTanks);

            _capacityController.Model = model;

            // Вот этот свитч будет выбирать, какой тип графика отрисовывается
            switch (true)
            {
                case true:
                    plot.Plot.XAxis.TickLabelNotation(invertSign: false);
                    plot.Plot.YAxis.Label("Емкость (Общий режим), (пФ)", color: System.Drawing.Color.Black, size: 16, fontName: "Arial", bold: true);
                    _capacityController.SetSensorsCapacity(model);
                    break;
                case false:
                    break;
            }

            _ = plot.Plot.XAxis.Label(@"Объем бака/баков, (литров)", color: System.Drawing.Color.Indigo, size: 16, fontName: "Arial", bold: true);
            plot.Plot.XAxis.MajorGrid(enable: true, color: System.Drawing.Color.Black, lineStyle: LineStyle.Dot);
            plot.Plot.YAxis.MajorGrid(enable: true, color: System.Drawing.Color.Black, lineStyle: LineStyle.Dot);
            plot.Plot.YAxis.TickLabelFormat("F2", dateTimeFormat: false);

            #region Блок подготовки к изменению цветов линий МПИ
            // Блок подготовки к изменению цвета линий МПИ
            double pitchDelta = Math.Abs(model.CurrentBranch.AnglesModel.MinPitch) > Math.Abs(model.CurrentBranch.AnglesModel.MaxPitch) ? 255 
                              / Math.Abs(model.CurrentBranch.AnglesModel.MinPitch) : 255 / Math.Abs(model.CurrentBranch.AnglesModel.MaxPitch);
            double rollDelta = Math.Abs(model.CurrentBranch.AnglesModel.MinRoll) > Math.Abs(model.CurrentBranch.AnglesModel.MaxRoll) ? 255 
                              / Math.Abs(model.CurrentBranch.AnglesModel.MinRoll) : 255 / Math.Abs(model.CurrentBranch.AnglesModel.MaxRoll);
            #endregion

            #region Цикл заполнения кривыми графика МПИ
            // В цикле заполняем кривые графика МПИ
            for (var i = 0; i < model.AngleFields.Count; i++)
            {
                #region Этот блок очень странный предмет - то он есть, то его нет!!!
                int parameter = (int)(Math.Abs(model.AngleFields[i].Pitch) * pitchDelta +
                                       Math.Abs(model.AngleFields[i].Roll) * rollDelta);
                int deltaG = 255 - parameter / 2; // Приращение по зеленому цвету
                int deltaB = deltaG < 155 ? 155 - deltaG : 0; // Приращение по синему цвету
                #endregion

                _ = plot.Plot.AddScatter(model.ListSerieX[i], model.ListSerieY[i], System.Drawing.Color.FromArgb(0, deltaG, deltaB), 0.01f, markerShape: MarkerShape.none);

                if (Math.Abs(model.AngleFields[i].Pitch - pitch) < Delta && Math.Abs(model.AngleFields[i].Roll - roll) < Delta)
                {
                    CurrentCapacitySerie = new ScatterPlot(model.ListSerieX[^1], model.ListSerieY[^1]);
                }
            }

            // Определяем индекс записи по заданным углам
            // Это необходимо, чтобы отрисовать одну единственную кривую, соответствующую выбранным углам
            int angleIndex = model.AngleFields.FindIndex(p => Math.Abs(p.Pitch - pitch) < Delta && Math.Abs(p.Roll - roll) < Delta);

            if (angleIndex == -1) return;

            // Выводим линию оси абсцисс
            plot.Plot.AddHorizontalLine(0, System.Drawing.Color.Black, 0.1F, LineStyle.Solid);

            plot.Plot.Remove(CurrentCapacitySerie); // Удаляем предыдущий вариант выбранной серии
            CurrentCapacitySerie = plot.Plot.AddScatter(model.ListSerieX[angleIndex], model.ListSerieY[angleIndex]);
            CurrentCapacitySerie.Color = System.Drawing.Color.Red;
            CurrentCapacitySerie.LineWidth = 3;
            CurrentCapacitySerie.MarkerSize = 4;
            CurrentCapacitySerie.MarkerShape = MarkerShape.openCircle;

            plot.Plot.AxisAuto(); // Если включен режим "Нет масштабам" - то не масштабируем
            plot.Render();
            #endregion
        }

        public void UpdateCapacityCurves(CalculationModel model, WpfPlot plot, double pitch, double roll)
        {
            switch (true)
            {
                case true:
                    plot.Plot.XAxis.TickLabelNotation(invertSign: false);
                    _ = plot.Plot.YAxis.Label("Емкость (Общий режим), (пФ)", color: System.Drawing.Color.Black, size: 16, fontName: "Arial", bold: true);
                    _capacityController.UpdateSensorsCapacity();
                    break;
                case false:
                    break;
            }

            _ = plot.Plot.XAxis.Label(@"Объем бака/баков, (литров)", color: System.Drawing.Color.Indigo, size: 16, fontName: "Arial", bold: true);
            plot.Plot.XAxis.MajorGrid(enable: true, color: System.Drawing.Color.Black, lineStyle: LineStyle.Dot);
            plot.Plot.YAxis.MajorGrid(enable: true, color: System.Drawing.Color.Black, lineStyle: LineStyle.Dot);
            plot.Plot.YAxis.TickLabelFormat("F2", dateTimeFormat: false);

            // Определяем индекс записи по заданным углам
            // Это необходимо, чтобы отрисовать одну единственную кривую, соответствующую выбранным углам
            int angleIndex = model.AngleFields.FindIndex(p => Math.Abs(p.Pitch - pitch) < Delta && Math.Abs(p.Roll - roll) < Delta);

            if (angleIndex == -1) return;

            // Выводим линию оси абсцисс
            plot.Plot.AddHorizontalLine(0, System.Drawing.Color.Black, 0.1F, LineStyle.Solid);

            plot.Plot.Remove(CurrentCapacitySerie); // Удаляем предыдущий вариант выбранной серии
            CurrentCapacitySerie = plot.Plot.AddScatter(model.ListSerieX[angleIndex], model.ListSerieY[angleIndex]);
            CurrentCapacitySerie.Color = System.Drawing.Color.Red;
            CurrentCapacitySerie.LineWidth = 3;
            CurrentCapacitySerie.MarkerSize = 4;
            CurrentCapacitySerie.MarkerShape = MarkerShape.openCircle;

            plot.Plot.AxisAuto(); // Если включен режим "Нет масштабам" - то не масштабируем
            plot.Render();
        }

        /// <summary>
        /// Отрисовка тарировочной кривой на канве
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="model"></param>
        /// <param name="tanks"></param>
        /// <param name="relative"></param>
        /// <param name="sensorSelected">Выделенный датчик (если null, то не отрисовывать цветом выделение)</param>
        public void FillTaringCanvas(Canvas canvas, CalculationModel model, bool relative, SensorModel sensorSelected, double pitch, double roll)
        {
            _relative = relative;
            //var pitch = model.CurrentBranch.AnglesModel.ReferencedPitch;
            //var roll  = model.CurrentBranch.AnglesModel.ReferencedRoll;

            try
            {
                canvas?.Children?.Clear();
                CalibrationGraph = InitializeGraph(canvas);

                if (model.SelectedTanks.Count > 1) // Несколько баков
                {
                    CalibrationGraph.Title = $"Суммарно (тангаж:{Math.Round(pitch, 2)}, крен:{Math.Round(roll, 2)})"; // Задаем заголовок графика для суммарной кривой
                    SetSummary(model, model.SelectedTanks, pitch, roll);
                    CalibrationGraph.PlotChart(relative); // Рисуем основу графика
                    CalibrationGraph.PlotCurves(true, 0); // Рисуем кривые
                    PlotSummarySensors(model.SelectedTanks, pitch, roll, sensorSelected); // Отрисовываем датчики для суммарной кривой}
                }
                else
                {
                    // Ищем тарировочные результаты для опорных углов
                    TarResult tr = model.SelectedTanks[^1].TarResultList.Find(x =>
                                            Math.Abs(x.Pitch - pitch) < Delta &&
                                            Math.Abs(x.Roll - roll) < Delta);
                    if (tr != null)
                    {
                        CalibrationGraph.Title = $"Тарировки (тангаж:{Math.Round(pitch, 2)}, крен:{Math.Round(roll, 2)})"; // Задаем заголовок графика для простой кривой
                        CalibrationGraph.AddCurve(tr.ResultAxisX,
                            relative ? tr.RelativeResultAxisY : tr.ResultAxisY,
                            model.SelectedTanks[^1].TankName, System.Windows.Media.Color.FromRgb(100, 200, 255));
                        CalibrationGraph.PlotChart(relative); // Рисуем основу графика
                        CalibrationGraph.PlotCurves(false, 0); // Рисуем кривые
                        PlotNonSummarySensors(model.SelectedTanks, pitch, roll, sensorSelected); // Отрисовываем датчики для всех кривых
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error($"Ошибка при построении тарировочной кривой: {ex}");
            }
        }

        /// <summary>
        /// Метод отрисовки датчиков на обычном графике тарировочных кривых
        /// </summary>
        /// <param name="tanks"></param>
        /// <param name="pitch"></param>
        /// <param name="roll"></param>
        /// <param name="selectedSensor"></param>
        private void PlotNonSummarySensors(List<TankModel> tanks, double pitch, double roll, SensorModel selectedSensor)
        {
            // Осуществляем попытку получить датчики для текущего бака
            List<SensorMeasuresModel> sm = GetActualTankSensors(tanks[^1], pitch, roll);

            // Если в баке нет датчиков, то выходим
            if (sm == null) return;

            // Упорядочиваем датчики по нижней кромке
            RearrangeSensors(sm);

            // Вычисляем удельный сдвиг датчиков по шкале абсцисс - в зависимости от их количества
            CalibrationGraph.CurvesList[^1].ElementShift =
              (CalibrationGraph.Canvas.ActualWidth - CalibrationGraph.Left - CalibrationGraph.Right) /
              (sm.Count + 1); // Приращение

            // И рисуем датчики
            PlotSensorsOnCurve(tanks, sm, selectedSensor, false);
        }

        /// <summary>
        /// Метод отрисовки датчиков на суммарном тарировочном графике
        /// </summary>
        private void PlotSummarySensors(List<TankModel> tanks, double pitch, double roll, SensorModel selectedSensor)
        {
            // Формируем список датчиков по нескольким топливным бакам (кривым)
            List<SensorMeasuresModel> summarySensorsList = new();

            // Проходим по всем кривым (бакам)
            foreach (var t1 in tanks)
            {
                // Получаем список датчиков по конкретному топливному баку
                var sm = GetActualTankSensors(t1, pitch, roll);
                if (sm == null) continue;
                foreach (var t in sm)
                {
                    summarySensorsList.Add(t);
                }
            }

            RearrangeSensors(summarySensorsList); // Упорядочиваем датчики по возрастанию нижней кромки
            // При этом нумерация датчиков сохраняется по нумерации в пределах конкретного топливного бака

            // Вычисляем приращение (сдвиг) датчиков в зависимости от их количества
            // Вычисляем относительный сдвиг в пределах конкретной кривой
            CalibrationGraph.CurvesList[^1].ElementShift =
                  (CalibrationGraph.Canvas.ActualWidth - CalibrationGraph.Left - CalibrationGraph.Right) /
                  (summarySensorsList.Count + 1); // Приращение

            // Проходим по всем датчикам суммарной кривой
            PlotSensorsOnCurve(tanks, summarySensorsList, selectedSensor, true);
        }

        /// <summary>
        /// Метод формирования датчиков конкретной кривой для конкретной суперпозиции углов
        /// Также в нем происходит упорядочивание по нижней кромке
        /// </summary>
        /// <param name="tankName"></param>
        /// <param name="tank"></param>
        /// <param name="pitch"></param>
        /// <param name="roll"></param>
        /// <returns></returns>
        public static List<SensorMeasuresModel> GetActualTankSensors(TankModel tank, double pitch, double roll)
        {
            // Создаем набор датчиков для конкретной суперпозиции углов тангажа и крена
            List<SensorMeasuresModel> sensorsList = new();

            foreach (SensorModel s in tank.Sensors)
            {
                if (!s.IsActiveSensor) continue;
                // Ищем значения измерений датчика для данной суперпозиции углов тангажа и крена
                SensorMeasuresModel sm = s.SensorMeasuresList.Find(item =>
                                         Math.Abs(item.Pitch - pitch) < Delta &&
                                         Math.Abs(item.Roll - roll) < Delta &&
                                         item.TankModel == tank);
                if (sm == null) continue;
                sensorsList.Add(sm);
            }

            return sensorsList;
        }

        /// <summary>
        /// Метод отрисовки датчика на тарировочной кривой
        /// </summary>
        /// <param name="tanks"></param>
        /// <param name="sm">Список неизмеряемых объемов датчиков</param>
        /// <param name="selectedSensor">Выделенный датчик</param>
        /// <param name="isSummary">Флаг суммарной кривой</param>
        private void PlotSensorsOnCurve(List<TankModel> tanks, List<SensorMeasuresModel> sm, SensorModel selectedSensor, bool isSummary)
        {
            if (sm.Count == 0) return;
            // Вычисляем низ самого нижнего в пространстве бака
            double absMinimum = sm[0].MinimumY;
            for (int i = 1; i < sm.Count; i++)
            {
                if (sm[i].MinimumY < absMinimum)
                {
                    absMinimum = sm[i].MinimumY;
                }
            }

            for (var sensor = 0; sensor < sm.Count; sensor++)
            {
                double lower;
                double upper;

                if (!isSummary)
                {
                    if (_relative)
                    {
                        lower = sm[sensor].RelativeLower;
                        upper = sm[sensor].RelativeUpper;
                    }
                    else
                    {
                        lower = sm[sensor].Lower;
                        upper = sm[sensor].Upper;
                    }
                }
                else
                {
                    if (_relative)
                    {
                        lower = sm[sensor].RelativeLower + (sm[sensor].MinimumY - absMinimum);
                        upper = sm[sensor].RelativeUpper + (sm[sensor].MinimumY - absMinimum);
                    }
                    else
                    {
                        lower = sm[sensor].Lower;
                        upper = sm[sensor].Upper;
                    }
                }


                if (upper > CalibrationGraph.CurvesList[^1].MaxY ||
                    upper < CalibrationGraph.CurvesList[^1].MinY ||
                    lower < CalibrationGraph.CurvesList[^1].MinY ||
                    lower > CalibrationGraph.CurvesList[^1].MaxY) return;

                // Сдвиг данного элемента от левого края графика
                double delta = CalibrationGraph.CurvesList[^1].ElementShift * (sensor + 1) * (CalibrationGraph.CurvesList[^1].MaxX / CalibrationGraph.MaxCurveX);

                var t = CalibrationGraph.CurvesList[^1].ArrayY.FindIndex(item => item >= lower);
                if (t == 0) t++;
                // Для нижней точки элемента
                // Координата X нижней точки того лассо, в котором заключена нижняя точка элемента
                double elementLowBottomX = CalibrationGraph.CurvesList[^1].ArrayX[t - 1] * CalibrationGraph.Kx;
                // Координата Y нижней точки того лассо, в котором заключена нижняя точка элемента
                double elementLowBottomY = CalibrationGraph.CurvesList[^1].ArrayY[t - 1] * CalibrationGraph.Ky;
                // Координата X верхней точки того лассо, в котором заключена нижняя точка элемента
                double elementLowTopX = CalibrationGraph.CurvesList[^1].ArrayX[t] * CalibrationGraph.Kx;
                // Координата Y верхней точки того лассо, в котором заключена нижняя точка элемента
                double elementLowTopY = CalibrationGraph.CurvesList[^1].ArrayY[t] * CalibrationGraph.Ky;

                t = CalibrationGraph.CurvesList[^1].ArrayY.FindIndex(item => item >= upper);
                if (t == 0) t++;
                // Для верхней точки элемента
                // Координата X нижней точки того лассо, в котором заключена верхняя точка элемента
                double elementUpBottomX = CalibrationGraph.CurvesList[^1].ArrayX[t - 1] * CalibrationGraph.Kx;
                // Координата Y нижней точки того лассо, в котором заключена верхняя точка элемента
                double elementUpBottomY = CalibrationGraph.CurvesList[^1].ArrayY[t - 1] * CalibrationGraph.Ky;
                // Координата X верхней точки того лассо, в котором заключена верхняя точка элемента
                double elementUpTopX = CalibrationGraph.CurvesList[^1].ArrayX[t] * CalibrationGraph.Kx;
                // Координата Y верхней точки того лассо, в котором заключена верхняя точка элемента
                double elementUpTopY = CalibrationGraph.CurvesList[^1].ArrayY[t] * CalibrationGraph.Ky;

                // Определяем сдвиг элемента относительно левого края графика (по оси X)
                var xPosition = CalibrationGraph.Left + delta;
                // Определяем низ графика
                var bottomCanvas = CalibrationGraph.Canvas.ActualHeight - CalibrationGraph.Bottom;
                // Масштабированное смещение верхней точки элемента
                var scaledTop = (upper - CalibrationGraph.MinCurveY) * CalibrationGraph.Ky;

                // Создаем экземпляр прямоугольника
                GraphRectangle rect = new(CalibrationGraph, CalibrationGraph.CurvesList[^1])
                {
                    X = xPosition - SensorWidth / 2,
                    Y = bottomCanvas - scaledTop,
                    Width = SensorWidth,
                    BorderWidth = 3,
                    BorderColor = Brushes.Black,
                    RectangleOpacity = 0.5
                };
                if (selectedSensor != null)
                {
                    if (selectedSensor.FuelTank.TankName == sm[sensor].TankModel.TankName)
                    {
                        rect.BackgroundColor = sm[sensor].SensorNumber == selectedSensor.SensorPositionInTank ?
                                                          new SolidColorBrush(Colors.Red) :
                                                          //new SolidColorBrush(sm[sensor].TankColor);
                                                          new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        //rect.BackgroundColor = new SolidColorBrush(sm[sensor].TankColor);
                        rect.BackgroundColor = new SolidColorBrush(Colors.Black);
                    }
                }
                else
                {
                    //rect.BackgroundColor = new SolidColorBrush(sm[sensor].TankColor);
                    rect.BackgroundColor = new SolidColorBrush(Colors.Black);
                }

                if (upper > lower)
                {
                    rect.Height = (upper - lower) * CalibrationGraph.Ky;
                    rect.PlotRectangle(0);
                }
                else
                {
                    rect.Height = (lower - upper) * CalibrationGraph.Ky;
                    rect.PlotRectangle(0);
                }

                // Определяем сдвиги лучей от верхней и нижней точек элементов
                var scale1 = (lower * CalibrationGraph.Ky - elementLowBottomY) / (elementLowTopY - elementLowBottomY);
                var deltaLower = (elementLowTopX - elementLowBottomX) * scale1 + elementLowBottomX - delta;
                var scale2 = (upper * CalibrationGraph.Ky - elementUpBottomY) / (elementUpTopY - elementUpBottomY);
                var deltaUpper = (elementUpTopX - elementUpBottomX) * scale2 + elementUpBottomX - delta;
                var topLineShift = deltaUpper; // Сдвиг верхней линии
                var bottomLineShift = deltaLower; // Сдвиг нижней линии

                GraphLine topLine = new(CalibrationGraph, CalibrationGraph.CurvesList[^1]) // Создаем линию по верхней кромке элемента
                {
                    //LineColor = new SolidColorBrush(tanks[^1].Color),
                    LineColor = new SolidColorBrush(Colors.Black),
                    LineWeight = 1 // Толщина линии
                };
                topLine.StartY = topLine.StopY = bottomCanvas - scaledTop;
                topLine.StartX = CalibrationGraph.Left + topLineShift + delta;
                topLine.StopX = xPosition;
                topLine.PlotLine(0);

                GraphLine bottomLine = new(CalibrationGraph, CalibrationGraph.CurvesList[^1]) // Создаем линию по нижней кромке элемента
                {
                    //LineColor = new SolidColorBrush(tanks[^1].Color),
                    LineColor = new SolidColorBrush(Colors.Black),
                    LineWeight = 1 // Толщина линии
                };
                bottomLine.StartY = bottomLine.StopY = bottomCanvas - (lower - CalibrationGraph.MinCurveY) * CalibrationGraph.Ky;
                bottomLine.StartX = CalibrationGraph.Left + bottomLineShift + delta;
                bottomLine.StopX = xPosition;
                bottomLine.PlotLine(0);

                // Отображаем номер датчика
                GraphText sensorNumber = new(CalibrationGraph, CalibrationGraph.CurvesList[^1])
                {
                    Text = $"{sm[sensor].SensorNumber}", // Номер элемента
                    TextColor = Brushes.Black, // Цвет текста
                    FontFamily = new FontFamily("Verdana"), // Шрифт Verdana
                    FontSize = 14, // Кегль шрифта
                    FontWeight = FontWeights.ExtraLight, // Полужирное начертание
                                                         // Позиционирование текста относительно холста
                    PositionX = xPosition - 5,
                    PositionY = bottomCanvas - scaledTop + 5
                };
                sensorNumber.PlotText(0);

                // Для суммарной тарировочной кривой у датчиков не показываем уровни и неизмеряемые объемы
                if (isSummary)
                {
                    #region Отображение уровней
                    // Определяем округленное занчение показания верха элемента (два знака после запятой)
                    var measureTop = decimal.Round((decimal)upper, 2);
                    var measureBottom = decimal.Round((decimal)lower, 2);

                    GraphText measureUp = new(CalibrationGraph, CalibrationGraph.CurvesList[^1]) // Создаем текст, отображающий показания верха датчика
                    {
                        Text = $"H: {measureTop}", // Показания верха элемента
                        TextColor = Brushes.Black, // Цвет текста
                        FontFamily = new FontFamily("Verdana"), // Шрифт Verdana
                        FontSize = 10, // Кегль шрифта
                        FontWeight = FontWeights.ExtraLight, // Полужирное начертание
                                                             // Позиционирование текста относительно холста
                        PositionX = xPosition - SensorWidth / 2,
                        PositionY = bottomCanvas - scaledTop - 30
                    };
                    measureUp.PlotText(0);

                    GraphText measureDown = new(CalibrationGraph, CalibrationGraph.CurvesList[^1]) // Создаем текст, отображающий показания верха элемента
                    {
                        Text = $"H: {measureBottom}", // Показания низа элемента
                        TextColor = Brushes.Black, // Цвет текста
                        FontFamily = new FontFamily("Verdana"), // Шрифт Verdana
                        FontSize = 10, // Кегль шрифта
                        FontWeight = FontWeights.ExtraLight, // Полужирное начертание
                                                             // Позиционирование текста относительно холста
                        PositionX = xPosition - SensorWidth / 2,
                        PositionY = bottomCanvas - (lower - CalibrationGraph.MinCurveY) * CalibrationGraph.Ky + 2
                    };
                    measureDown.PlotText(0);
                    #endregion
                }
                else
                {
                    #region Отображение уровней и неизмеряемых объемов
                    // Здесь получим объем бака
                    var tankVolume = tanks[^1].TankVolume;

                    // Определяем округленное занчение показания верха элемента (два знака после запятой)
                    var measureTop = decimal.Round((decimal)upper, 2);
                    var measureBottom = decimal.Round((decimal)lower, 2);
                    var volumeTop = decimal.Round((decimal)(tankVolume - sm[sensor].VolumeUp), 3);
                    var volumeBottom = decimal.Round((decimal)sm[sensor].VolumeDown, 3);

                    GraphText measureUp = new(CalibrationGraph, CalibrationGraph.CurvesList[^1]) // Создаем текст, отображающий показания верха датчика
                    {
                        Text = $"H: {measureTop}\nV: {volumeTop}", // Показания верха элемента
                        TextColor = Brushes.Black, // Цвет текста
                        FontFamily = new FontFamily("Verdana"), // Шрифт Verdana
                        FontSize = 10, // Кегль шрифта
                        FontWeight = FontWeights.ExtraLight, // Полужирное начертание
                                                             // Позиционирование текста относительно холста
                        PositionX = xPosition - SensorWidth / 2,
                        PositionY = bottomCanvas - scaledTop - 30
                    };
                    measureUp.PlotText(0);

                    GraphText measureDown = new(CalibrationGraph, CalibrationGraph.CurvesList[^1]) // Создаем текст, отображающий показания верха элемента
                    {
                        Text = $"H: {measureBottom}\nV: {volumeBottom}", // Показания низа элемента
                        TextColor = Brushes.Black, // Цвет текста
                        FontFamily = new FontFamily("Verdana"), // Шрифт Verdana
                        FontSize = 10, // Кегль шрифта
                        FontWeight = FontWeights.ExtraLight, // Полужирное начертание
                                                             // Позиционирование текста относительно холста
                        PositionX = xPosition - SensorWidth / 2,
                        PositionY = bottomCanvas - (lower - CalibrationGraph.MinCurveY) * CalibrationGraph.Ky + 2
                    };
                    measureDown.PlotText(0);
                    #endregion
                }
            }
        }

        /// <summary>
        /// Метод формирования суммарной кривой по нескольким топливным бакам
        /// </summary>
        private void SetSummary(CalculationModel model, List<TankModel> tanks, double pitch, double roll)
        {
            var count = model.CurrentBranch.AnglesModel.NodesQuantity;
            List<double[,]> auxCurves = new(); // Создаем вспомогательный список
                                               // Проходим в цикле по всем бакам - заполняем список
            double[] minimumY = new double[tanks.Count];
            double[] maximumY = new double[tanks.Count];
            for (var t = 0; t < tanks.Count; t++)
            {
                // Формируем промежуточный массив, где
                // 1-я размерность - это указатель на ось:
                // 0 - ось Y
                // 1 - ось X
                // 2-я размерность - это значения по тем или иным осям
                double[,] tankArray = new double[2, count];
                for (var i = 0; i < count; i++)
                {
                    // Ищем набор кривых, соответствующих углам тангажа и крена и топливному баку
                    TarResult tr = tanks[t].TarResultList.Find(item =>
                                   Math.Abs(item.Pitch - pitch) < Delta &&
                                   Math.Abs(item.Roll - roll) < Delta);
                    if (tr == null) continue;
                    tankArray[0, i] = tr.ResultAxisY[i];
                    minimumY[t] = tr.ResultAxisY.Min();
                    maximumY[t] = tr.ResultAxisY.Max();
                    tankArray[1, i] = tr.ResultAxisX[i];
                }

                auxCurves.Add(tankArray);
            }

            // По окончании данного цикла у нас есть два массива - минимальных и максимальных значений по оси Y
            CalibrationGraph.MinCurveY = minimumY.Min();
            CalibrationGraph.MaxCurveY = maximumY.Max();
            // Теперь мы имеем самую минимальную и самую максимальную величину по оси Y

            #region Блок формирования значений по оси ординат (показания датчиков)

            SummaryArray = new double[2, count]; // Результирующий массив (совокупная тарировочная кривая)
                                                 // Формируем ординаты результирующего массива
            SummaryArray[0, 0] = CalibrationGraph.MinCurveY;
            SummaryArray[0, count - 1] = CalibrationGraph.MaxCurveY;
            for (var i = 1; i < count - 1; i++)
            {
                SummaryArray[0, i] = SummaryArray[0, 0] + i * ((SummaryArray[0, count - 1] - SummaryArray[0, 0]) / (count - 1));
            }

            #endregion

            #region Блок формирования значений по оси абсцисс (объем бака/баков)
            // Формируем абсциссы (объемы) результирующего массива
            // 1 - Внешний цикл по срезам итогового массива
            // 2 - Второй цикл - по бакам
            // 3 - Третий цикл - по срезам выбранного бака
            double p = 0; // Параметр - коэффициент пересчета (метод интерполяции)
            for (var resultSlice = 1; resultSlice < count; resultSlice++) // Первый (нулевой) срез не считаем
            {
                for (var fileIndex = 0; fileIndex < tanks.Count; fileIndex++) // Проходим по бакам
                {
                    for (var tankSlice = 0; tankSlice < count; tankSlice++) // Проходим по срезам бака
                    {
                        // Проверяем, если результирующий срез ниже текущего среза бака, то прыгаем далее
                        if (SummaryArray[0, resultSlice] <= auxCurves[fileIndex][0, tankSlice]) continue;
                        else if (tankSlice + 1 == count) continue;
                        else if (SummaryArray[0, resultSlice] <= auxCurves[fileIndex][0, tankSlice + 1])
                        {
                            // Результирующий срез выше текущего среза бака, но ниже следующего
                            // Считаем коэффициент
                            p = (SummaryArray[0, resultSlice] - auxCurves[fileIndex][0, tankSlice]) /
                                (auxCurves[fileIndex][0, tankSlice + 1] - auxCurves[fileIndex][0, tankSlice]);
                            // Прибавляем к результату приращение по коэффициенту
                            SummaryArray[1, resultSlice] +=
                              (float)p * (auxCurves[fileIndex][1, tankSlice + 1] - auxCurves[fileIndex][1, tankSlice]);
                        }
                        else // Результирующий срез выше и текущего среза бака и выше следующего
                        {
                            // Прибавляем приращение на блок между срезами бака
                            SummaryArray[1, resultSlice] +=
                              auxCurves[fileIndex][1, tankSlice + 1] - auxCurves[fileIndex][1, tankSlice];
                        }
                    }
                }
            }
            #endregion

            #region Блок формирования значений для абсолютного или относительного расчетов
            if (_relative)
            {
                // Если расчет относительный, то вычитаем из всех значений оси Y значение первого (нулевого) элемента
                var minY = SummaryArray[0, 0];
                for (var i = 0; i < count; i++)
                {
                    SummaryArray[0, i] -= minY;
                }
            }
            #endregion

            List<double> resultX = new();
            List<double> resultY = new();
            for (var i = 0; i < count; i++)
            {
                resultX.Add(SummaryArray[1, i]);
                resultY.Add(SummaryArray[0, i]);
            }

            // Обращение к экземпляру суммарного графика
            CalibrationGraph.AddCurve(resultX, resultY, "Суммарная", Colors.Blue);
        }

        /// <summary>
        /// Метод упорядочивания датчиков
        /// по возрастанию нижней кромки датчика
        /// </summary>
        private static void RearrangeSensors(IList<SensorMeasuresModel> list)
        {
            // Упорядочиваем датчики по возрастанию нижней кромки
            var list2 = (from item in list orderby item.Lower select item).ToList();
            for (var i = 0; i < list.Count; i++)
            {
                list[i] = list2[i];
            }
        }

        /// <summary>
        /// Инициализация графика тарировочной кривой
        /// </summary>
        /// <returns></returns>
        private static CalibrationGraph InitializeGraph(Canvas canvas)
        {
            //Создаем графопостроитель тарировочных кривых
            CalibrationGraph graph = new()
            {
                Canvas = canvas,
                ScaleY = 1, // Коэффициент масштабирования по оси ординат
                NameAxisX = "Объем\n    (V)", // Определяем надпись оси абсцисс
                NameAxisY = "Показания\nдатчика (H)", // Определяем надпись оси ординат
                CurvesList = new List<GraphCurve>() // Инициализируем список кривых
            };
            return graph;
        }
    }
}
