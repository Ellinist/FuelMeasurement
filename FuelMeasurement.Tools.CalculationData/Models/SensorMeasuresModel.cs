using System.Windows.Media;

namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Класс измеряемых датчиками уровней и объемов топливного бака
    /// </summary>
    public class SensorMeasuresModel
    {
        /// <summary>
        /// Внутренний (для домашнего применения) датчик
        /// </summary>
        public SensorModel IntrinsicSensor { get; set; }

        /// <summary>
        /// Номер топливного датчика
        /// </summary>
        public int SensorNumber { get; set; }

        /// <summary>
        /// Угол тангажа
        /// </summary>
        public double Pitch { get; set; }

        /// <summary>
        /// Угол крена
        /// </summary>
        public double Roll { get; set; }

        /// <summary>
        /// Верхнее показание датчика
        /// Уровень верхней кромки датчика
        /// </summary>
        public double Upper { get; set; }

        /// <summary>
        /// Нижнее показание датчика
        /// Уровень нижней кромки датчика
        /// </summary>
        public double Lower { get; set; }

        /// <summary>
        /// Относительное показание низа датчика (относительно нижней точки бака)
        /// </summary>
        public double RelativeLower { get; set; }

        /// <summary>
        /// Относительное показание верха датчика (относительно нижней точки бака)
        /// </summary>
        public double RelativeUpper { get; set; }

        /// <summary>
        /// Верхний неизмеряемый объем бака
        /// </summary>
        public double VolumeUp { get; set; }

        /// <summary>
        /// Нижний неизмеряемый объем бака
        /// </summary>
        public double VolumeDown { get; set; }

        /// <summary>
        /// Низ топливного бака для данных углов
        /// </summary>
        public double MinimumY { get; set; }

        /// <summary>
        /// Цвет бака для наследования в отрисовываемых элементах
        /// </summary>
        public Color TankColor { get; set; }

        /// <summary>
        /// Модель бака
        /// </summary>
        public TankModel TankModel { get; set; }

        public SensorMeasuresModel(SensorModel sensorModel)
        {
            IntrinsicSensor = sensorModel;
        }

        ///// <summary>
        ///// Нижняя точка датчика
        ///// </summary>
        //public Point3D SensorBottom { get; set; }

        ///// <summary>
        ///// Верхняя точка датчика
        ///// </summary>
        //public Point3D SensorTop { get; set; }

        ///// <summary>
        ///// Объем топливного бака, которому принадлежит датчик
        ///// </summary>
        //public float TankVolume { get; set; }

        ///// <summary>
        ///// Номер топливного бака
        ///// </summary>
        //public int TankNumber { get; set; }

        ///// <summary>
        ///// Название топливного бака
        ///// </summary>
        //public string TankName { get; set; }

        ///// <summary>
        ///// Удельная емкость датчика
        ///// </summary>
        //public double SensorLinearCapacity { get; set; }

        //public bool IsOff { get; set; } = true;
    }
}
