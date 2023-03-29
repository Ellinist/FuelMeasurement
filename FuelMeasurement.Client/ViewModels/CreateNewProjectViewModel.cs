using FuelMeasurement.Common.DialogModule.Interfaces;
using FuelMeasurement.Common.Constants;
using FuelMeasurement.Common.Events;
using FuelMeasurement.Common.Events.ProjectEvents;
using FuelMeasurement.Common.SupportedFileFormats;
using FuelMeasurement.Model.DTO.Models.AirplaneModels;
using FuelMeasurement.Model.DTO.Models.ProjectModels;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FuelMeasurement.Common.DialogModule.Implementations;
using AutoMapper;
using FuelMeasurement.Tools.FileManager.Interfaces;
using System.Collections.Generic;

namespace FuelMeasurement.Client.ViewModels
{
    internal class CreateNewProjectViewModel : BindableBase, IDisposable, IActionInViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogServices _dialogServices;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IFileFilterManager _fileFilterManager;

        private bool _disposed = false;

        private readonly Dispatcher _dispatcher;

        public DelegateCommand<object> CloseViewCommand { get; private set; }
        public DelegateCommand SetProjectPathCommand { get; private set; }
        public DelegateCommand SetProjectNameCommand { get; private set; }
        public DelegateCommand SetAuthorCommand { get; private set; }
        public DelegateCommand CreateNewBranchCommand { get; private set; }
        public DelegateCommand AddNewFuelTankCommand { get; private set; }
        public DelegateCommand AddNewInsideModelFuelTankCommand { get; private set; }

        public DelegateCommand CreateProjectCommand { get; private set; }
        /// <summary>
        /// Команда создания модели вычислителя
        /// </summary>
        public DelegateCommand CreateCalcModelCommand { get; private set; }

        public DelegateCommand<object> EditFuelTankCommand { get; private set; }

        private string _filePath = null;
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        private string _projectName = null;

        public string ProjectName
        {
            get => _projectName;
            set => SetProperty(ref _projectName, value);
        }

        private string _projectAuthor = null;
        public string ProjectAuthor
        {
            get => _projectAuthor;
            set => SetProperty(ref _projectAuthor, value);
        }

        private string _branchName = null;
        public string BranchName
        {
            get => _branchName;
            set 
            {
                SetProperty(ref _branchName, value);
            } 
        }

        private ObservableCollection<FuelTankModelDTO> _branchFuelTanks = new();
        public ObservableCollection<FuelTankModelDTO> BranchFuelTanks
        {
            get => _branchFuelTanks;
            set => SetProperty(ref _branchFuelTanks, value);
        }

        private ObservableCollection<InsideModelFuelTankModelDTO> _insideModelsBranchFuelTanks = new();
        public ObservableCollection<InsideModelFuelTankModelDTO> InsideModelsBranchFuelTanks
        {
            get => _insideModelsBranchFuelTanks;
            set => SetProperty(ref _insideModelsBranchFuelTanks, value);
        }


        private bool _branchFuelTanksVisibility = false;
        public bool BranchFuelTanksVisibility
        {
            get => _branchFuelTanksVisibility;
            set => SetProperty(ref _branchFuelTanksVisibility, value);
        }


        private bool _useDefaultConfiguration = true;
        /// <summary>
        /// Флаг применения конфигурации по умолчанию
        /// </summary>
        public bool UseDefaultConfiguration
        {
            get => _useDefaultConfiguration;
            set => SetProperty(ref _useDefaultConfiguration, value);
        }


