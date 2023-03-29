using Assimp;

namespace FuelMeasurement.Tools.Geometry.Interfaces.TxtFormat
{
    public interface ITxtFormatFileWriter
    {
        Task<bool> Write(Scene scene, string filePath);
    }
}
