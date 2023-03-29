using FuelMeasurement.Client.UIModule.Services.Interfaces;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.UIModule.Services.Implementations
{
    public class IndentService : IIndentService
    {
        public void Indent(
            Vector3 inTankUpPoint, 
            Vector3 inTankDownPoint, 
            float upPointIndent,
            float downPointIndent, 
            out Vector3 upPoint, 
            out Vector3 downPoint)
        {
            Vector3 vectorUp = new(0, -upPointIndent, 0);
            Vector3 vectorDown = new(0, downPointIndent, 0);

            upPoint = inTankUpPoint + vectorUp;
            downPoint = inTankDownPoint + vectorDown;
        }
    }
}
