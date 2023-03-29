using FuelMeasurement.Tools.CalculationData.Models;
using FuelMeasurement.Tools.Compute.Interfaces;

namespace FuelMeasurement.Tools.ComputeModule.Interfaces
{
    /// <summary>
    /// Расчет МПИ по опорным углам
    /// </summary>
    public interface IReferencedController
    {
        #region Блок вычислений МПИ по опорным углам для общего режима
        void SetReferencedErrors(CalculationModel model, IErrorsController errorsController, ICapacityController capacityController);
        void UpdateReferencedErrors();
        #endregion

        #region Блок вычислений МПИ по опорным углам для режима заправки
        void SetReferencedErrorsIn(CalculationModel model, IErrorsController errorsController, ICapacityController capacityController);
        void UpdateReferencedErrorsIn();
        #endregion

        #region Блок вычислений МПИ по опорным углам для режима выработки
        void SetReferencedErrorsOut(CalculationModel model, IErrorsController errorsController, ICapacityController capacityController);
        void UpdateReferencedErrorsOut();
        #endregion
    }
}
