using FuelMeasurement.Tools.CalculationData.Models;

namespace FuelMeasurement.Tools.CalculationData.Models
{
    public class ValuablePoint
    {
        /// <summary>
        /// Тип точки - true - датчик, false - бак
        /// </summary>
        public bool IsSensor { get; init; }

        /// <summary>
        /// Нижняя точка сущности
        /// </summary>
        public double BasePoint { get; init; }

        /// <summary>
        /// Верхняя точка сущности
        /// </summary>
        public double? NextPoint { get; init; }

        /// <summary>
        /// Бак, которому принадлежит сущность
        /// </summary>
        public TankModel Tank { get; init; }

        /// <summary>
        /// Для датчика - удельная емкость (емкость в пикофарадах на погонный миллиметр)
        /// </summary>
        public double LinearCapacity { get; init; }

        /// <summary>
        /// Внутренний (присущий) датчик
        /// </summary>
        public SensorModel IntrinsicSensor { get; init; }
    }
}