        public CreateNewProjectViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public CreateNewProjectViewModel
            (
            IEventAggregator eventAggregator,
            IDialogServices dialogServices,
            ILogger logger,
            IMapper mapper,
            IFileFilterManager fileFilterManager
            ) : this()
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _dialogServices = dialogServices ?? throw new ArgumentNullException(nameof(dialogServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _fileFilterManager = fileFilterManager ?? throw new ArgumentNullException(nameof(fileFilterManager));

            InitializeCommand();

#if DEBUG
            BranchName = "Новая ветка";
            ProjectAuthor = "Дебаг автор";
            FilePath = "Новая папка";
            ProjectName = "Новый проект";

            BranchFuelTanksVisibility = true;
#endif
        }

        private void InitializeCommand()
        {
            CloseViewCommand = new DelegateCommand<object>(OnCloseView);
            SetProjectPathCommand = new DelegateCommand(SetProjectPath);
            SetProjectNameCommand = new DelegateCommand(SetProjectName);
            SetAuthorCommand = new DelegateCommand(SetAuthor);
            CreateNewBranchCommand = new DelegateCommand(CreateNewBranch);
            AddNewFuelTankCommand = new DelegateCommand(AddNewFuelTank);
            EditFuelTankCommand = new DelegateCommand<object>(EditFuelTank);
            CreateProjectCommand = new DelegateCommand(CreateProject);
            AddNewInsideModelFuelTankCommand = new DelegateCommand(AddNewInsideModelFuelTank);
        }

        

        private bool CanCreateProject()
        {
            if (string.IsNullOrWhiteSpace(ProjectName) &&
                string.IsNullOrWhiteSpace(ProjectAuthor) &&
                string.IsNullOrWhiteSpace(BranchName) &&
                string.IsNullOrWhiteSpace(FilePath))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void CreateProject()
        {
            if (CanCreateProject())
            {
                BranchConfigurationModelDTO defaultConfiguration = null;

                if (UseDefaultConfiguration)
                {
                    defaultConfiguration = new() 
                    {
                        MinPitch                = BranchConfigurationModelDefault.MinPitch,
                        MaxPitch                = BranchConfigurationModelDefault.MaxPitch,
                        MinRoll                 = BranchConfigurationModelDefault.MinRoll,
                        MaxRoll                 = BranchConfigurationModelDefault.MaxRoll,
                        PitchStep               = BranchConfigurationModelDefault.PitchStep,
                        RollStep                = BranchConfigurationModelDefault.RollStep,
                        NodesQuantity           = BranchConfigurationModelDefault.NodesQuantity,
                        ReferencedPitch         = BranchConfigurationModelDefault.ReferencedPitch,
                        ReferencedRoll          = BranchConfigurationModelDefault.ReferencedRoll,
                        Coefficient             = BranchConfigurationModelDefault.Coefficient,
                        LengthCoef              = BranchConfigurationModelDefault.LengthCoef,
                        DefaultMinIndent        = BranchConfigurationModelDefault.DefaultMinIndent,
                        DefaultUpIndent         = BranchConfigurationModelDefault.DefaultUpIndent,
                        DefaultDownIndent       = BranchConfigurationModelDefault.DefaultDownIndent,
                        VisibleSensorDiametr    = BranchConfigurationModelDefault.VisibleSensorDiametr,
                        SensorVisualDiametrType = BranchConfigurationModelDefault.SensorVisualDiametrType
                    };
                }

                string projectId = Guid.NewGuid().ToString();

                ProjectModelDTO project = new()
                {
                    Id = projectId,
                    Name = ProjectName,
                    Creation = DateTime.Now,
                    ProjectAuthor = ProjectAuthor,
                    Configurations = new ObservableCollection<ConfigurationModelDTO>() 
                    {
                        new ConfigurationModelDTO()
                        {
                            ProjectId = projectId,
                            Id = Guid.NewGuid().ToString(),
                            Name = "Новая конфигурация",
                            FuelTanks = new ObservableCollection<TankModelDTO>(BranchFuelTanks.Cast<TankModelDTO>()),
                            InsideModelFuelTanks = new ObservableCollection<TankModelDTO>(InsideModelsBranchFuelTanks.Cast<TankModelDTO>()),
                            Branches = new ObservableCollection<BranchModelDTO>()
                            {
                                 new BranchModelDTO
                                 {
                                     ProjectId = projectId,
                                     Id= Guid.NewGuid().ToString(),
                                     Name = BranchName,
                                     Creation = DateTime.Now,
                                     Type = Common.Enums.BranchType.Working,
                                     TankInGroups = new ObservableCollection<TankGroupModelDTO>()
                                     {
                                         new TankGroupModelDTO()
                                         {
                                             ProjectId = projectId,
                                             Id = Guid.NewGuid().ToString(),
                                             Name = "Новая группа заправки",
                                             FuelTanksInGroup = new ObservableCollection<string>()
                                         }
                                     },
                                     TankOutGroups = new ObservableCollection<TankGroupModelDTO>()
                                     {
                                         new TankGroupModelDTO()
                                         {
                                             ProjectId = projectId,
                                             Id = Guid.NewGuid().ToString(),
                                             Name = "Новая группа выработки",
                                             FuelTanksInGroup = new ObservableCollection<string>()
                                         }
                                     },
                                     Configuration = defaultConfiguration,
                                     FuelTanks = new ObservableCollection<FuelTankModelDTO>(BranchFuelTanks)
                                 }
                                 ,
                                 new BranchModelDTO
                                 {
                                     ProjectId = projectId,
                                     Id= Guid.NewGuid().ToString(),
                                     Name = "Какая то ветка",
                                     Creation = DateTime.Now,
                                     Type = Common.Enums.BranchType.Default,
                                     TankInGroups = new ObservableCollection<TankGroupModelDTO>()
                                     {
                                         new TankGroupModelDTO()
                                         {
                                             ProjectId = projectId,
                                             Id = Guid.NewGuid().ToString(),
                                             Name = "Новая группа заправки",
                                             FuelTanksInGroup = new ObservableCollection<string>()
                                         }
                                     },
                                     TankOutGroups = new ObservableCollection<TankGroupModelDTO>()
                                     {
                                         new TankGroupModelDTO()
                                         {
                                             ProjectId = projectId,
                                             Id = Guid.NewGuid().ToString(),
                                             Name = "Новая группа выработки",
                                             FuelTanksInGroup = new ObservableCollection<string>()
                                         }
                                     },
                                     Configuration = defaultConfiguration,
                                     FuelTanks = new ObservableCollection<FuelTankModelDTO>(BranchFuelTanks.Take(1))
                                 }

                            },
                            Type = Common.Enums.ConfigurationType.Working
                        },
                        new ConfigurationModelDTO()
                        {
                            ProjectId = projectId,
                            Id = Guid.NewGuid().ToString(),
                            Name = "Какая то конфигурация"
                        }
                    }
                };

                OnCloseView(false);
                _eventAggregator.GetEvent<ProjectLoaded>().Publish(project);
            }
            else
            {
                MessageBox.Show(
                    "Заполните все поля",
                    "Создание проекта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                    );
            }
        }

        private void EditFuelTank(object obj)
        {
            if(obj is FuelTankModelDTO tank)
            {

            }
        }

        private void AddNewFuelTank()
        {
            ActionInViewModel(() =>
            {
                _dialogServices.OpenFileDialog(
                    _fileFilterManager.GetFilesFilter(new List<Type>() 
                    {
                        typeof(FileSTL),
                        typeof(FileTRI), 
                        typeof(FileTXT) }
                    ),
                    true,
                    dialogResult =>
                    {
                        if(dialogResult.Any())
                        {
                            StringBuilder sb = new();

                            foreach (var file in dialogResult)
                            {
                                var name = Path.GetFileNameWithoutExtension(file);

                                var findTanks = BranchFuelTanks.Where(tank => tank.GeometryFilePath == file);

                                if(!findTanks.Any())
                                {
                                    string tankId = Guid.NewGuid().ToString();

                                    var tank = new FuelTankModelDTO()
                                    {
                                        Id = tankId,
                                        Name = name,
                                        GeometryFilePath = file,
                                        Sensors = new ObservableCollection<SensorModelDTO>() 
                                        {
                                            //new SensorModelDTO()
                                            //{
                                            //    TankId = tankId,
                                            //    Id = Guid.NewGuid().ToString(),
                                            //    Name = "Датчик 1",
                                            //    UpPoint = new System.Numerics.Vector3(0,0,0),
                                            //    DownPoint = new System.Numerics.Vector3(0,5,0)
                                            //},
                                            //new SensorModelDTO()
                                            //{
                                            //    TankId = tankId,
                                            //    Id = Guid.NewGuid().ToString(),
                                            //    Name = "Датчик 2",
                                            //    UpPoint = new System.Numerics.Vector3(0,0,0),
                                            //    DownPoint = new System.Numerics.Vector3(0,5,0)
                                            //}
                                        }
                                    };

                                    BranchFuelTanks.Add(tank);
                                }
                                else
                                {
                                    sb.Append(name);
                                    sb.AppendLine();
                                }
                            }

                            var message = sb.ToString();
                            sb.Clear();

                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                sb.Append("Баки уже добавлены в список:");
                                sb.AppendLine();
                                sb.AppendLine(message);

                                MessageBox.Show(sb.ToString());
                            }
                        }
                    }
                );
            });
        }

        private void AddNewInsideModelFuelTank()
        {
            ActionInViewModel(() =>
            {
                _dialogServices.OpenFileDialog(
                    _fileFilterManager.GetFilesFilter(new List<Type>() 
                    {
                        typeof(FileSTL), 
                        typeof(FileTRI), 
                        typeof(FileTXT) }
                    ),
                    true,
                    dialogResult =>
                    {
                        if (dialogResult.Any())
                        {
                            StringBuilder sb = new();

                            foreach (var file in dialogResult)
                            {
                                var name = Path.GetFileNameWithoutExtension(file);

                                var findTanks = InsideModelsBranchFuelTanks.Where(tank => tank.GeometryFilePath == file);

                                if (!findTanks.Any())
                                {
                                    var insideModel = new InsideModelFuelTankModelDTO()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        Name = name,
                                        GeometryFilePath = file
                                    };

                                    InsideModelsBranchFuelTanks.Add(insideModel);
                                }
                                else
                                {
                                    sb.Append(name);
                                    sb.AppendLine();
                                }
                            }

                            var message = sb.ToString();
                            sb.Clear();

                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                sb.Append("Внутренность баков уже добавлены в список:");
                                sb.AppendLine();
                                sb.AppendLine(message);

                                MessageBox.Show(sb.ToString());
                            }
                        }
                    }
                );
            });
        }


