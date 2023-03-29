using DryIoc;
using FuelMeasurement.Common.Constants;
using FuelMeasurement.Common.Events;
using MahApps.Metro.Controls;
using Prism.Events;
using Prism.Regions;
using System.Windows;

namespace FuelMeasurement.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(IContainer container)
        {
            InitializeComponent();

            var regionManager = container.Resolve<IRegionManager>();

            if (regionManager != null)
            {
                SetRegionManager(regionManager, WorkingRegion, RegionNames.WorkingRegion);
                SetRegionManager(regionManager, TreeViewRegion, RegionNames.TreeViewRegion);
            }

            var windowLoadedEvent =  container.Resolve<IEventAggregator>().GetEvent<MainWindowLoadedEvent>();
            windowLoadedEvent.Publish();
        }
        void SetRegionManager(IRegionManager regionManager, DependencyObject regionTarget, string regionName)
        {
            RegionManager.SetRegionName(regionTarget, regionName);
            RegionManager.SetRegionManager(regionTarget, regionManager);
        }

        private void MetroWindow_ContentRendered(object sender, System.EventArgs e)
        {
            Activate();
        }
    }
}
