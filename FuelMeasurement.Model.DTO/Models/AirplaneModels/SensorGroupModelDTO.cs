using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FuelMeasurement.Model.DTO.Models.AirplaneModels
{
    [Serializable]
    [DisplayName("Группа датчиков")]
    public class SensorGroupModelDTO : NamedModelDTO
    {
        [DisplayName("Id баков в группе")]
        IEnumerable<string> SensorsInGroup { get; set; }

        public SensorGroupModelDTO()
        {

        }
    }
}
