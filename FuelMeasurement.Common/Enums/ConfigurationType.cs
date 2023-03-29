using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Common.Enums
{
    [Description("Тип конфигурации")]
    public enum ConfigurationType
    {
        [Description("Обычная")]
        Default = 0,
        [Description("Рабочая")]
        Working = 1
    }
}
