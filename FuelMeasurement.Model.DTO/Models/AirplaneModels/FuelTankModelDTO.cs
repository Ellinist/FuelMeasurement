using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FuelMeasurement.Model.DTO.Models.AirplaneModels
{
    [Serializable]
    [DisplayName("Топливный бак")]
    public class FuelTankModelDTO : TankModelDTO
    {
        [DisplayName("Датчики")]
        public IEnumerable<SensorModelDTO> Sensors { get; set; }

        [DisplayName("Группа баков в порядке заправки")]
        public TankGroupModelDTO TankGroupIn { get; set; }

        [DisplayName("Группа баков в порядке выработки")]
        public TankGroupModelDTO TankGroupOut { get; set; }

        public FuelTankModelDTO()
        {

        }
    }
}
