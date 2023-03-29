using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FuelMeasurement.Model.DTO.Models.AirplaneModels
{
    [Serializable]
    [DisplayName("Группа баков в порядке заправки|выработки")]
    public class TankGroupModelDTO : NamedModelDTO
    {
        [DisplayName("Id баков в группе")]
        public IEnumerable<string> FuelTanksInGroup { get; set; }

        [Description("Ид проекта к которому принадлежит")]
        public string ProjectId { get; set; }

        public TankGroupModelDTO()
        {

        }
    }
}