        private void CreateNewBranch()
        {
            ActionInViewModel(() =>
            {
                _dialogServices.ShowInputTextDialog(
                    "Новая ветка",
                    "Введите название ветки",
                    BranchName ?? "Новая ветка",
                    dialogResult =>
                    {
                        var name = dialogResult.Parameters.GetValue<string>(DialogParametersNames.InputText);

                        if (dialogResult.Result == Prism.Services.Dialogs.ButtonResult.OK)
                        {
                            BranchName = name;
                            if(!string.IsNullOrWhiteSpace(BranchName))
                            {
                                BranchFuelTanksVisibility = true;
                            }
                            else
                            {
                                BranchFuelTanksVisibility = false;
                                BranchFuelTanks.Clear();
                                InsideModelsBranchFuelTanks.Clear();
                            }
                        }
                    }
                );
            });
        }

        private void SetAuthor()
        {
            ActionInViewModel(() =>
            {
                _dialogServices.ShowInputTextDialog(
                    "Автор",
                    "Введите автора проекта",
                    ProjectAuthor ?? "Ф.И.О",
                    dialogResult =>
                    {
                        var name = dialogResult.Parameters.GetValue<string>(DialogParametersNames.InputText);

                        if (dialogResult.Result == Prism.Services.Dialogs.ButtonResult.OK)
                        {
                            ProjectAuthor = name;
                        }
                    }
                );
            });
        }

