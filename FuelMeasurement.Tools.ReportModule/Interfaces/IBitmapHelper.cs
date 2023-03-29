using FuelMeasurement.Tools.CalculationData.Models;
using FuelMeasurement.Tools.ReportModule.Models;
using ScottPlot;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FuelMeasurement.Tools.ReportModule.Interfaces
{
    /// <summary>
    /// Интерфейс получения изображений значимых сущностей (для отчетов)
    /// </summary>
    public interface IBitmapHelper
    {
        /// <summary>
        /// Создание изображения МПИ для самолета
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tanks"></param>
        /// <param name="path"></param>
        /// <returns>Модель изображения</returns>
        ImageReportModel ErrorsGraphToBitmap(CalculationModel model, List<TankModel> tanks);

        /// <summary>
        /// Создание изображения МПИ для топливного бака
        /// </summary>
        /// <param name="model">Модель вычислителя</param>
        /// <param name="tank">Топливный бак</param>
        /// <returns>Модель изображения</returns>
        ImageReportModel ErrorsGraphToBitmap(CalculationModel model, TankModel tank);

        /// <summary>
        /// Создание изображения тарировочной кривой
        /// </summary>
        /// <param name="canvas">Канва</param>
        /// <returns>Модель изображения</returns>
        ImageReportModel TaringCurveToBitmap(Canvas canvas);

        /// <summary>
        /// Создание изображения топливного бака
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>Модель изображения</returns>
        ImageReportModel TankViewToBitmap(BitmapSource bitmap);

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="model"></param>
        ///// <param name="plot"></param>
        ///// <param name="tanks"></param>
        //void GetErrorsCurves(CalculationModel model, WpfPlot plot, List<TankModel> tanks);
    }
}