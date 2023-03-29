using System.ComponentModel;

namespace FuelMeasurement.Model.DTO.Models
{
    [DisplayName("Модель у которой есть название/имя")]
    public abstract class NamedModelDTO : BaseModelDTO
    {
        public string Name { get; set; }
    }
}
