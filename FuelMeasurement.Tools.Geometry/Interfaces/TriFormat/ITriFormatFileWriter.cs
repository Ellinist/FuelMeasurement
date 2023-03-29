using System.Threading.Tasks;
using Assimp;

namespace FuelMeasurement.Tools.Geometry.Interfaces.TriFormat
{
    public interface ITriFormatFileWriter
    {
        Task<bool> Write(Scene scene, string filePath);
    }
}
