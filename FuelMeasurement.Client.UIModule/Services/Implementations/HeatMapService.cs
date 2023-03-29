using FuelMeasurement.Client.UIModule.Infrastructure;
using FuelMeasurement.Client.UIModule.Models;
using FuelMeasurement.Client.UIModule.Services.Interfaces;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FuelMeasurement.Client.UIModule.Services.Implementations
{
    public class HeatMapService : IHeatMapService
    {
        public const int    DefaultHeatMapDelta = 10;
        public const double DefaultStepCount = 50; 
        public const double DefaultHeatMapSizeCoef = 1.5;
        public const double DefaultHeatMapZoneSize = 20;

        public static System.Windows.Media.Color DownZonesHeatMapZoneColor1 = Colors.Navy;
        public static System.Windows.Media.Color DownZonesHeatMapZoneColor2 = Colors.Blue;
        public static System.Windows.Media.Color DownZonesHeatMapZoneColor3 = Colors.LightSkyBlue;
        public static System.Windows.Media.Color DownZonesHeatMapZoneColor4 = Colors.Aqua;

        public static System.Windows.Media.Color UpZonesHeatMapZoneColor1 = Colors.Maroon;
        public static System.Windows.Media.Color UpZonesHeatMapZoneColor2 = Colors.Crimson;
        public static System.Windows.Media.Color UpZonesHeatMapZoneColor3 = Colors.Red;
        public static System.Windows.Media.Color UpZonesHeatMapZoneColor4 = Colors.Tomato;


        private static double _stepX;
        private static double _stepZ;

        private static int _pointXsize;
        private static int _pointZsize;

        public HeatMapService()
        {

        }

        public void CreateHeatMap(FuelTankMesh tankMesh)
        {
            var bounds = tankMesh.Geometry.Bound;

            if (bounds.Width > bounds.Height)
            {
                _stepX = DefaultStepCount * DefaultHeatMapSizeCoef;
                _stepZ = DefaultStepCount;
            }
            else
            {
                _stepX = DefaultStepCount;
                _stepZ = DefaultStepCount * DefaultHeatMapSizeCoef;
            }

            _pointXsize = (int)Math.Round(bounds.Width / _stepX);
            _pointZsize = (int)Math.Round(bounds.Height / _stepZ);

            CreateMap(tankMesh);
        }

        private void CreateMap(FuelTankMesh tankMesh)
        {
            var positions = tankMesh.Geometry.Positions.ToList();

            Vector3[,] pointsToHitTest = new Vector3
                [
                    (int)Math.Round(_stepX, 0), 
                    (int)Math.Round(_stepZ, 0)
                ];

            var maxYCoord = Math.Round(
                tankMesh.Geometry.Bound.Maximum.Y + 1, 2
                );

            var xLocation = Math.Round(
                positions.OrderBy(p => p.X)
                .First().X, 2
                );

            var zLocation = Math.Round(
                positions.OrderBy(p => p.Z)
                .First().Z, 2
                );

            for (int i = 0; i < _stepX; i++)
            {
                pointsToHitTest[i, 0] = CreatePoint(xLocation, maxYCoord, zLocation);

                xLocation += _pointXsize;

                for (int j = 0; j < _stepZ; j++)
                {
                    pointsToHitTest[i, j] = CreatePoint(pointsToHitTest[i, 0].X, maxYCoord, zLocation);

                    zLocation += _pointZsize;
                }
            }

            CreateZones(FindUpAndDownPoints(tankMesh, pointsToHitTest), tankMesh);
        }

        private static void SetZonesColors(FuelTankMesh tankMesh)
        {
            if (tankMesh.DownZones.Count > 2)
            {
                var zoneColors = CreateZonesColors(
                    tankMesh.DownZones.Count, 
                    DownZonesHeatMapZoneColor1, 
                    DownZonesHeatMapZoneColor2, 
                    DownZonesHeatMapZoneColor3, 
                    DownZonesHeatMapZoneColor4
                    );

                for (int i = 0; i < tankMesh.DownZones.Count; i++)
                {
                    tankMesh.DownZones[i].ZoneColor = zoneColors[i];
                }
            }
            else
            {
                for (int i = 0; i < tankMesh.DownZones.Count; i++)
                {
                    tankMesh.DownZones[i].ZoneColor = Colors.Blue;
                }
            }

            if (tankMesh.UpZones.Count > 2)
            {
                var zoneColors = CreateZonesColors(
                    tankMesh.UpZones.Count, 
                    UpZonesHeatMapZoneColor1, 
                    UpZonesHeatMapZoneColor2, 
                    UpZonesHeatMapZoneColor3, 
                    UpZonesHeatMapZoneColor4
                    );

                for (int i = 0; i < tankMesh.UpZones.Count; i++)
                {
                    tankMesh.UpZones[i].ZoneColor = zoneColors[i];
                }
            }
            else
            {
                for (int i = 0; i < tankMesh.UpZones.Count; i++)
                {
                    tankMesh.UpZones[i].ZoneColor = Colors.Salmon;
                }
            }
        }

        private static List<System.Windows.Media.Color> CreateZonesColors(
            int zonesCount, 
            System.Windows.Media.Color c1, 
            System.Windows.Media.Color c2, 
            System.Windows.Media.Color c3, 
            System.Windows.Media.Color c4
            )
        {
            List<System.Windows.Media.Color> colors = new();

            int colorsCount = 3;

            var zonesInColor = zonesCount / colorsCount;

            int test = 0;

            if (zonesCount % colorsCount != 0)
            {
                test = zonesCount % colorsCount;
            }

            colors.AddRange(ColorGradientHelper.GetColorGradient(c1, c2, zonesInColor).ToList());
            colors.AddRange(ColorGradientHelper.GetColorGradient(c2, c3, zonesInColor).ToList());
            colors.AddRange(ColorGradientHelper.GetColorGradient(c3, c4, zonesInColor + test).ToList());

            return colors;
        }

        private static void CreateZones((List<Vector3>, List<Vector3>) result, FuelTankMesh tankMesh)
        {
            tankMesh.UpZones.AddRange(CreateZone(result.Item1.OrderBy(a => a.Y).ToList()));
            tankMesh.DownZones.AddRange(CreateZone(result.Item2.OrderBy(a => a.Y).ToList()));

            SetZonesColors(tankMesh);
        }

        private static List<HeatMapZoneModel> CreateZone(List<Vector3> pointCollection)
        {
            int count = 0;

            List<HeatMapZoneModel> zones = new();

            while (count < pointCollection.Count)
            {
                var findValue = pointCollection[count].Y;
                List<Vector3?> zonePoints = new();

                for (int j = 0; j < DefaultHeatMapDelta; j++)
                {
                    var serchValue = findValue + j;

                    var find = zones.
                        FirstOrDefault(a => a.Points.
                        FirstOrDefault(a => a?.Y == serchValue)?.Y == serchValue);

                    if (find == null)
                    {
                        var p = pointCollection.FindAll(a => a.Y == serchValue);

                        if (p.Count > 0)
                        {
                            foreach (var point in p)
                            {
                                zonePoints.Add(point);
                            }
                        }

                        count += p.Count;
                    }
                }

                if (zonePoints.Count > 0)
                {
                    if (zonePoints.Count != 1) 
                    {
                        zones.Add(new HeatMapZoneModel(zonePoints)); 
                    }
                }
                else 
                { 
                    count++; 
                }
            }

            return zones;
        }

        private static (List<Vector3>, List<Vector3>) FindUpAndDownPoints(FuelTankMesh tankMesh, Vector3[,] pointsToHitTest)
        {
            List<Vector3> upPoints = new();
            List<Vector3> downPoints = new();

            for (int i = 0; i < _stepX; i++)
            {
                for (int j = 0; j < _stepZ; j++)
                {
                    if (tankMesh.Geometry.TryFindHits(pointsToHitTest[i, j], RenderingConstants.HitTestDownVector, out var hits))
                    {
                        var tankHits = hits.FindAll(x => (x.Geometry as GeometryInfoMesh3D) != null && 
                        (x.Geometry as GeometryInfoMesh3D).Id == tankMesh.Geometry.Id);

                        var first = hits.First().PointHit;
                        var second = hits.Last().PointHit;

                        if (first.Y > second.Y)
                        {
                            upPoints.Add(RoundPoint(first));
                            downPoints.Add(RoundPoint(second));
                        }
                        else
                        {
                            upPoints.Add(RoundPoint(second));
                            downPoints.Add(RoundPoint(first));
                        }
                    }
                    else
                    {
                        // чушь какая то тут происходит..
                    }
                }
            }

            return (upPoints, downPoints);
        }



        private static Vector3 CreatePoint(double x, double y, double z)
        {
            return new Vector3(
                Convert.ToSingle(Math.Round(x, 2)), 
                Convert.ToSingle(Math.Round(y, 2)), 
                Convert.ToSingle(Math.Round(z, 2))
                );
        }

        private static Vector3 RoundPoint(Vector3 pointHit)
        {
            return new Vector3(
                Convert.ToSingle(Math.Round(pointHit.X, 2)),
                Convert.ToSingle(Math.Round(pointHit.Y, 2)),
                Convert.ToSingle(Math.Round(pointHit.Z, 2))
                );
        }
    }
}
