using FuelMeasurement.Common.DialogModule.Implementations;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace FuelMeasurement.Common.DialogModule.ViewModels
{
    public class DialogViewModelBase : BindableBase, IDialogAware
    {
        #region Message

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        private string _message;

        #endregion

        #region IDialogAware

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        private string _title = String.Empty;

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            Message =
                parameters?.GetValue<string>(FormParametersNames.Message) ?? string.Empty;
        }

        public virtual bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {
            return;
        }

        public event Action<IDialogResult> RequestClose;

        #endregion

        #region RaiseRequestClose

        public bool RequestClosePassed
        { get; private set; }

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClosePassed = true;
            RequestClose?.Invoke(dialogResult);
        }

        #endregion

        #region CloseDialogCommand

        public DelegateCommand<string> CloseDialogCommand => _closeDialogCommand ??= new DelegateCommand<string>(CloseDialog);

        private DelegateCommand<string> _closeDialogCommand;

        protected virtual void CloseDialog(string parameter)
        {
            var result = ButtonResult.None;

            // ReSharper disable once ConvertIfStatementToSwitchExpression
            if (parameter?.ToLower() == "true")
                result = ButtonResult.OK;
            else
            if (parameter?.ToLower() == "false")
                result = ButtonResult.Cancel;

            RaiseRequestClose(new DialogResult(result));
        }

        #endregion
    }
}
