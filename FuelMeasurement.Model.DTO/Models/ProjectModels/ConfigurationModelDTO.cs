using FuelMeasurement.Common.Enums;
using FuelMeasurement.Model.DTO.Models.AirplaneModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Model.DTO.Models.ProjectModels
{
    [Serializable]
    [DisplayName("Конфигурация проекта")]
    public class ConfigurationModelDTO : NamedModelDTO
    {
        [DisplayName("Баки")]
        public IEnumerable<TankModelDTO> FuelTanks { get; set; }

        [Description("Список веток в проекте")]
        public IEnumerable<BranchModelDTO> Branches { get; set; }

        [Description("Список внутренностей в проекте")]
        public IEnumerable<TankModelDTO> InsideModelFuelTanks { get; set; }

        [Description("Тип конфигурации")]
        public ConfigurationType Type { get; set; }

        [Description("Ид проекта к которому принадлежит")]
        public string ProjectId { get; set; }

        public ConfigurationModelDTO()
        {

        }
    }
}
