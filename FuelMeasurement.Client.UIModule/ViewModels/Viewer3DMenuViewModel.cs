using FuelMeasurement.Client.UIModule.Events;
using FuelMeasurement.Client.UIModule.Infrastructure;
using FuelMeasurement.Common.Enums;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FuelMeasurement.Client.UIModule.ViewModels
{
    public class Viewer3DMenuViewModel : BindableBase, IActionInViewModel
    {
        private Dispatcher _dispatcher;
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;

        public string SwitchViewerModeHeader
        {
            get
            {
                switch (ViewerWorkingMode)
                {
                    case ViewerWorkingModes.Manipulating:
                        return RenderingConstants.SwitchViewerManipulationModeHeader;
                    case ViewerWorkingModes.View:
                        return RenderingConstants.SwitchViewerViewModeHeader;
                    default:
                        return string.Empty;
                }
            }
        }

        public ViewerWorkingModes ViewerWorkingMode
        {
            get => _viewerWorkingMode;
            set 
            {
                if(SetProperty(ref _viewerWorkingMode, value))
                {
                    RaisePropertyChanged(nameof(SwitchViewerModeHeader));
                }
            } 
        }
        private ViewerWorkingModes _viewerWorkingMode = ViewerWorkingModes.View;


        public DelegateCommand SwitchViewerModeCommand
        {
            get => _switchViewerModeCommand;
            protected set => SetProperty(ref _switchViewerModeCommand, value);
        }
        private DelegateCommand _switchViewerModeCommand;



        public Viewer3DMenuViewModel(
            ILogger logger, 
            IEventAggregator eventAggregator
            )
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            InitializeCommand();
        }

        private void InitializeCommand()
        {
            SwitchViewerModeCommand = new DelegateCommand(SwitchViewerModeCommandExecute);
        }

        private void SwitchViewerModeCommandExecute()
        {
            var switchEvent = _eventAggregator
                .GetEvent<SwitchViewerModeEvent>();

            switch (ViewerWorkingMode)
            {
                case ViewerWorkingModes.View:
                    ViewerWorkingMode = ViewerWorkingModes.Manipulating;
                    switchEvent.Publish(ViewerWorkingModes.Manipulating);
                    break;
                case ViewerWorkingModes.Manipulating:
                    ViewerWorkingMode = ViewerWorkingModes.View;
                    switchEvent.Publish(ViewerWorkingModes.View);
                    break;
            }
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
    }
}
