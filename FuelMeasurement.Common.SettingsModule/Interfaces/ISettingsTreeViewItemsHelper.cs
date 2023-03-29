using FuelMeasurement.Common.SettingsModule.Models;
using System.ComponentModel;

namespace FuelMeasurement.Common.SettingsModule.Interfaces
{
    public interface ISettingsTreeViewItemsHelper
    {
        BindingList<SettingsTreeViewItem> CreateTreeViewItems();
    }
}
