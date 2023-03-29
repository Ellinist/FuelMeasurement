using FuelMeasurement.Model.DTO.Models.ProjectModels;
using FuelMeasurement.Tools.CalculationData.Models;

namespace FuelMeasurement.Tools.TaringModule.Interfaces
{
    /// <summary>
    /// Интерфейс для создания модели вычислителя
    /// </summary>
    public interface ICalculationController
    {
        /// <summary>
        /// Создание модели вычислителя
        /// </summary>
        /// <param name="inputBranch"></param>
        /// <returns></returns>
        CalculationModel CalculationInitialize(BranchModelDTO inputBranch);

        /// <summary>
        /// Метод получения текущей модели вычислителя
        /// </summary>
        /// <returns></returns>
        CalculationModel GetCalculationModel();
    }
}
