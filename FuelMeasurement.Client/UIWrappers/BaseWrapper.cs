using FuelMeasurement.Client.Models;
using FuelMeasurement.Data.Repositories.Repositories.Interfaces;
using HelixToolkit.SharpDX.Core.Model;
using System.Windows.Threading;

namespace FuelMeasurement.Client.UIWrappers
{
    public abstract class BaseWrapper : ObservableObject
    {
        protected readonly Dispatcher _dispatcher;
        protected readonly IGeometryRepository _geometryRepository;
        protected BaseWrapper(
            Dispatcher dispatcher,
            IGeometryRepository geometryRepository,
            ModelBase model
            )
        {
            _dispatcher = dispatcher;
            _geometryRepository = geometryRepository;
        }
    }
}
