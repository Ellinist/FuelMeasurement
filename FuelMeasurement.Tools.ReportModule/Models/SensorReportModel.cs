namespace FuelMeasurement.Tools.ReportModule.Models
{
    /// <summary>
    /// Модель отчета по датчику
    /// </summary>
    public class SensorReportModel
    {
        /// <summary>
        /// Название датчика
        /// </summary>
        public string SensorName { get; set; }
        /// <summary>
        /// Длина датчика
        /// </summary>
        public double SensorLength { get; set; }
        /// <summary>
        /// Верхняя точка датчика
        /// </summary>
        public PointReportModel UpPoint { get; set; }
        /// <summary>
        /// Нижняя точка датчика
        /// </summary>
        public PointReportModel DownPoint { get; set; }
        /// <summary>
        /// Верхний зазор датчика
        /// </summary>
        public double UpIndent { get; set; }
        /// <summary>
        /// Нижний зазор датчика
        /// </summary>
        public double DownIndent { get; set; }
        public string GroupName { get; set; }
        public double LowFuelLevel { get; }
        public double HighFuelLevel { get; }
        public double LowFuelVolume { get; }
        public double HighFuelVolume { get; }

        public SensorReportModel(string sensorName, double length,
                                 PointReportModel upPoint, PointReportModel downPoint,
                                 double upIndent, double downIndent, string groupName,
                                 double lowFuelLevel, double highFuelLevel,
                                 double lowFuelVolume, double highFuelVolume)
        {
            SensorName = sensorName;
            SensorLength = length;
            UpPoint = upPoint;
            DownPoint = downPoint;
            UpIndent = upIndent;
            DownIndent = downIndent;
            GroupName = groupName;
            LowFuelLevel = lowFuelLevel;
            HighFuelLevel = highFuelLevel;
            LowFuelVolume = lowFuelVolume;
            HighFuelVolume = highFuelVolume;
        }
    }
}
