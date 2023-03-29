namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Класс комбинации углов тангажа и крена
    /// </summary>
    public class AnglesPair
    {
        /// <summary>
        /// Угол тангажа
        /// </summary>
        public double Pitch { get; set; }

        /// <summary>
        /// Угол крена
        /// </summary>
        public double Roll { get; set; }
    }
}