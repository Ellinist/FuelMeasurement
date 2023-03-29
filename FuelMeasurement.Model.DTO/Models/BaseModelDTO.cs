using System;
using System.ComponentModel;

namespace FuelMeasurement.Model.DTO.Models
{
    [DisplayName("Базовая модель")]
    public abstract class BaseModelDTO
    {
        public string Id { get; set; }
    }
}