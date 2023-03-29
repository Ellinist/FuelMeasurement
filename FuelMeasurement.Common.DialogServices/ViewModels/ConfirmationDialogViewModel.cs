using FuelMeasurement.Common.DialogModule.Implementations;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace FuelMeasurement.Common.DialogModule.ViewModels
{
    public class ConfirmationDialogViewModel : DialogViewModelBase
    {
        public DelegateCommand AcceptCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        private string _okButtonText;

        public string OkButtonText
        {
            get => _okButtonText;
            set => SetProperty(ref _okButtonText, value);
        }

        private string _cancelButtonText;

        public string CancelButtonText
        {
            get => _cancelButtonText;
            set => SetProperty(ref _cancelButtonText, value);
        }

        public ConfirmationDialogViewModel()
        {
            AcceptCommand = new DelegateCommand(AcceptCommandExecute);
            CancelCommand = new DelegateCommand(CancelCommandExecute);
        }

        public void AcceptCommandExecute()
        {
            RaiseRequestClose(new DialogResult(ButtonResult.OK));
        }

        public void CancelCommandExecute()
        {
            RaiseRequestClose(new DialogResult(ButtonResult.Cancel));
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>(DialogParametersNames.Title);
            Message = parameters.GetValue<string>(DialogParametersNames.Message);
            OkButtonText = parameters.GetValue<string>(DialogParametersNames.OkButtonText);
            CancelButtonText = parameters.GetValue<string>(DialogParametersNames.CancelButtonText);
        }
    }
}
