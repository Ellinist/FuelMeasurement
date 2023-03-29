using FuelMeasurement.Common.Enums;
using System.Windows.Controls;

namespace FuelMeasurement.Common.SettingsModule.Models
{
    public class SettingsTreeViewItem : TreeViewItem
    {
        public SettingsTreeViewItemPropertyEnum SettingsTreeViewItemPropertyEnum { get; set; }

        public SettingsTreeViewItem(SettingsTreeViewItemPropertyEnum itemPropertyEnum, string header)
        {
            SettingsTreeViewItemPropertyEnum = itemPropertyEnum;
            Header = header;
            FontSize = 15;
        }
    }
}
