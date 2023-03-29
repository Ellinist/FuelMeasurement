using FuelMeasurement.Client.UIModule.Infrastructure;
using FuelMeasurement.Client.UIModule.Models;
using FuelMeasurement.Client.UIModule.Services.Interfaces;
using HelixToolkit.SharpDX.Core;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.UIModule.Services.Implementations
{
    public class HitTestService : IHitTestService
    {
        public bool TryFindSensorPoints(Vector3 point, GeometryInfoMesh3D mesh, out List<HitTestResult> results)
        {
            results = new List<HitTestResult>();

            if (mesh.TryFindHits(point, RenderingConstants.HitTestDownVector, out results))
            {
                if (results.Count > 1)
                {
                    return true;
                }
                else
                {
                    results.Clear();

                    var uppderPoint = new Vector3()
                    {
                        X = point.X,
                        Y = point.Y + 100f,
                        Z = point.Z
                    };

                    if (mesh.TryFindHits(uppderPoint, RenderingConstants.HitTestDownVector, out results))
                    {
                        if (results.Count > 1)
                        {
                            return true;
                        }
                        else
                        {
                            results.Clear();

                            var lowerPoint = new Vector3()
                            {
                                X = point.X,
                                Y = point.Y - 100f,
                                Z = point.Z
                            };

                            if (mesh.TryFindHits(uppderPoint, RenderingConstants.HitTestDownVector, out results))
                            {
                                if (results.Count > 1)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
    }
}
