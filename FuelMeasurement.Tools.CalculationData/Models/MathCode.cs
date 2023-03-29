using System;

namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Класс математических выражений
    /// </summary>
    public static class MathCode
    {
        /// <summary>
        /// Метод перевода градусов в радианы
        /// </summary>
        /// <param name="deg">Градусы</param>
        /// <returns>Радианы</returns>
        public static float ToRadians(this double deg) => (float)Math.Round((deg * Math.PI / 180), 2);
    }
}
