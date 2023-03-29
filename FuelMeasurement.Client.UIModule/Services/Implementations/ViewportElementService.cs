using FuelMeasurement.Client.UIModule.Models;
using FuelMeasurement.Client.UIModule.Services.Interfaces;
using HelixToolkit.Wpf.SharpDX;
using System.Collections.Generic;

namespace FuelMeasurement.Client.UIModule.Services.Implementations
{
    public class ViewportElementService : IViewportElementService
    {
        public IEnumerable<CoordinateLineMesh> CreateViewportLines(bool allLine = true)
        {
            List<CoordinateLineMesh> lines = new() 
            {
                new CoordinateLineMesh(
                    PhongMaterials.Red,
                    FuelMeasurement.Common.Enums.CoordinateLineType.X
                    ),
                new CoordinateLineMesh(
                    PhongMaterials.Green,
                    FuelMeasurement.Common.Enums.CoordinateLineType.Y
                    ),
                new CoordinateLineMesh(
                    PhongMaterials.Blue,
                    FuelMeasurement.Common.Enums.CoordinateLineType.Z
                    )
            };

            return lines;
        }

        public GridMesh CreateViewportGrid()
        {
            return new GridMesh(PhongMaterials.Gray, 15);
        }
    }
}
