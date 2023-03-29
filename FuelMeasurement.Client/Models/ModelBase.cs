using FuelMeasurement.Common.Enums;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.Models
{
    public class ModelBase : BindableBase
    {
        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set 
            {
                if(SetProperty(ref _isSelected, value))
                {
                    if(IsSelected)
                    {
                        IsExpanded = true;
                    }
                    else
                    {
                        IsExpanded = false;
                    }
                }
            } 
        }

        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public virtual DTOObjectType Type { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _id;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
    }
}
