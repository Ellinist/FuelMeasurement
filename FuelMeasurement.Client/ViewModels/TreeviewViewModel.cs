using AutoMapper;
using FuelMeasurement.Client.Models;
using FuelMeasurement.Client.Services.Interfaces;
using FuelMeasurement.Common.Events.ProjectEvents;
using FuelMeasurement.Common.Events.SelectionEvents;
using FuelMeasurement.Model.DTO.Models.ProjectModels;
using NLog;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FuelMeasurement.Client.ViewModels
{
    internal class TreeviewViewModel : BindableBase, IDisposable, IActionInViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ISelectionManager _selectionManager;

        private readonly Dispatcher _dispatcher;

        private bool _disposed = false;

        private readonly SubscriptionToken _projectLoadedToken;

        private ProjectModel _currentProject = null;
        public ProjectModel CurrentProject
        {
            get => _currentProject;
            set => SetProperty(ref _currentProject, value);
        }

        private ObservableCollection<ProjectModel> _projects = new();
        public ObservableCollection<ProjectModel> Projects
        {
            get => _projects;
            set => SetProperty(ref _projects, value);
        }

        private ModelBase _selectedItem = null;
        public ModelBase SelectedItem
        {
            get => _selectedItem;
            set 
            {
                if(SetProperty(ref _selectedItem, value))
                {
                    _selectionManager.SetSelectedItem(SelectedItem);

                    if (SelectedItem != null)
                    {
                        PublishEvent(true);
                    }
                    else
                    {
                        PublishEvent(false);
                    }
                }
            } 
        }

        public TreeviewViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public TreeviewViewModel
            (
            IEventAggregator eventAggregator,
            ILogger logger,
            IMapper mapper,
            ISelectionManager selectionManager
            ) : this()
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _selectionManager = selectionManager ?? throw new ArgumentNullException(nameof(selectionManager));

            _projectLoadedToken = _eventAggregator.GetEvent<ProjectLoaded>().Subscribe(OnProjectLoaded);

            InitializeCommands();
        }

        private void InitializeCommands()
        {

        }

        private void PublishEvent(bool isVisible)
        {
            _eventAggregator.GetEvent<TreeViewSelectionChangedEvent>().Publish(isVisible);
        }

        private void OnProjectLoaded(object obj)
        {
            if(obj is ProjectModelDTO project)
            {
                var projectModel = _mapper.Map<ProjectModel>(project);

                ActionInViewModel(() => 
                {
                    Projects.Clear();
                    _projects.Add(projectModel);

                    CurrentProject = projectModel;
                });
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


        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _projectLoadedToken?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TreeviewViewModel()
        {
            Dispose(false);
        }
    }
}
