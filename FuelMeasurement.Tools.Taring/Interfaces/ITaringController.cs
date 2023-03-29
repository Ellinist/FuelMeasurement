using FuelMeasurement.Model.Models.GeometryModels;
using FuelMeasurement.Tools.CalculationData.Models;

namespace FuelMeasurement.Tools.TaringModule.Interfaces
{
    /// <summary>
    /// Интерфейс тарировки топливных баков
    /// </summary>
    public interface ITaringController
    {
        /// <summary>
        /// Добавление нового бака и его тарировка
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tank"></param>
        /// <param name="volumeScale"></param>
        void AddTankModel(CalculationModel model, string id, MeshModel tank, string name, float volumeScale);

        /// <summary>
        /// Удаление топливного бака из ветки проекта
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tank"></param>
        void DeleteTankModel(CalculationModel model, string id, TankModel tank);

        /// <summary>
        /// Вычисление объема топливного бака
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tank"></param>
        /// <param name="volumeScale"></param>
        /// <returns></returns>
        void GetTankVolume(CalculationModel model, TankModel tank, float volumeScale);
    }
}
