using Prism.Commands;
using Prism.Services.Dialogs;
using FuelMeasurement.Common.DialogModule.Implementations;

namespace FuelMeasurement.Common.DialogModule.ViewModels
{
    public class InputTextDialogViewModel : DialogViewModelBase
    {
        private string _inputLabel;

        public string InputLabel
        {
            get => _inputLabel;
            set => SetProperty(ref _inputLabel, value);
        }

        private string _extendedInputLabel;

        public string ExtendedInputLabel
        {
            get => _extendedInputLabel;
            set => SetProperty(ref _extendedInputLabel, value);
        }

        private bool _isExtendedInputShown;

        public bool IsExtendedInputShown
        {
            get => _isExtendedInputShown;
            set => SetProperty(ref _isExtendedInputShown, value);
        }

        private string _inputText;

        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        private string _extendedInputText;

        public string ExtendedInputText
        {
            get => _extendedInputText;
            set => SetProperty(ref _extendedInputText, value);
        }

        public DelegateCommand AcceptCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public InputTextDialogViewModel()
        {
            AcceptCommand = new DelegateCommand(AcceptCommandExecute);
            CancelCommand = new DelegateCommand(CancelCommandExecute);
        }

        public void AcceptCommandExecute()
        {
            RaiseRequestClose(new DialogResult(ButtonResult.OK, new DialogParameters()
            {
                { DialogParametersNames.InputText, _inputText },
                { DialogParametersNames.ExtendedInputText, _extendedInputText }
            }));
            ResetForm();
        }

        public void CancelCommandExecute()
        {
            RaiseRequestClose(new DialogResult(ButtonResult.Cancel));
            ResetForm();
        }

        private void ResetForm()
        {
            InputText = string.Empty;
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>(DialogParametersNames.Title);
            InputLabel = parameters.GetValue<string>(DialogParametersNames.InputLabel);
            ExtendedInputLabel = parameters.GetValue<string>(DialogParametersNames.ExtendedInputLabel);
            InputText = parameters.GetValue<string>(DialogParametersNames.InputText);
            ExtendedInputText = parameters.GetValue<string>(DialogParametersNames.ExtendedInputText);
            IsExtendedInputShown = parameters.GetValue<bool>(DialogParametersNames.IsExtendedInputShown);
        }
    }
}
