using FuelMeasurement.Common.Enums;
using FuelMeasurement.Tools.CalculationData.Models;
using System.Collections.Generic;
using System.Windows.Controls;
using FuelMeasurement.Tools.ReportModule.Models;

namespace FuelMeasurement.Tools.ReportModule.Interfaces
{
    /// <summary>
    /// Интерфейс создания отчетов
    /// </summary>
    public interface IReportController
    {
        /// <summary>
        /// Создание отчета по всему самолету (для текущей ветки проекта)
        /// </summary>
        /// <param name="tankViewList"></param>
        /// <param name="canvas"></param>
        /// <param name="reportType"></param>
        /// <param name="model"></param>
        /// <param name="branchTanks"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        void CreateAirplaneReport(List<ImageReportModel> tankViewList,
                                  Canvas canvas,
                                  ReportTypeEnum reportType,
                                  CalculationModel model,
                                  List<TankModel> branchTanks,
                                  string file);

        /// <summary>
        /// Создание отчета для выбранного топливного бака
        /// </summary>
        /// <param name="tankViewList"></param>
        /// <param name="canvas"></param>
        /// <param name="reportType"></param>
        /// <param name="model"></param>
        /// <param name="tank"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        void CreateTankReport(List<ImageReportModel> tankViewList,
                              Canvas canvas,
                              ReportTypeEnum reportType,
                              CalculationModel model,
                              List<TankModel> tank,
                              string file);

        /// <summary>
        /// Получение пути и имени файла
        /// </summary>
        /// <returns></returns>
        string GetFilePath();
    }
}
