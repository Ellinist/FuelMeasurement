using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FuelMeasurement.Client.ViewModels
{
    public interface IActionInViewModel
    {
        void ActionInViewModel( Action action, DispatcherPriority priority = DispatcherPriority.Normal);

        Task ActionInViewModelAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal);
    }
}
