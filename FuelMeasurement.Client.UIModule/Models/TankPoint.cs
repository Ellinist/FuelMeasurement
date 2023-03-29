using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.UIModule.Models
{
    public class TankPoint : BindableBase
    {
        #region Приватные поля
        private string _tankName;
        private double _upY;
        private double _downY;
        #endregion

        public string TankName
        {
            get => _tankName;
            set
            {
                if (_tankName == value) return;
                _tankName = value;
                RaisePropertyChanged(nameof(TankName));
            }
        }
        public double UpY
        {
            get => _upY;
            set
            {
                if (_upY == value) return;
                _upY = value;
                RaisePropertyChanged(nameof(UpY));
            }
        }

        public double DownY
        {
            get => _downY;
            set
            {
                if (_downY == value) return;
                _downY = value;
                RaisePropertyChanged(nameof(DownY));
            }
        }
    }
}
