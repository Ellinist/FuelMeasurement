using FuelMeasurement.Common.DialogModule.Interfaces;
using FuelMeasurement.Client.WaitWindowModule.Interfaces;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Prism.Services.Dialogs;

namespace FuelMeasurement.Client.ViewModels
{
    //TODO Удалить потом
    
    /// <summary>
    /// Здесь тесты внешнего вида и всякой фигни
    /// </summary>
    public class TestWindowViewModel: BindableBase
    {
        public DelegateCommand N_DExampleCommand { get; private set; }
        public DelegateCommand I_DExampleCommand { get; private set; }
        public DelegateCommand C_DExampleCommand { get; private set; }

        public DelegateCommand ShowWaitWindowCommand { get; private set; }

        public DelegateCommand CommandB1 { get; private set; }
        public DelegateCommand CommandB2 { get; private set; }
        public DelegateCommand CommandB3 { get; private set; }
        public DelegateCommand CommandB4 { get; private set; }

        private readonly IDialogServices _dialogService;

        private Dispatcher _dispatcher;

        private readonly ILogger _logger;

        private readonly IWaitWindowService _waitWindowService;

        private int count = 0;
        public TestWindowViewModel(
            IDialogServices dialogService,
             ILogger logger,
             IWaitWindowService waitWindowService
            )
        {
            
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _waitWindowService = waitWindowService ?? throw new ArgumentNullException(nameof(waitWindowService));

            _dispatcher = Application.Current.Dispatcher;

            InitializeCommand();
        }

        private void InitializeCommand()
        {
            #region Its for tests
            //TODO its for tests MAY delete later
            N_DExampleCommand = new DelegateCommand(() =>
            {
                ActionInViewModel(() =>
                {
                    StringBuilder builder = new();
                    builder.Append("ИРДТ - Интерактивная расстановка датчиков топливомера");
                    builder.AppendLine();
                    builder.Append("(с) ПАО ТЕХПРИБОР 2021");
                    _dialogService.ShowNotificationDialog("О программе", builder.ToString());
                }
                );
            });

            I_DExampleCommand = new DelegateCommand(() =>
            {
                ActionInViewModel(() =>
                {
                    _dialogService.ShowInputTextDialog("Title Title", "input something:", "input text", null, true, "extended input something:", "extended input text");
                }
                );
                
            });

            C_DExampleCommand = new DelegateCommand(() =>
            {
                _dialogService.ShowConfirmationDialog("Смысл жизни", "Есть ли он", null, "есть", "неа");

                //ActionInViewModel(() =>
                //{
                //    _dialogService.ShowConfirmationDialog("Смысл жизни", "Есть ли он", null, "есть", "неа");
                //}
                //);
            });

            ShowWaitWindowCommand = new DelegateCommand(() =>
            {
                
                
                _waitWindowService.PrimaryProgressBar.Caption = "Cool1";
                //_waitWindowService.SecondaryProgressBar.IsIndeterminate = true;
                _waitWindowService.Show(true,true);
            }
            );

            CommandB1 = new DelegateCommand(() =>
            {
                _waitWindowService.PrimaryProgressBar.Value += 2;
            });

            CommandB2 = new DelegateCommand(() =>
            {
                _waitWindowService.Close();
            });

            CommandB3 = new DelegateCommand(() =>
            {
                _waitWindowService.AddMessage(count+"_sdfgggggggtruuttttttttttttttttttttttttttttttttttttttttttttttttttttttttttdyjDFHTRYJTRYJSFGHCDHTYH");
                count++;
            });

            CommandB4 = new DelegateCommand(() =>
            {
            Task.Factory.StartNew(async () =>
                await LongOperation()
                );
                Task.Factory.StartNew(async () =>
                await LongOperation2()
                );
            });

            #endregion Its for tests
        }

        private async Task LongOperation()
        {
            for (int i = 0; i <= 10; i++)
            {
                _waitWindowService.PrimaryProgressBar.Value = i * 10;
                _waitWindowService.PrimaryProgressBar.Info = (i * 10).ToString();
                _waitWindowService.AddMessage(i+"_sdfgggggggtruut");
                _waitWindowService.SetTitle(i.ToString());
               await Task.Delay(1000);
            }

        }

        private async Task LongOperation2()
        {

            for (int i = 0; i <= 10; i++)
            {
                _waitWindowService.SecondaryProgressBar.Value = i * 10;
                _waitWindowService.SecondaryProgressBar.Info = (i * 10).ToString();
                await Task.Delay(2000);
            }
        }



        public void ActionInViewModel(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _dispatcher.Invoke(priority,
                new Action(() =>
                {
                    action.Invoke();
                    //try
                    //{
                    //    action.Invoke();
                    //}
                    //catch (Exception ex)
                    //{
                    //    _logger.Error(ex);
                    //}
                }));
        }

    }
}
