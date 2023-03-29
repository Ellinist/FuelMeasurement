using FuelMeasurement.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.Services.Interfaces
{
    public interface ISelectionManager
    {
        void SetSelectedItem(ModelBase selectedItem);
        ModelBase GetSelectedItem();
    }
}
