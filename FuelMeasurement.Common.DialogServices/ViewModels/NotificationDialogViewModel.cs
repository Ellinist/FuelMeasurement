using FuelMeasurement.Common.DialogModule.Implementations;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace FuelMeasurement.Common.DialogModule.ViewModels
{
    public class NotificationDialogViewModel : DialogViewModelBase
    {
        private bool _isSuccessIconVisible;

        public bool IsSuccessIconVisible
        {
            get => _isSuccessIconVisible;
            set => SetProperty(ref _isSuccessIconVisible, value);
        }

        private bool _isErrorIconVisible;

        public bool IsErrorIconVisible
        {
            get => _isErrorIconVisible;
            set => SetProperty(ref _isErrorIconVisible, value);
        }

        public DelegateCommand AcceptCommand { get; private set; }

        public NotificationDialogViewModel()
        {
            AcceptCommand = new DelegateCommand(AcceptCommandExecute);
        }

        private void AcceptCommandExecute()
        {
            RaiseRequestClose(new DialogResult(ButtonResult.OK));
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>(DialogParametersNames.Title);
            Message = parameters.GetValue<string>(DialogParametersNames.Message);
        }
    }
}
