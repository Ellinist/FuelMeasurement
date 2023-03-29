using FuelMeasurement.Tools.CalculationData.Models;
using FuelMeasurement.Tools.ReportModule.Models;
using System.Collections.Generic;
using System.Windows.Controls;

namespace FuelMeasurement.Tools.ReportModule.Interfaces
{
    /// <summary>
    /// Создание модели отчета (самолета, топливного бака, зайца и т.д.)
    /// </summary>
    public interface IReportModelCreator
    {
        /// <summary>
        /// Создание модели отчета для самолет
        /// </summary>
        /// <returns></returns>
        AirplaneReportModel CreateAirplaneReportModel(List<ImageReportModel> tankViewList,
                                                      Canvas canvas,
                                                      CalculationModel model,
                                                      List<TankModel> tanks);

        /// <summary>
        /// Создание модели отчета топливного бака
        /// </summary>
        /// <returns></returns>
        TankReportModel CreateTankReportModel(List<ImageReportModel> tankViewList,
                                              Canvas canvas,
                                              CalculationModel model,
                                              TankModel tank);
    }
}
