using FuelMeasurement.Tools.FileManager.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Tools.FileManager.Implementations
{
    public class ProjectPathManager : IProjectPathManager
    {
        public ProjectPathManager()
        {

        }

        public string GetTempPath()
        {
            return ConfigurationManager.AppSettings["TEMP_PATH"];
        }
    }
}
