using System.ComponentModel;

namespace FuelMeasurement.Common.Enums
{
    /// <summary>
    /// Перечислитель отображаемых кривых - сущностей
    /// </summary>
    public enum CurvesTypeEnum
    {
        [Description("Расчет МПИ для обычного режима")]
        UsualErrors = 0,
        [Description("Расчет МПИ для режима заправки")]
        FuelInErrors = 1,
        [Description("Расчет МПИ для режима выработки")]
        FuelOutErrors = 2,
        [Description("Расчет МПИ по общему зеркалу топлива (общий режим и режим заправки)")]
        CommonMirrorErrors = 3,
        [Description("Расчет МПИ по общему зеркалу топлива (режим выработки)")]
        FuelOutMirrorErrors = 4,
        [Description("Расчет МПИ по опорным углам (общий режим)")]
        UsualReferencedErrors = 5,
        [Description("Расчет МПИ по опорным углам (режим заправки)")]
        FuelInReferencedErrors = 6,
        [Description("Расчет МПИ по опорным углам (режим выработки)")]
        FuelOutReferencedErrors = 7,
        [Description("Расчет погонной емкости для обычного режима")]
        UsualCapacity = 8,
        [Description("Расчет погонной емкости для режима заправки")]
        FuelInCapacity = 9,
        [Description("Расчет погонной емкости для режима выработки")]
        FuelOutCapacity = 10,
        [Description("ХЗ")]
        Calibrations = 11
    }
}
