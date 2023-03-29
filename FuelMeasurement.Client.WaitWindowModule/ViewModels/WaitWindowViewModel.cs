using FuelMeasurement.Client.WaitWindowModule.Events;
using FuelMeasurement.Client.WaitWindowModule.Models;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace FuelMeasurement.Client.WaitWindowModule.ViewModels
{
    internal class WaitWindowViewModel: BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger? _logger;

        private readonly Dispatcher _dispatcher;

        private ObservableCollection<string> _messages = new ObservableCollection<string>();
        /// <summary>
        /// Коллекция сообщений для отображения
        /// </summary>
        public ObservableCollection<string> Messages
        {
            get => _messages;
            set => SetProperty(ref _messages, value);
        }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public ProgressBarSettings PrimaryProgressBar { get => _primaryProgressBar; set => SetProperty(ref _primaryProgressBar, value); }
        public ProgressBarSettings SecondaryProgressBar { get => _secondaryProgressBar; set => SetProperty(ref _secondaryProgressBar, value); }
        private bool _showCancelButton = false;
        /// <summary>
        /// Флаг отображения кнопки отмены
        /// </summary>
        public bool ShowCancelButton
        {
            get => _showCancelButton;
            set => _showCancelButton = value;
        }

        private string _title = "Подождите, пожалуйста";

        private ProgressBarSettings _primaryProgressBar = new ProgressBarSettings();
        private ProgressBarSettings _secondaryProgressBar = new ProgressBarSettings();

        /// <summary>
        /// Команда отмены - выполняется при нажатии на кнопку отмены
        /// </summary>
        public DelegateCommand? CancelCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="eventAggregator"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public WaitWindowViewModel(
             ILogger logger,
             IEventAggregator eventAggregator
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _dispatcher = Dispatcher.CurrentDispatcher;
            CommandsInitialization();
        }

        /// <summary>
        /// Инициализация команд - пока только отмены
        /// </summary>
        private void CommandsInitialization()
        {
            CancelCommand = new DelegateCommand(
                () =>
                {
                    _eventAggregator.GetEvent<WaitCancelledEvent>().Publish();
                }
                );
        }

        internal void ClearMessage()
        {
            _dispatcher.Invoke(() =>
            { Messages.Clear(); });
        }

        internal void AddMessage(string message)
        {
            _dispatcher.Invoke(()=>
            { Messages.Insert(0,message); });
        }

        internal void Close(Window window)
        {
            _dispatcher.Invoke(() =>
            { window.Close(); });
        }

        internal void Show(Window window)
        {
            _dispatcher.Invoke(() =>
            { window.Show(); });
        }

    }
}
