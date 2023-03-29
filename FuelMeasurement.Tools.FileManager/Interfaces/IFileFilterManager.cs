using FuelMeasurement.Common.SupportedFileFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Tools.FileManager.Interfaces
{
    public interface IFileFilterManager
    {
        string GetFileFilter(Type type);
        string GetFilesFilter(List<Type> types);
    }
}
