using System.IO;
using System.Threading.Tasks;
using Assimp;
using FuelMeasurement.Tools.Geometry.Interfaces;

namespace FuelMeasurement.Tools.Geometry.Implementations
{
    public abstract class CustomWriter : ControllerBase, ICustomWriter
    {
        public virtual async Task<bool> Write(Scene scene, string filePath)
        {
            using var stream = File.Create(filePath);

            var result = await Write(stream, scene);

            stream.Dispose();

            return result;
        }

        public abstract Task<bool> Write(Stream stream, Scene scene);
    }
}
