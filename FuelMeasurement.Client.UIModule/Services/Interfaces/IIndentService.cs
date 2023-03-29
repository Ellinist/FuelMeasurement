using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.UIModule.Services.Interfaces
{
    public interface IIndentService
    {
        void Indent(
            Vector3 inTankUpPoint, 
            Vector3 inTankDownPoint,
            float upPointIndent,
            float downPointIndent,
            out Vector3 upPoint, 
            out Vector3 downPoint
            );
    }
}
