using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FuelMeasurement.Model.DTO.Models.ProjectModels
{
    [Serializable]
    [DisplayName("Проект")]
    public class ProjectModelDTO : NamedModelDTO
    {
        [DisplayName("Конфигурации проекта")]
        public IEnumerable<ConfigurationModelDTO> Configurations { get; set; }

        [Description("Дата создания проекта")]
        public DateTime Creation { get; set; }

        [Description("Автор проекта")]
        public string ProjectAuthor { get; set; }

        [Description("Путь к архиву сохранённому на ЖД")]
        public string FilePath { get; set; }

        public ProjectModelDTO()
        {

        }
    }
}
