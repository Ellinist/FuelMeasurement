using System.Collections.Generic;

namespace FuelMeasurement.Tools.ReportModule.Models
{
    /// <summary>
    /// Модель отчета по топливному баку
    /// </summary>
    public class TankReportModel
    {
        /// <summary>
        /// Название топливного бака
        /// </summary>
        public string TankName { get; }
        /// <summary>
        /// Объем топливного бака
        /// </summary>
        public double TankVolume { get; }

        /// <summary>
        /// Список данных объемов по срезам топливного бака
        /// </summary>
        public List<double> ResultAxisX { get; set; } = new();

        /// <summary>
        /// Список уровней срезов топливного бака (абсолютные величины в мировой системе координат)
        /// </summary>
        public List<double> ResultAxisY { get; set; } = new();

        /// <summary>
        /// Список уровней срезов топливного бака (относительные величины - от нуля и выше)
        /// </summary>
        public List<double> RelativeResultAxisY { get; set; } = new();

        /// <summary>
        /// Датчики топливного бака
        /// </summary>
        public List<SensorReportModel> Sensors { get; } = new();

        public double UpperUnmeasurable { get; }
        public double UpperPercent { get; }
        public double LowerUnmeasurable { get; }
        public double LowerPercent { get; }
        public int SensorsCount { get; }
        public int InactiveSensorsCount { get; }

        #region Блок изображений для отчетов
        /// <summary>
        /// Изображение тарировочной кривой с датчиками
        /// </summary>
        public ImageReportModel TarCurveImage { get; set; }

        /// <summary>
        /// Изображение МПИ
        /// </summary>
        public ImageReportModel ErrorsImage { get; set; }

        /// <summary>
        /// Изображение топливного бака
        /// </summary>
        public ImageReportModel TankImage2D { get; set; }
        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name"></param>
        /// <param name="volume"></param>
        /// <param name="axisX"></param>
        /// <param name="axisY"></param>
        /// <param name="relAxisY"></param>
        /// <param name="upperUnmeasurable"></param>
        /// <param name="lowerUnmeasurable"></param>
        /// <param name="upPercent"></param>
        /// <param name="lowerPercent"></param>
        /// <param name="sensors"></param>
        /// <param name="inactiveSensors"></param>
        /// <param name="tarCurveImage"></param>
        /// <param name="errorsImage"></param>
        /// <param name="tankImage"></param>
        public TankReportModel(string name, double volume,
                               List<double> axisX, List<double> axisY, List<double> relAxisY,
                               double upperUnmeasurable, double lowerUnmeasurable,
                               double upPercent, double lowerPercent,
                               List<SensorReportModel> sensors, int inactiveSensors,
                               ImageReportModel tarCurveImage, ImageReportModel errorsImage, ImageReportModel tankImage)
        {
            TankName = name;
            TankVolume = volume;
            ResultAxisX = axisX;
            ResultAxisY = axisY;
            RelativeResultAxisY = relAxisY;
            UpperUnmeasurable = upperUnmeasurable;
            LowerUnmeasurable = lowerUnmeasurable;
            UpperPercent = upPercent;
            LowerPercent = lowerPercent;
            InactiveSensorsCount = inactiveSensors;
            Sensors = sensors;
            SensorsCount = sensors.Count;
            TarCurveImage = tarCurveImage;
            ErrorsImage = errorsImage;
            TankImage2D = tankImage;
        }
    }
}