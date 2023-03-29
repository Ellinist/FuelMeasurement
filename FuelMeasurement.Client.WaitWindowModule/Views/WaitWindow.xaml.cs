using FuelMeasurement.Client.WaitWindowModule.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FuelMeasurement.Client.WaitWindowModule.Views
{
    /// <summary>
    /// Interaction logic for WaitWindow.xaml
    /// </summary>
    public partial class WaitWindow : MetroWindow
    {
        public WaitWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Hide();
            //e.Cancel = true;
        }
    }
}
