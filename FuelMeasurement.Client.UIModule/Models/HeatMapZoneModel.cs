using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.UIModule.Models
{
    public class HeatMapZoneModel
    {
        public List<Vector3?> Points { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinZ { get; set; }
        public double MaxZ { get; set; }
        public System.Windows.Media.Color ZoneColor { get; set; }
        public HeatMapZoneModel(List<Vector3?> points)
        {
            var ordered = points.OrderBy(a => a?.Y).ToList();
            Points = ordered;

            MaxY = (double)points.Max(a => a?.Y);
            MinY = (double)points.Min(a => a?.Y);

            MaxX = (double)points.Max(a => a?.X);
            MinX = (double)points.Min(a => a?.X);

            MaxZ = (double)points.Max(a => a?.Z);
            MinZ = (double)points.Min(a => a?.Z);
        }
    }
}
