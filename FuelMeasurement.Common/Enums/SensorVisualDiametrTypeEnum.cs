using System.ComponentModel;

namespace FuelMeasurement.Common.Enums
{
    [Description("Режим установки размера датчиков")]
    public enum SensorVisualDiametrTypeEnum
    {
        [Description("Автоматический")]
        Auto = 0,
        [Description("Ручной")]
        Hand = 1
    }
}
