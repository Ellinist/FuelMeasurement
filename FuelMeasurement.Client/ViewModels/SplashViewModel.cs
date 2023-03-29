using FuelMeasurement.Common.Events.SplashEvents;
using NLog;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FuelMeasurement.Client.ViewModels
{
    internal class SplashViewModel : BindableBase, IDisposable, IActionInViewModel
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;

        private ObservableCollection<string> _messages = new();
        private bool _disposed = false;
        private bool _closeTrigger = false;

        private readonly Dispatcher _dispatcher;

        public ObservableCollection<string> Messages
        {
            get => _messages;
            set => SetProperty(ref _messages, value);
        }

        public bool CloseTrigger
        {
            get => _closeTrigger;
            set => SetProperty(ref _closeTrigger, value);
        }

        private readonly SubscriptionToken _messageSubscription1;
        private readonly SubscriptionToken _messageSubscription2;

        public SplashViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public SplashViewModel(
            IEventAggregator eventAggregator,
            ILogger logger
            ) : this()
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            _messageSubscription1 = _eventAggregator.GetEvent<SplashMessage>().Subscribe(OnUpdateMessage, ThreadOption.PublisherThread);
            _messageSubscription2 = _eventAggregator.GetEvent<CloseSplashEvent>().Subscribe(OnCloseSplash);
        }

        private void OnCloseSplash()
        {
            CloseTrigger = true;
        }

        private void OnUpdateMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            ActionInViewModel(() => 
            {
                Messages.Add(message);
                //Thread.Sleep(500); //не ну и зачем искусственно затягивать загрузку на (N модулей * на 500 мс) чтобы мы на сплэш смотрели? а я то думаю че грузится так долго
            });
        }

        public void ActionInViewModel(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _dispatcher.Invoke(priority,
                new Action(() =>
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }));
        }

        public async Task ActionInViewModelAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            await _dispatcher.BeginInvoke(priority,
                new Action(() =>
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }));
        }




        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _messageSubscription1?.Dispose();
                _messageSubscription2?.Dispose();
                Messages.Clear();
            }

            _disposed = true;
        }

        public void InvokeShutdown()
        {
            _dispatcher.InvokeShutdown();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SplashViewModel()
        {
            Dispose(false);
        }
    }
}
