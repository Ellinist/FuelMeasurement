namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Класс ветви проекта
    /// </summary>
    public class BranchModel
    {
        /// <summary>
        /// Модель конфигурации (углов, шага и иных параметров) ветви проекта
        /// </summary>
        public ConfigurationModel AnglesModel { get; set; } = new();

        /// <summary>
        /// Модель вычислений
        /// </summary>
        public CalculationModel CurrentModel { get; set; }

        /// <summary>
        /// Название самолета
        /// </summary>
        public string AirplaneName { get; set; }

        /// <summary>
        /// Конструктор без параметров, как того требует автомаппер
        /// </summary>
        public BranchModel() { }

        /// <summary>
        /// Конструктор ветви вычислителя
        /// </summary>
        /// <param name="configuration"></param>
        public BranchModel(ConfigurationModel configuration)
        {
            AnglesModel = configuration; // Конфигурация ветви (углы, шаг, узлы)

            CurrentModel = new(this);    // Создаем модель вычислений
        }
    }
}