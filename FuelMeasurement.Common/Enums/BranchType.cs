using System.ComponentModel;

namespace FuelMeasurement.Common.Enums
{
    [Description("Тип ветки")]
    public enum BranchType
    {
        [Description("Обычная")]
        Default = 0,
        [Description("Рабочая")]
        Working = 1
    }
}
