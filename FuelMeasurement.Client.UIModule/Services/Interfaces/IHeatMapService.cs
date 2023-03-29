using FuelMeasurement.Client.UIModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.UIModule.Services.Interfaces
{
    public interface IHeatMapService
    {
        void CreateHeatMap(FuelTankMesh tankMesh);
    }
}
