using FuelMeasurement.Common.SupportedFileFormats;
using FuelMeasurement.Tools.FileManager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Tools.FileManager.Implementations
{
    public class FileFilterManager : IFileFilterManager
    {
        public FileFilterManager()
        {

        }
        public string GetFileFilter(Type type)
        {
            string result = string.Empty;
            var description = type.GetField("Description");
            var extension = type.GetField("Extension");

            if (description != null && extension != null)
            {
                result = $"{description.GetValue(description)} | *{extension.GetValue(extension)};";
            }

            return result;
        }

        public string GetFilesFilter(List<Type> types)
        {
            StringBuilder sb = new ();
            sb.Append("Geometry files ");

            foreach (var type in types)
            {
                var extensions = type.GetField("Extension");

                if (extensions != null)
                {
                    sb.Append($" | *{extensions.GetValue(extensions)}; ");
                }
            }

            sb.Append($"| All Files |*.*;");
            return sb.ToString();
        }
    }
}
