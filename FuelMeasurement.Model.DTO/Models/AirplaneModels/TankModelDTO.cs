using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Model.DTO.Models.AirplaneModels
{
    [Serializable]
    public class TankModelDTO : NamedModelDTO
    {
        [DisplayName("Путь к файлу геометрии")]
        public string GeometryFilePath { get; set; }

        [Description("Ид проекта к которому принадлежит")]
        public string ProjectId { get; set; }

        public TankModelDTO()
        {

        }
    }
}
