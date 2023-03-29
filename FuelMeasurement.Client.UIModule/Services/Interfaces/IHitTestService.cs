using FuelMeasurement.Client.UIModule.Models;
using HelixToolkit.SharpDX.Core;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.UIModule.Services.Interfaces
{
    public interface IHitTestService
    {
        bool TryFindSensorPoints(Vector3 point, GeometryInfoMesh3D mesh, out List<HitTestResult> hits);

    }
}
