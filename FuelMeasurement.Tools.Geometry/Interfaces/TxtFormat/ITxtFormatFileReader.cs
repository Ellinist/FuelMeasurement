using FuelMeasurement.Model.Models.GeometryModels;

namespace FuelMeasurement.Tools.Geometry.Interfaces.TxtFormat
{
    public interface ITxtFormatFileReader
    {
        Task<MeshModel> Read(string filePath);
    }
}
