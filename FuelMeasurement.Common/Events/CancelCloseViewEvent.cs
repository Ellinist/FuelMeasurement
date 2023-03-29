using FuelMeasurement.Common.Enums;
using FuelMeasurement.Common.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Common.Events
{
    public class CloseViewEvent : PubSubEvent<CloseViewParams>
    {
    }
}
