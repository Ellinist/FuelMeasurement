using System;

namespace FuelMeasurement.Common.DialogModule.Interfaces
{
    public interface IDialogServices
    {
        void OpenFileDialog(
            string filter, 
            bool multiSelect, 
            Action<string[]> callback
            );

        void SaveFileDialog(
            string filter, 
            string filename, 
            Action<string> callback
            );

        void ShowNotificationDialog(
            string title, 
            string message,          
            Action<Prism.Services.Dialogs.IDialogResult> callBack = null
            );

        void ShowConfirmationDialog(
            string title, 
            string message,                
            Action<Prism.Services.Dialogs.IDialogResult> callBack,
            string okButtonText = null,
            string cancelButtomText = null
            );

        void ShowInputTextDialog(
            string title, 
            string inputLabel, 
            string inputText,                 
            Action<Prism.Services.Dialogs.IDialogResult> callBack,
            bool isExtendedInputShown = false, 
            string extendedInputLabel = null,
            string extendedInputText = null
            );
    }
}
