namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Класс вспомогательных сущностей
    /// </summary>
    public class AuxiliaryEntity
    {
        /// <summary>
        /// Удельная емкость датчика
        /// </summary>
        public double SensorCapacity { get; init; }

        /// <summary>
        /// Присущий сущности датчик в классе вспомогательных сущностей
        /// </summary>
        public SensorModel IntrinsicSensor { get; init; }

        /// <summary>
        /// Дельта объема бака, охваченного датчиком, для текущей суперпозиции углов
        /// </summary>
        public double DeltaVolume { get; init; }
    }
}
