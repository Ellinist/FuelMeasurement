using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.WaitWindowModule.Models
{
    public class ProgressBarSettings: BindableBase
    {
        private int _min = 0;
        private int _max = 100;
        private double _value = 0;

        public int Min { get => _min; set => SetProperty(ref _min, value); }
        public int Max { get => _max; set => SetProperty(ref _max, value); }
        public double Value { get => _value; set => SetProperty(ref _value, value); }
        public bool IsIndeterminate { get => _isIndeterminate; set => SetProperty(ref _isIndeterminate, value); }
        public bool Show { get => _show; set => SetProperty(ref _show, value); }
        public bool ShowCaption { get => _showCaption; set => SetProperty(ref _showCaption, value); }
        public bool ShowInfo { get => _showInfo; set => SetProperty(ref _showInfo, value); }
        public string Caption { get => _caption; set => SetProperty(ref _caption, value); }
        public string Info { get => _info; set => SetProperty(ref _info, value); }

        private bool _isIndeterminate = false;

        private bool _show = true;

        private bool _showCaption = true;
        private string _caption = string.Empty;

        private bool _showInfo = true;
        private string _info = string.Empty;
    }
}
