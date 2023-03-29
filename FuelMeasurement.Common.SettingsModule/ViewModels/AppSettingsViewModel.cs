using FuelMeasurement.Common.SettingsModule.Models;
using Prism.Mvvm;
using System.ComponentModel;
using FuelMeasurement.Common.SettingsModule.Interfaces;
using Prism.Commands;

namespace FuelMeasurement.Common.SettingsModule.ViewModels
{
    public class AppSettingsViewModel : BindableBase
    {
        private readonly ISettingsTreeViewItemsHelper _settingsTreeViewItemHelper;

        private string _regionHeader = "Настройки программы";
        private BindingList<SettingsTreeViewItem> _settingsItems = new();
        private SettingsTreeViewItem _selectedProperty = null;

        public DelegateCommand<object> SelectedPropertyChangedCommand { get; private set; }

        /// <summary>
        /// Заголовок региона настроек
        /// </summary>
        public string RegionHeader
        {
            get => _regionHeader;
            set
            {
                _regionHeader = value;
                RaisePropertyChanged(nameof(RegionHeader));
            }
        }

        /// <summary>
        /// Выделенный пункт меню
        /// </summary>
        public SettingsTreeViewItem SelectedProperty
        {
            get => _selectedProperty;
            set
            {
                _selectedProperty = value;

                RaisePropertyChanged(nameof(SelectedProperty));
            }
        }

        /// <summary>
        /// Коллекция пунктов меню
        /// </summary>
        public BindingList<SettingsTreeViewItem> SettingsItems
        {
            get => _settingsItems;
            set
            {
                _settingsItems = value;
                RaisePropertyChanged(nameof(SettingsItems));
            }
        }

        public AppSettingsViewModel(ISettingsTreeViewItemsHelper settingsTreeViewItemsHelper)
        {
            _settingsTreeViewItemHelper = settingsTreeViewItemsHelper;

            SettingsItems = _settingsTreeViewItemHelper.CreateTreeViewItems();

            SelectedPropertyChangedCommand = new DelegateCommand<object>(ChangeSettingsItem);
        }

        private void ChangeSettingsItem(object o)
        {
            if(o is SettingsTreeViewItem item)
            {
                SelectedProperty = item;
            }
        }
    }
}
