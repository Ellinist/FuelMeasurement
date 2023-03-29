using FuelMeasurement.Tools.ReportModule.Models;

namespace FuelMeasurement.Tools.ReportModule.Interfaces
{
    /// <summary>
    /// Интерфейс создания файла WORD
    /// </summary>
    public interface IWordReportController
    {
        void CreateAirplaneWordDocument(string file, AirplaneReportModel model);

        void CreateTankWordDocument(string filePath, TankReportModel model);
    }
}
