using System;
using System.ComponentModel;
using System.Numerics;

namespace FuelMeasurement.Model.DTO.Models.AirplaneModels
{
    [Serializable]
    [DisplayName("Датчик")]
    public class SensorModelDTO : NamedModelDTO
    {
        [Description("Верхняя точка")]
        public Vector3 UpPoint { get; set; }

        [Description("Нижняя точка")]
        public Vector3 DownPoint { get; set; }

        [Description("Верхняя точка крепления к баку")]
        public Vector3 InTankUpPoint { get; set; }

        [Description("Нижняя точка крепления к баку")]
        public Vector3 InTankDownPoint { get; set; }

        [Description("Отступ верхней точки")]
        public double UpPointIndent { get; set; }
        [Description("Отступ нижней точки")]
        public double DownPointIndent { get; set; }

        [Description("Ид проекта к которому принадлежит")]
        public string ProjectId { get; set; }

        [Description("Ид проекта к которому принадлежит")]
        public string TankId { get; set; }

        [Description("Длина датчика")]
        public double Length { get; set; }

        [Description("Емкость датчика (погонная)")]
        public double LinearCapacity { get; set; }

        [Description("Активный ли датчик")]
        public bool IsActiveSensor { get; set; }

        public SensorModelDTO()
        {
            IsActiveSensor = true;
        }
    }
}
