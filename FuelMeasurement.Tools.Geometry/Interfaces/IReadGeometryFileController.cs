using FuelMeasurement.Model.Models.GeometryModels;

namespace FuelMeasurement.Tools.Geometry.Interfaces
{
    public interface IReadGeometryFileController
    {
        Task<FuelTankGeometryModel?> ReadGeometry(string file);
    }
}
