using System.Collections.Generic;

namespace FuelMeasurement.Tools.ReportModule.Models
{
    /// <summary>
    /// Модель отчета по самолету
    /// </summary>
    public class AirplaneReportModel
    {
        /// <summary>
        /// Название самолета
        /// </summary>
        public string AirplaneName { get; }

        /// <summary>
        /// Топливные баки самолета
        /// </summary>
        public List<TankReportModel> Tanks { get; }

        public List<ImageReportModel> TanksImages { get; }

        public ImageReportModel CurveImage { get; }

        /// <summary>
        /// Изображение МПИ для самолета
        /// </summary>
        public ImageReportModel ErrorsImage { get; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="airplaneName"></param>
        /// <param name="tankImages"></param>
        /// <param name="tanks"></param>
        /// <param name="errorsImage"></param>
        /// <param name="curve"></param>
        public AirplaneReportModel(string airplaneName, List<ImageReportModel> tankImages, List<TankReportModel> tanks, ImageReportModel errorsImage, ImageReportModel curve)
        {
            AirplaneName = airplaneName;
            Tanks        = tanks;
            TanksImages  = tankImages;
            ErrorsImage  = errorsImage;
            CurveImage   = curve;
        }
    }
}