        private void SetProjectName()
        {
            ActionInViewModel(() =>
            {
                _dialogServices.ShowInputTextDialog(
                    "Название проекта",
                    "Введите название проекта",
                    ProjectName ?? "Новый проект",
                    dialogResult =>
                    {
                        var name = dialogResult.Parameters.GetValue<string>(DialogParametersNames.InputText);

                        if (dialogResult.Result == Prism.Services.Dialogs.ButtonResult.OK)
                        {
                            ProjectName = name;
                        }
                    }
                );
            });
        }

        private void SetProjectPath()
        {
            ActionInViewModel(() =>
            {
                _dialogServices.SaveFileDialog(
                    _fileFilterManager.GetFileFilter(typeof(FileXML)),
                    FilePath ?? "Новый проект",
                    filepath => 
                    {
                        if(!string.IsNullOrWhiteSpace(filepath))
                        {
                            FilePath = filepath;
                        }
                    }
                );
            });
        }

        private void OnCloseView(object isCancel)
        {
            _eventAggregator.GetEvent<CloseViewEvent>().Publish(new Common.Models.CloseViewParams() 
            {
                Type = Common.Enums.ViewType.CreateNewProjectView,
                IsCancle = (bool)isCancel
            });

            // Пока в процессе разработки не чистить это читим только баки и внутренности!!!!
            //BrunchFuelTanksVisibility = false;
            //BranchName = null;
            //ProjectAuthor = null;
            //ProjectName = null;
            //FilePath = null;
            BranchFuelTanks.Clear();
            InsideModelsBranchFuelTanks.Clear();
        }

        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                InsideModelsBranchFuelTanks.Clear();
                BranchFuelTanks.Clear();
                BranchFuelTanksVisibility = false;
                BranchName = null;
                ProjectAuthor = null;
                ProjectName = null;
                FilePath = null;
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        ~CreateNewProjectViewModel()
        {
            Dispose(false);
        }
    }
}
