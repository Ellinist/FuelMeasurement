using FuelMeasurement.Client.UIModule.Models;

namespace FuelMeasurement.Client.UIModule.Services.Interfaces
{
    public interface IViewportElementManipulationService
    {
        void ChangeViewportGridLevel(GridMesh grid, double level);
    }
}
