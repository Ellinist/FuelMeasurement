using ControlzEx.Theming;
using Prism.Commands;
using System.Windows;
using System.Windows.Media;


namespace FuelMeasurement.Client.Tools
{

    public class AccentColorMenuData
    {
        public string Name { get; set; }

        public Brush BorderColorBrush { get; set; }

        public Brush ColorBrush { get; set; }

        public AccentColorMenuData()
        {
            this.ChangeThemeCommand = new DelegateCommand<string>(DoChangeTheme);
        }

        public DelegateCommand<string> ChangeThemeCommand { get; private set; }
        

        protected void DoChangeTheme(string name)
        {
            if (name is not null)
            {
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, name);
            }
        }
    }

}
