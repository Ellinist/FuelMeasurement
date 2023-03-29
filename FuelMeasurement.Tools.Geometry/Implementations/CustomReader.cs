using System.IO;
using System.Threading.Tasks;
using FuelMeasurement.Model.Models.GeometryModels;
using FuelMeasurement.Tools.Geometry.Interfaces;

namespace FuelMeasurement.Tools.Geometry.Implementations
{
    public abstract class CustomReader : ControllerBase, ICustomReader
    {
        public virtual async Task<MeshModel> Read(string filePath)
        {
            using var stream = File.OpenRead(filePath);

            var meshModel = await Read(stream);

            stream.Dispose();

            return meshModel;
        }

        public abstract Task<MeshModel> Read(Stream stream);
    }
}
