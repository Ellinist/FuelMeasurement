using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Common.Enums
{
    /// <summary>
    /// Перечислитель режимов вычисления
    /// </summary>
    public enum ComputeTypeEnum
    {
        ErrorsUsual = 0, // МПИ для обычного режима
        ErrorsFuelIn = 1, // МПИ для режима заправки
        ErrorsFuelOut = 2, // МПИ для режима выработки
        MirrorErrorsCommonIn = 3, // МПИ для общего зеркала топлива в обычном режиме и режиме заправки
        MirrorErrorsOut = 4, // МПИ для общего зеркала топлива в режиме выработки
        ReferencedErrorsUsual = 5, // МПИ по опорным углам для обычного режима
        ReferencedErrorsIn = 6, // МПИ по опорным углам в режиме заправки
        ReferencedErrorsOut = 7 // МПИ по опорным углам в режиме выработки
    }
}
