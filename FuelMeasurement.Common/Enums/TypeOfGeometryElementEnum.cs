using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Common.Enums
{
    public enum TypeOfGeometryElementEnum
    {
        FuelSensor = 0, // Датчик
        FuelTank = 1, // Бак
        ManipulationSphere = 2, // Сфера
        IgnorableHitGeometryItem = 3 // Игнорируемый объект
    }
}
