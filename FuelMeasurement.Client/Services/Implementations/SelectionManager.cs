using FuelMeasurement.Client.Models;
using FuelMeasurement.Client.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.Services.Implementations
{
    public class SelectionManager : ISelectionManager
    {
        private ModelBase CurrentSelectedItem { get; set; }

        public SelectionManager()
        {

        }

        public void SetSelectedItem(ModelBase selectedItem)
        {
            CurrentSelectedItem = selectedItem;
        }

        public ModelBase GetSelectedItem()
        {
            return CurrentSelectedItem;
        }
    }
}
