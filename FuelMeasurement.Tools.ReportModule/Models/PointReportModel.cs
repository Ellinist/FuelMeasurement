namespace FuelMeasurement.Tools.ReportModule.Models
{
    /// <summary>
    /// Модель отчета по точке - нафига?
    /// </summary>
    public class PointReportModel
    {
        public string X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }

        public PointReportModel(string point)
        {
            var points = point.Split(';');
            X = points[0];
            Y = points[1];
            Z = points[2];
        }
    }
}
