using FuelMeasurement.Model.Models.GeometryModels;
using FuelMeasurement.Tools.CalculationData.Models;
using FuelMeasurement.Tools.TaringModule.Interfaces;
using Prism.Events;
using NLog;
using FuelMeasurement.Client.WaitWindowModule.Interfaces;
using FuelMeasurement.Client.WaitWindowModule.Events;
using Prism.Mvvm;
using FuelMeasurement.Common.DialogModule.Interfaces;

namespace FuelMeasurement.Tools.TaringModule.Implementations
{
    /// <summary>
    /// Контроллер тарировщика
    /// </summary>
    public class TaringController : BindableBase, ITaringController
    {
        /// <summary>
        /// Аггрегатор событий
        /// </summary>
        private readonly IEventAggregator _eventAggregator;
        private static ILogger _logger;
        private readonly IWaitWindowService _waitWindowService;
        private readonly IDialogServices _dialogServices;
        private const float DefaultScale = 1_000_000; // Дефолтный коэффициент пересчета объема в литры

        private bool _activateStopTrigger = false;
        public bool ActivateStopTrigger
        {
            get => _activateStopTrigger;
            set => SetProperty(ref _activateStopTrigger, value);
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="logger"></param>
        /// <param name="waitWindowService"></param>
        public TaringController(IEventAggregator eventAggregator,
                                ILogger logger,
                                IWaitWindowService waitWindowService,
                                IDialogServices dialogServices)
        {
            _eventAggregator = eventAggregator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _waitWindowService = waitWindowService?? throw new ArgumentNullException(nameof(waitWindowService));
            _dialogServices = dialogServices;

            _ = _eventAggregator.GetEvent<WaitCancelledEvent>().Subscribe(StopTaring);
        }

        /// <summary>
        /// Добавление топливного бака в ветку проекта и тарировка по всему полю углов
        /// </summary>
        /// <param name="model">Модель вычислителя</param>
        /// <param name="id">Идентификатор топливного бака</param>
        /// <param name="tank">Топливный бак из геометрии (меш-модель)</param>
        /// <param name="name">Название бака</param>
        /// <param name="volumeScale">Коэффициент пересчета объема в литры</param>
        public void AddTankModel(CalculationModel model, string id, MeshModel tank, string name, float volumeScale = DefaultScale)
        {
            try
            {
                TankModel tankModel = new TankModel(tank, name, id);
                model.BranchTanks.Add(tankModel);

                _waitWindowService.PrimaryProgressBar.Min = 0;
                _waitWindowService.PrimaryProgressBar.Max = model.CurrentBranch.AnglesModel.PitchQuantity * model.CurrentBranch.AnglesModel.RollQuantity;
                _waitWindowService.PrimaryProgressBar.Caption = $"Бак {name} и его Id: {id}";

                GetTankVolume(model, tankModel, volumeScale); // Вычисление объема топливного бака
                TaringProcess(model, tankModel, volumeScale); // Тарировка топливного бака
                _logger.Info($"Бак Guid:{tankModel.Id} успешно добавлен и оттарирован");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка в тарировщике: {ex}");
            }
        }

        /// <summary>
        /// Процесс тарировки
        /// </summary>
        /// <param name="model">Модель вычислителя</param>
        /// <param name="tank">Топливный бак</param>
        /// <param name="volumeScale">Коэффициент пересчета объема в литры</param>
        private void TaringProcess(CalculationModel model, TankModel tank, float volumeScale = DefaultScale)
        {
            int counter = 0;

            foreach (var angle in model.AngleFields)
            {
                if (!ActivateStopTrigger)
                {
                    TarResult result = tank.GetCurve(angle.Pitch, angle.Roll, model.CurrentBranch.AnglesModel.NodesQuantity, volumeScale);
                    result.Pitch = angle.Pitch;
                    result.Roll = angle.Roll;

                    tank.TarResultList.Add(result);

                    counter++;
                    _waitWindowService.PrimaryProgressBar.Info = $"Прогресс тарировки: {Math.Round(counter / (decimal)_waitWindowService.PrimaryProgressBar.Max * 100, 2)}%";
                    _waitWindowService.PrimaryProgressBar.Value = counter;
                }
            }
        }

        /// <summary>
        /// Удаление топливного бака из модели вычислителя
        /// </summary>
        /// <param name="model">Модель вычислителя</param>
        /// <param name="id">Идентификатор топливного бака</param>
        /// <param name="tank">Топливный бак модели вычислителя</param>
        public void DeleteTankModel(CalculationModel model, string id, TankModel tank)
        {
            TankModel tankToDelete = model.BranchTanks.Find(t => t.Id == id);
            if (tankToDelete == null) return;
            model.BranchTanks.Remove(tankToDelete);
        }

        /// <summary>
        /// Метод получения объема топливного бака
        /// </summary>
        /// <param name="model">Модель вычислителя</param>
        /// <param name="tank">Топливный бак модели вычислителя</param>
        /// <param name="volumeScale">Коэффициент пересчета объема в литры</param>
        /// <returns></returns>
        public void GetTankVolume(CalculationModel model, TankModel tank, float volumeScale = DefaultScale)
        {
            if (tank == null || model == null) return;

            // Считаем объем бака
            // По умолчанию, если не задан коэффициент пересчета объема, он равен 1
            tank.TankVolume = tank.GetVolume(volumeScale <= double.Epsilon ? 1 : volumeScale);
        }
        
        /// <summary>
        /// Остановка триггера выполнения
        /// </summary>
        private void StopTaring()
        {
            if (!ActivateStopTrigger)
            {
                ActivateStopTrigger = true;
            }
        }
    }
}
