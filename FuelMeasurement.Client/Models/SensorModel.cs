using FuelMeasurement.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.Models
{
    public class SensorModel : ModelBase
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
        public override DTOObjectType Type => DTOObjectType.Sensor;
        public SensorModel()
        {

        }
    }
}
