using Assimp;
using System.Collections.Generic;

namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Результаты тарировки топливного бака
    /// </summary>
    public class TarResult
    {
        /// <summary>
        /// Угол тангажа
        /// </summary>
        public double Pitch { get; set; }

        /// <summary>
        /// Угол крена
        /// </summary>
        public double Roll { get; set; }

        /// <summary>
        /// Список данных объемов по срезам топливного бака
        /// </summary>
        public List<double> ResultAxisX { get; set; } = new();

        /// <summary>
        /// Список уровней срезов топливного бака (абсолютные величины в мировой системе координат)
        /// </summary>
        public List<double> ResultAxisY { get; set; } = new();

        /// <summary>
        /// Список уровней срезов топливного бака (относительные величины - от нуля и выше)
        /// </summary>
        public List<double> RelativeResultAxisY { get; set; } = new();

        /// <summary>
        /// Нижняя точка топливного бака для данных углов тангажа и крена
        /// </summary>
        public Vector3D TankBottom { get; set; }

        /// <summary>
        /// Верхняя точка топливного бака для данных углов тангажа и крена
        /// </summary>
        public Vector3D TankTop { get; set; }
    }
}
