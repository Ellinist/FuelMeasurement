using FuelMeasurement.Common.Enums;
using FuelMeasurement.Model.DTO.Models.AirplaneModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace FuelMeasurement.Model.DTO.Models.ProjectModels
{
    [Serializable]
    [DisplayName("Ветка проекта")]
    public class BranchModelDTO : NamedModelDTO
    {
        [Description("Дата создания ветки")]
        public DateTime Creation { get; set; }

        [Description("Дата последнего обновления ветки")]
        public DateTime? Updated { get; set; }

        [DisplayName("Баки")]
        public IEnumerable<FuelTankModelDTO> FuelTanks { get; set; }

        [DisplayName("Группы заправки баков")]
        public IEnumerable<TankGroupModelDTO> TankInGroups { get; set; }

        [DisplayName("Группы выработки баков")]
        public IEnumerable<TankGroupModelDTO> TankOutGroups { get; set; }

        [Description("Настройки/Конфигурация ветки")]
        public BranchConfigurationModelDTO Configuration { get; set; }

        [Description("Тип ветки")]
        public BranchType Type { get; set; }

        [Description("Ид проекта к которому принадлежит")]
        public string ProjectId { get; set; }

        public BranchModelDTO()
        {

        }
    }
}
