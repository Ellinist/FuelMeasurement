using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace FuelMeasurement.Client.UIModule.Infrastructure.Extensions
{
    public static class PointExtensions
    {
        public static System.Numerics.Vector3 ToVector3(this SharpDX.Vector3 vector)
        {
            return new System.Numerics.Vector3(
                vector.X, 
                vector.Y, 
                vector.Z
                );
        }

        public static SharpDX.Vector3 ToVector3(this Point3D point)
        {
            return new SharpDX.Vector3(
                Convert.ToSingle(point.X),
                Convert.ToSingle(point.Y),
                Convert.ToSingle(point.Z)
                );
        }

        public static System.Numerics.Vector3 ToVector3AndRound(this SharpDX.Vector3 vector, int roundValue = 2)
        {
            return new System.Numerics.Vector3(
                Convert.ToSingle(Math.Round(vector.X, roundValue)),
                Convert.ToSingle(Math.Round(vector.Y, roundValue)),
                Convert.ToSingle(Math.Round(vector.Z, roundValue))
                );
        }

        public static Point3D ToPoint3DAndRound(this SharpDX.Vector3 vector, int roundValue = 2)
        {
            return new Point3D(
                Math.Round(vector.X, roundValue),
                Math.Round(vector.Y, roundValue),
                Math.Round(vector.Z, roundValue)
                );
        }
    }
}
