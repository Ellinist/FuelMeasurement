using FuelMeasurement.Model.Models.GeometryModels;

namespace FuelMeasurement.Tools.Geometry.Interfaces
{
    public interface ICustomReader
    {
        Task<MeshModel> Read(string filePath);
        Task<MeshModel> Read(Stream stream);
    }
}
