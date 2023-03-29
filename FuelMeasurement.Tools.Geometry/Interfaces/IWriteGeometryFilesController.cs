using System.Threading.Tasks;

namespace FuelMeasurement.Tools.Geometry.Interfaces
{
    public interface IWriteGeometryFilesController
    {
        Task WriteGeometryInFile(object writeObject, string filePath, string fileExtension);
    }
}
