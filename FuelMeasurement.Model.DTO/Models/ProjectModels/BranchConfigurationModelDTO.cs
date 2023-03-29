using FuelMeasurement.Common.Enums;
using System;
using System.ComponentModel;

namespace FuelMeasurement.Model.DTO.Models.ProjectModels
{
    [Serializable]
    [DisplayName("Настройки ветки")]
    public class BranchConfigurationModelDTO
    {
        [DisplayName("Минимальный угол тангажа")]
        public double MinPitch { get; set; }

        [DisplayName("Максимальный угол тангажа")]
        public double MaxPitch { get; set; }

        [DisplayName("Минимальный угол крена")]
        public double MinRoll { get; set; }

        [DisplayName("Максимальный угол крена")]
        public double MaxRoll { get; set; }

        [DisplayName("Шаг приращения угла тангажа")]
        public double PitchStep { get; set; }

        [DisplayName("Шаг приращения угла крена")]
        public double RollStep { get; set; }

        [DisplayName("Количество расчетных точек (срезов) бака")]
        public int NodesQuantity { get; set; }

        [DisplayName("Опорный угол тангажа")]
        public double ReferencedPitch { get; set; }

        [DisplayName("Опорный угол крена")]
        public double ReferencedRoll { get; set; }

        [DisplayName("Коэффициент пересчета в литры")]
        public float Coefficient { get; set; }

        [DisplayName("Коэффициент длины датчика")]
        public int LengthCoef { get; set; }

        [DisplayName("Минимальный нижний отступ датчика")]
        public double DefaultMinIndent { get; set; }

        [DisplayName("Верхний отступ датчика")]
        public double DefaultUpIndent { get; set; }

        [DisplayName("Нижний отступ датчика")]
        public double DefaultDownIndent { get; set; }

        [DisplayName("Видимый диаметр датчиков")]
        public double VisibleSensorDiametr { get; set; }

        [DisplayName("Режим установки размера датчиков")]
        public SensorVisualDiametrTypeEnum SensorVisualDiametrType { get; set; }

        public BranchConfigurationModelDTO()
        {

        }
    }
}
