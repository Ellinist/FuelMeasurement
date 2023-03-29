using System.Threading.Tasks;
using FuelMeasurement.Model.Models.GeometryModels;

namespace FuelMeasurement.Tools.Geometry.Interfaces.TriFormat
{
    public interface ITriFormatFileReader
    {
        Task<MeshModel> Read(string filePath);
    }
}
