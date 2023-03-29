using Assimp;

namespace FuelMeasurement.Tools.Geometry.Interfaces
{
    public interface ICustomWriter
    {
        Task<bool> Write(Scene scene, string filePath);
        Task<bool> Write(Stream stream, Scene scene);
    }
}
