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
    public class SensorWrapper : BaseWrapper
    {
        public SensorWrapper(
            Dispatcher dispatcher, 
            IGeometryRepository geometryRepository,
            SensorModel model
            )
            : base(dispatcher, geometryRepository, model)
        {

        }
    }
}
