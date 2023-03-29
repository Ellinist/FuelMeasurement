using FuelMeasurement.Client.WaitWindowModule.Events;
using FuelMeasurement.Client.WaitWindowModule.Interfaces;
using FuelMeasurement.Client.WaitWindowModule.ViewModels;
using FuelMeasurement.Client.WaitWindowModule.Views;
using Prism.Events;
using Prism.Ioc;
using System;

namespace FuelMeasurement.Client.WaitWindowModule.Models
{
    public class WaitWindowService: IWaitWindowService
    {
        private readonly IContainerProvider _containerProvider;
        private readonly IEventAggregator _eventAggregator;
        private WaitWindow? _waitWindow;
        private WaitWindowViewModel _vm;

        public WaitWindowService(
            IContainerProvider containerProvider,
            IEventAggregator eventAggregator
            )
        {
            _containerProvider = containerProvider;
            _eventAggregator = eventAggregator;
            _vm = _containerProvider.Resolve<WaitWindowViewModel>();

        }

        public ProgressBarSettings PrimaryProgressBar
        {
            get { return _vm.PrimaryProgressBar; }
            set
            {
                _vm.PrimaryProgressBar = value;
            }
        }

        public ProgressBarSettings SecondaryProgressBar
        {
            get { return _vm.SecondaryProgressBar; }
            set
            {
                _vm.SecondaryProgressBar = value;
            }
        }

        public void HideSecondaryProgressBar()
        {
            _vm.SecondaryProgressBar.Show = false;
            _vm.SecondaryProgressBar.ShowCaption = false;
            _vm.SecondaryProgressBar.ShowInfo = false;
        }

        public void ShowSecondaryProgressBar()
        {
            _vm.SecondaryProgressBar.Show = true;
            _vm.SecondaryProgressBar.ShowCaption = true;
            _vm.SecondaryProgressBar.ShowInfo = true;
        }

        public void AddMessage(string message,bool addTime = true)
        {
            string mes = string.Format("{0} {1}", DateTime.Now.ToLongTimeString(), message);
            _vm.AddMessage(mes);
        }

        public void ClearMessage()
        {
            _vm.ClearMessage();
        }

        public void Show(bool showSecondary = false, bool showCancelButton = false)
        {
            if (showSecondary)
            {
                ShowSecondaryProgressBar();
            }
            else
            {
                HideSecondaryProgressBar();
            }

            _vm.ShowCancelButton = showCancelButton;

            if ((_waitWindow is null) || (!_waitWindow.IsLoaded))
            {
                _waitWindow?.Close();
                _waitWindow = new WaitWindow();
                _waitWindow.DataContext = _vm;
            }

            _vm.Show(_waitWindow);
        }

        public void SetTitle(string title)
        {
            _vm.Title = title;
        }

        public void Close()
        {
            if (_waitWindow != null)
                _vm.Close(_waitWindow);
            _waitWindow = null;
            _eventAggregator.GetEvent<WaitWindowClosedEvent>().Publish();
        }
    }
}
