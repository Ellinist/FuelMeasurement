using FuelMeasurement.Client.UIModule.Models;
using System.Collections.Generic;

namespace FuelMeasurement.Client.UIModule.Services.Interfaces
{
    public interface IViewportElementService
    {
        IEnumerable<CoordinateLineMesh> CreateViewportLines(bool allLine = true);

        GridMesh CreateViewportGrid();
    }
}
