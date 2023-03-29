using AutoMapper;
using FuelMeasurement.Model.DTO.Models.ProjectModels;
using FuelMeasurement.Tools.CalculationData.Models;
using FuelMeasurement.Tools.TaringModule.Interfaces;

namespace FuelMeasurement.Tools.TaringModule.Implementations
{
    /// <summary>
    /// Контроллер вычислителя
    /// </summary>
    public class CalculationController : ICalculationController
    {
        private readonly IMapper _mapper;
        public CalculationController(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Получение текущей модели вычислителя
        /// </summary>
        /// <returns></returns>
        public CalculationModel GetCalculationModel()
        {
            return CurrentBranch.CurrentModel;
        }

        /// <summary>
        /// Текущая ветвь вычислителя
        /// </summary>
        public BranchModel CurrentBranch { get; set; }

        /// <summary>
        /// Инициализация модели вычислителя
        /// Выполняется каждый раз, когда меняется ветка проекта
        /// </summary>
        /// <param name="inputBranch">Ветка проекта</param>
        /// <returns>Модель вычислителя, пробрасываемая в UI</returns>
        public CalculationModel CalculationInitialize(BranchModelDTO inputBranch)
        {
            // При создании локальной ветки автоматически создается и текущая модель вычислителя
            CurrentBranch = new BranchModel(_mapper.Map<BranchModel>(inputBranch).AnglesModel)
            {
                AirplaneName = inputBranch.Name
            };// Создаем ветку вычислителя

            return CurrentBranch.CurrentModel; // Возвращаем портрет в UI - а нужен ли он там? Вот в чем вопрос!
        }
    }
}
