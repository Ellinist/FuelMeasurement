using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Модель топливного датчика
    /// </summary>
    public class SensorModel
    {
        /// <summary>
        /// Номер датчика в метавселенной
        /// </summary>
        public int SensorPositionInTank { get; set; }

        /// <summary>
        /// Id датчика
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// Название датчика
        /// </summary>
        public string SensorName { get; set; }

        /// <summary>
        /// Бак, которому принадлежит датчик
        /// </summary>
        public TankModel FuelTank;

        /// <summary>
        /// Верхняя точка датчика в мировой системе координат
        /// </summary>
        public Point3D UpPoint { get; set; }
        /// <summary>
        /// Нижняя точка датчика в мировой системе координат
        /// </summary>
        public Point3D DownPoint { get; set; }

        /// <summary>
        /// Список измеренных датчиком величин для поля углов
        /// </summary>
        public List<SensorMeasuresModel> SensorMeasuresList = new List<SensorMeasuresModel>();

        /// <summary>
        /// Флаг активности датчика
        /// </summary>
        public bool IsActiveSensor;

        /// <summary>
        /// Длина топливного датчика
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// Удельная емкость датчика
        /// </summary>
        public double SensorLinearCapacity { get; set; }

        public double UpIndent { get; set; }
        public double DownIndent { get; set; }
        public string GroupName { get; set; }

        #region Данные для отчетов (для таблицы привязки датчиков к ТХ) для опорных углов
        public double LowFuelLevel { get; set; }
        public double HighFuelLevel { get; set; }
        public double LowFuelVolume { get; set; }
        public double HighFuelVolume { get; set; }
        #endregion
    }
}
