using FuelMeasurement.Common.Enums;
using FuelMeasurement.Model.DTO.Models.AirplaneModels;
using FuelMeasurement.Model.Models.GeometryModels;
using FuelMeasurement.Tools.CalculationData.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

namespace FuelMeasurement.Tools.Compute.Interfaces
{
    /// <summary>
    /// Интерфейс расчетчика при расстановке топливных датчиков
    /// </summary>
    public interface IComputationController
    {
        /// <summary>
        /// Инициализация расчетчика
        /// для выбранных топливных баков модели вычислителя
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tanks"></param>
        void GraphicsInitialize(CalculationModel model, ObservableCollection<FuelTankGeometryModel> tanks);

        /// <summary>
        /// Добавление датчика
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tank"></param>
        /// <param name="sensor"></param>
        void AddSensor(CalculationModel model, string tankId, SensorModelDTO sensor);

        /// <summary>
        /// Удаление датчика
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tank"></param>
        /// <param name="sensor"></param>
        void DeleteSensor(CalculationModel model, SensorModelDTO sensor);

        /// <summary>
        /// Движение датчика или изменение его параметров
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tank"></param>
        /// <param name="sensor"></param>
        void MoveSensor(CalculationModel model, SensorModel sensor, Point3D up, Point3D down);
    }
}
