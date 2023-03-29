using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Common.Enums
{
    public enum SelectedCalibrationTypeEnum
    {
        Обычный = 0,
        Заправка = 1,
        Выработка = 2,
        Зеркало_Заправка = 3,
        Зеркало_Выработка = 4,
        Опорная_МПИ_Общий = 5,
        Опорная_МПИ_Заправка = 6,
        Опорная_МПИ_Выработка = 7
    }
}
