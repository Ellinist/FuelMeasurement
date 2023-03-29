using FuelMeasurement.Common.Enums;
using Prism.Events;

namespace FuelMeasurement.Common.Events.Reports
{
    public class GenerateReportEvent : PubSubEvent<ReportTypeEnum>
    {

    }
}