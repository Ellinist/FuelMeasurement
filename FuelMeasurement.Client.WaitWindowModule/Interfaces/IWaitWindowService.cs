using FuelMeasurement.Client.WaitWindowModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FuelMeasurement.Client.WaitWindowModule.Interfaces
{
    public interface IWaitWindowService
    {
        void Show(bool showSecondary = false, bool showCancelButton = false);
        void AddMessage(string message, bool addTime = true);
        void ClearMessage();
        void SetTitle (string title);
        void HideSecondaryProgressBar();
        void ShowSecondaryProgressBar();
        ProgressBarSettings PrimaryProgressBar { get; }
        ProgressBarSettings SecondaryProgressBar { get; }
        void Close();

    }
}
