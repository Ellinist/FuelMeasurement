using FuelMeasurement.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.Models
{
    public class BranchConfigurationModel : ModelBase
    {
        public double MinPitch { get; set; }
        public double MaxPitch { get; set; }
        public double MinRoll { get; set; }
        public double MaxRoll { get; set; }
        public double PitchStep { get; set; }
        public double RollStep { get; set; }
        public int NodesQuantity { get; set; }
        public double ReferencedPitch { get; set; }
        public double ReferencedRoll { get; set; }
        public float Coefficient { get; set; }
        public int LengthCoef { get; set; }
        public double DefaultMinIndent { get; set; }
        public double DefaultUpIndent { get; set; }
        public double DefaultDownIndent { get; set; }
        public double VisibleSensorDiametr { get; set; }
        public SensorVisualDiametrTypeEnum SensorVisualDiametrType { get; set; }
        public override DTOObjectType Type => DTOObjectType.BranchConfiguration;
        public BranchConfigurationModel()
        {

        }
    }
}
