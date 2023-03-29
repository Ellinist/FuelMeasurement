using FuelMeasurement.Tools.CalculationData.Models;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using FuelMeasurement.Tools.ReportModule.Models;
using FuelMeasurement.Tools.ReportModule.Interfaces;
using FuelMeasurement.Tools.Compute.Interfaces;
using FuelMeasurement.Tools.Plotter.Interfaces;

namespace FuelMeasurement.Tools.ReportModule.Helpers
{
    /// <summary>
    /// Класс построителя изображений для МПИ, топливного бака и тарировочной кривой
    /// </summary>
    public class BitmapReportHelper : IBitmapHelper
    {
        #region Константы - думать по поводу переноса в справочники
        private const int MaxWidth = 1600; // Размер экрана
        private const int MaxHeight = 1000; // Размер экрана
        private const int DocWidth = 720;  // Размер документа
        private const int DocHeight = 450;  // Размер документа
        private const double Scale = 1;    // Масштабирование
        #endregion

        private readonly IMeasureController _measureController;
        private readonly IErrorsController  _errorsController;
        private readonly IPlotterController _plotterController;

        /// <summary>
        /// Конструктор построителя изображений
        /// </summary>
        /// <param name="measureController"></param>
        /// <param name="errorsController"></param>
        /// <param name="plotterController"></param>
        public BitmapReportHelper(IMeasureController measureController, IErrorsController errorsController, IPlotterController plotterController)
        {
            _measureController = measureController;
            _errorsController  = errorsController;
            _plotterController = plotterController;
        }

        /// <summary>
        /// Создание изображения тарировочной кривой 
        /// </summary>
        /// <param name="canvas"></param>
        /// <returns></returns>
        public ImageReportModel TaringCurveToBitmap(Canvas canvas)
        {
            var resultImageArray = GetTaringImage(canvas); // Переводим в байтовый массив

            return resultImageArray; // Возвращаем картинку
        }

        /// <summary>
        /// Топливный бак ---> в изображение
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>Модель изображения</returns>
        public ImageReportModel TankViewToBitmap(BitmapSource bitmap)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder(); // Не стоит использовать encoder для bitmap - возникает черная полоса (на картинке и в жизни)
            using MemoryStream stream = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(stream);
            byte[] byteRes = stream.ToArray();
            stream.Close();
            
            ImageReportModel irm = new ImageReportModel(byteRes, (int)bitmap.Width * 300 / 96, (int)bitmap.Height * 300 / 96);

            return irm;
        }

        /// <summary>
        /// График МПИ в изображение для целого самолета
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tanks"></param>
        /// <param name="path"></param>
        /// <returns>Модель изображения</returns>
        public ImageReportModel ErrorsGraphToBitmap(CalculationModel model, List<TankModel> tanks)
        {
            ImageReportModel graph = GetErrorsPlot(model, tanks);

            return graph;
        }

        /// <summary>
        /// Преобразование МПИ в картинку для одного топливного бака
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tank"></param>
        /// <returns>Модель изображения</returns>
        public ImageReportModel ErrorsGraphToBitmap(CalculationModel model, TankModel tank)
        {
            List<TankModel> tm = new()
            {
                tank
            };
            ImageReportModel graph = GetErrorsPlot(model, tm);

            return graph;
        }

        /// <summary>
        /// Метод получения графика МПИ
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tanks"></param>
        /// <returns>Модель изображения</returns>
        private ImageReportModel GetErrorsPlot(CalculationModel model, List<TankModel> tanks)
        {
            WpfPlot errorsGraphPlot = new WpfPlot();
            _plotterController.SetErrorsCurves(model, errorsGraphPlot, tanks, 0, 0);

            Bitmap start = errorsGraphPlot.Plot.Render(1600, 1000, false, 1); // Получаем качественное изображение
            Bitmap finish = new Bitmap(start, 720, 450); // Получаем масштабированное изображение

            var res = ImageToByteArray(finish); // Вызываем построитель МПИ

            // Считаем размерность изображения с учетом коэффициентов разрешения экрана
            var imageWidthEMU = (long)(finish.Width / finish.HorizontalResolution * 914400L);
            var imageHeightEMU = (long)(finish.Height / finish.VerticalResolution * 914400L);
            ImageReportModel irm = new ImageReportModel(res, imageWidthEMU, imageHeightEMU);

            return irm;
        }

        /// <summary>
        /// Преобразование Bitmap в байтовый массив
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static byte[] ImageToByteArray(Bitmap img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Получение изображения тарировочной кривой в байтовом массиве
        /// </summary>
        /// <param name="graph"></param>
        /// <returns>Модель изображения</returns>
        private ImageReportModel GetTaringImage(Canvas graph)
        {
            int iWidth = (int)graph.ActualWidth;
            int iHeight = (int)graph.ActualHeight;
            // 1) Получаем текущее разрешение
            var pSource = PresentationSource.FromVisual(graph);
            Matrix m = pSource.CompositionTarget.TransformToDevice;
            double dpiX = m.M11 * 96;
            double dpiY = m.M22 * 96;

            // 2) Создание целевого объекта
            var elementBitmap = new RenderTargetBitmap(iWidth, iHeight, dpiX, dpiY, PixelFormats.Default);

            // 3) undo element transformation
            var drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                var visualBrush = new VisualBrush(graph);
                drawingContext.DrawRectangle(
                    visualBrush,
                    null,
                    new Rect(new System.Windows.Point(0, 0), new System.Windows.Size(iWidth / m.M11, iHeight / m.M22)));
            }

            // 4) Отрисовка
            elementBitmap.Render(drawingVisual);

            // 5) Создание изображения
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(elementBitmap));

            MemoryStream ms = new MemoryStream();
            encoder.Save(ms);
            var resultImageArray = ms.ToArray();

            // Здесь может быть проблема с размерностью! Проверить на практике!
            ImageReportModel irm = new ImageReportModel(resultImageArray, iWidth, iHeight);

            return irm;
        }
    }
}
