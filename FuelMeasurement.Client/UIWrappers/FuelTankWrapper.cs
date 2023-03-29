using FuelMeasurement.Client.Models;
using FuelMeasurement.Data.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FuelMeasurement.Client.UIWrappers
{
    public class FuelTankWrapper : BaseWrapper
    {
        public FuelTankWrapper(
            Dispatcher dispatcher, 
            IGeometryRepository geometryRepository,
            FuelTankModel model
            ) 
            : base(dispatcher, geometryRepository, model)
        {

        }
    }
}
