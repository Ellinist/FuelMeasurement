namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Класс конфигурации (параметров) ветви проекта
    /// </summary>
    public class ConfigurationModel
    {
        /// <summary>
        /// Минимальное значение угла тангажа
        /// </summary>
        public double MinPitch { get; set; }

        /// <summary>
        /// Максимальное значение угла тангажа
        /// </summary>
        public double MaxPitch { get; set; }

        /// <summary>
        /// Минимальное значение угла крена
        /// </summary>
        public double MinRoll { get; set; }

        /// <summary>
        /// Максимальное значение угла крена
        /// </summary>
        public double MaxRoll { get; set; }

        /// <summary>
        /// Шаг приращения угла тангажа
        /// </summary>
        public double PitchStep { get; set; }

        /// <summary>
        /// Шаг приращения угла крена
        /// </summary>
        public double RollStep { get; set; }

        /// <summary>
        /// Количество расчетных точек (срезов)
        /// </summary>
        public int NodesQuantity { get; set; }

        /// <summary>
        /// Опорный угол тангажа
        /// </summary>
        public double ReferencedPitch { get; set; }

        /// <summary>
        /// Опорный угол крена
        /// </summary>
        public double ReferencedRoll { get; set; }

        /// <summary>
        /// Количество углов тангажа
        /// Требуется для слайдеров UI и для тарировщика
        /// </summary>
        public int PitchQuantity { get; set; }

        /// <summary>
        /// Количество углов крена
        /// Требуется для слайдеров UI и для тарировщика
        /// </summary>
        public int RollQuantity { get; set; }

        public ConfigurationModel()
        {

        }
    }
}
