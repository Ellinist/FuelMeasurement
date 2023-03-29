using FuelMeasurement.Common.Enums;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.Models
{
    public class ProjectModel : ModelBase
    {
        public IEnumerable<ConfigurationModel> Configurations { get; set; }
        public DateTime Creation { get; set; }
        public string ProjectAuthor { get; set; }
        public override DTOObjectType Type => DTOObjectType.Project;

        public ProjectModel()
        {

        }
    }
}
