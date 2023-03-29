using FuelMeasurement.Client.UIWrappers;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FuelMeasurement.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для SelectedItemWrapper.xaml
    /// </summary>
    public partial class SelectedItemWrapper : UserControl
    {
        public BaseWrapper ItemInSceneWrapper
        {
            get => (BaseWrapper)GetValue(ItemInSceneWrapperProperty);
            set => SetValue(ItemInSceneWrapperProperty, value);
        }

        public static readonly DependencyProperty ItemInSceneWrapperProperty =
            DependencyProperty.Register(
                nameof(ItemInSceneWrapper),
                typeof(BaseWrapper),
                typeof(SelectedItemWrapper)
            );

        public SelectedItemWrapper()
        {
            InitializeComponent();
        }
    }
}
