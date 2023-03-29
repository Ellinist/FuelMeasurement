using FuelMeasurement.Common.Enums;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.UIModule.Events
{
    public class SwitchViewerModeEvent : PubSubEvent<ViewerWorkingModes>
    {
    }
}
