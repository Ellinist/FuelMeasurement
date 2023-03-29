using FuelMeasurement.Common.DialogModule.Interfaces;
using FuelMeasurement.Common.DialogModule.Views;
using Microsoft.Win32;
using Prism.Services.Dialogs;
using System;
using System.IO;

namespace FuelMeasurement.Common.DialogModule.Implementations
{
    public class DialogServices : IDialogServices
    {
        private readonly IDialogService _prismDialogService;
        public DialogServices(IDialogService prismDialogService)
        {
            _prismDialogService = prismDialogService;
        }
        public void OpenFileDialog(string filter, bool multiSelect, Action<string[]> callback)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = filter,
                Multiselect = multiSelect
            };
            if (openFileDialog.ShowDialog() == true)
            {
                callback(openFileDialog.FileNames);
            }
        }

        public void SaveFileDialog(string filter, string filename, Action<string> callback)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = filter,
                FileName = Path.GetFileName(filename),
                InitialDirectory = Path.GetDirectoryName(filename)
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                callback(saveFileDialog.FileName);
            }
        }

        public void ShowConfirmationDialog(string title, string message, Action<IDialogResult> callBack, string okButtonText = null, string cancelButtomText = null)
        {
            var p = new DialogParameters
            {
                { DialogParametersNames.Title, title },
                { DialogParametersNames.Message, message },
                { DialogParametersNames.OkButtonText, okButtonText },
                { DialogParametersNames.CancelButtonText, cancelButtomText },
            };

            _prismDialogService.ShowDialog(nameof(ConfirmationDialogView), p, callBack);
        }

        public void ShowInputTextDialog(string title, string inputLabel, string inputText, Action<IDialogResult> callBack, bool isExtendedInputShown = false, string extendedInputLabel = null, string extendedInputText = null)
        {
            var p = new DialogParameters
            {
                { DialogParametersNames.Title, title },
                { DialogParametersNames.InputLabel, inputLabel },
                { DialogParametersNames.InputText, inputText },
                { DialogParametersNames.IsExtendedInputShown, isExtendedInputShown },
                { DialogParametersNames.ExtendedInputLabel, extendedInputLabel },
                { DialogParametersNames.ExtendedInputText, extendedInputText },
            };

            _prismDialogService.ShowDialog(nameof(InputTextDialogView), p, callBack);
        }

        public void ShowNotificationDialog(string title, string message, Action<IDialogResult> callBack = null)
        {
            var p = new DialogParameters
            {
                { DialogParametersNames.Title, title },
                { DialogParametersNames.Message, message }
            };

            _prismDialogService.ShowDialog(nameof(NotificationDialogView), p, callBack);
        }
    }
}
