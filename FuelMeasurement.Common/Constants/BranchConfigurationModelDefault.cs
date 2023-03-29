using FuelMeasurement.Common.Enums;

namespace FuelMeasurement.Common.Constants
{
    public class BranchConfigurationModelDefault
    {
        public const double MinPitch = -5;

        public const double MaxPitch =  5;

        public const double MinRoll = -5;

        public const double MaxRoll =  5;

        public const double PitchStep = 1;

        public const double RollStep = 1;

        public const int NodesQuantity = 20;

        public const double ReferencedPitch = 0;

        public const double ReferencedRoll = 0;

        public const float Coefficient = 1_000_000F;

        public const int LengthCoef = 5;

        public const double DefaultMinIndent = 10;

        public const double DefaultUpIndent = 15;

        public const double DefaultDownIndent = 15;

        public const double VisibleSensorDiametr = 80;

        public const SensorVisualDiametrTypeEnum SensorVisualDiametrType = SensorVisualDiametrTypeEnum.Auto;
    }
}
