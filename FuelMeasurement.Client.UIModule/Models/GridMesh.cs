using FuelMeasurement.Common.Enums;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using System;
using System.Windows.Media.Media3D;

namespace FuelMeasurement.Client.UIModule.Models
{
    public class GridMesh : MeshDataModel
    {
        public override MeshType MeshType => MeshType.Grid;

        public GridMesh(
            PhongMaterial material,
            double thickness
            ) : base(Guid.NewGuid().ToString(), "")
        {
            Material = material;
            Center = new Point3D(0, 0, 0);
            MajorDistance = 5000;
            MinorDistance = 5000;
            Normal = new Vector3D(0, 1, 0);
            LengthDirection = new Vector3D(1, 0, 0);
            Width = 100000;
            Length = 100000;
            Thickness = thickness;
            CreateGeometry(MeshType);
        }

        private HelixToolkit.Wpf.SharpDX.Material _material;
        public HelixToolkit.Wpf.SharpDX.Material Material
        {
            get
            {
                return _material;
            }
            set => Set(ref _material, value);
        }

        private GeometryInfoMesh3D _geometry;
        public GeometryInfoMesh3D Geometry
        {
            get => _geometry;
            set => Set(ref _geometry, value);
        }

        private void CreateGeometry(MeshType meshType)
        {
            LengthDirection.Normalize();

            // #136, chrkon, 2015-03-26
            // if NormalVector and LenghtDirection are not perpendicular then overwrite LengthDirection
            if (System.Windows.Media.Media3D.Vector3D.DotProduct(Normal, LengthDirection) != 0.0)
            {
                LengthDirection = FindAnyPerpendicular(Normal);
                LengthDirection.Normalize();
            }

            // create WidthDirection by rotating lengthDirection vector 90° around normal vector
            var rotate = new System.Windows.Media.Media3D.RotateTransform3D(
                new System.Windows.Media.Media3D.AxisAngleRotation3D(
                    this.Normal, 
                    90.0)
                );
            WidthDirection = rotate.Transform(LengthDirection);
            WidthDirection.Normalize();

            var builder = new MeshBuilder(true, false);
            double minX = -this.Width / 2;
            double minY = -this.Length / 2;
            double maxX = this.Width / 2;
            double maxY = this.Length / 2;

            double x = minX;
            double eps = this.MinorDistance / 10;
            while (x <= maxX + eps)
            {
                double t = this.Thickness;
                if (IsMultipleOf(x, this.MajorDistance))
                {
                    t *= 2;
                }

                this.AddLineX(builder, x, minY, maxY, t);
                x += this.MinorDistance;
            }

            double y = minY;
            while (y <= maxY + eps)
            {
                double t = Thickness;
                if (IsMultipleOf(y, this.MajorDistance))
                {
                    t *= 2;
                }

                this.AddLineY(builder, y, minX, maxX, t);
                y += this.MinorDistance;
            }

            Geometry = new GeometryInfoMesh3D(builder.ToMesh(), Id, meshType, Id);
        }

        private System.Windows.Media.Media3D.Point3D _center;
        public System.Windows.Media.Media3D.Point3D Center
        {
            get => _center;
            set => Set(ref _center, value);
        }

        private double _majorDistance;
        public double MajorDistance
        {
            get => _majorDistance;
            set => Set(ref _majorDistance, value);
        }
        private double _minorDistance;
        public double MinorDistance
        {
            get => _minorDistance;
            set => Set(ref _minorDistance, value);
        }

        private System.Windows.Media.Media3D.Vector3D _lengthDirection;
        public System.Windows.Media.Media3D.Vector3D LengthDirection
        {
            get => _lengthDirection;
            set => Set(ref _lengthDirection, value);
        }

        private System.Windows.Media.Media3D.Vector3D _widthDirection;
        public System.Windows.Media.Media3D.Vector3D WidthDirection
        {
            get => _widthDirection;
            set => Set(ref _widthDirection, value);
        }


        private double _length;
        public double Length
        {
            get => _length;
            set => Set(ref _length, value);
        }

        private double _width;
        public double Width
        {
            get => _width;
            set => Set(ref _width, value);
        }

        private System.Windows.Media.Media3D.Vector3D _normal;
        public System.Windows.Media.Media3D.Vector3D Normal
        {
            get => _normal;
            set => Set(ref _normal, value);
        }

        private double _thickness;
        public double Thickness
        {
            get => _thickness;
            set => Set(ref _thickness, value);
        }

        private static bool IsMultipleOf(double y, double d)
        {
            double y2 = d * (int)(y / d);
            return Math.Abs(y - y2) < 1e-3;
        }

        private void AddLineX(MeshBuilder mesh, double x, double minY, double maxY, double thickness)
        {
            int i0 = mesh.Positions.Count;
            mesh.Positions.Add(GetPoint(x - (thickness / 2), minY).ToVector3());
            mesh.Positions.Add(GetPoint(x - (thickness / 2), maxY).ToVector3());
            mesh.Positions.Add(GetPoint(x + (thickness / 2), maxY).ToVector3());
            mesh.Positions.Add(GetPoint(x + (thickness / 2), minY).ToVector3());
            mesh.Normals.Add(Normal.ToVector3());
            mesh.Normals.Add(Normal.ToVector3());
            mesh.Normals.Add(Normal.ToVector3());
            mesh.Normals.Add(Normal.ToVector3());
            mesh.TriangleIndices.Add(i0);
            mesh.TriangleIndices.Add(i0 + 1);
            mesh.TriangleIndices.Add(i0 + 2);
            mesh.TriangleIndices.Add(i0 + 2);
            mesh.TriangleIndices.Add(i0 + 3);
            mesh.TriangleIndices.Add(i0);
        }

        private void AddLineY(MeshBuilder mesh, double y, double minX, double maxX, double thickness)
        {
            int i0 = mesh.Positions.Count;
            mesh.Positions.Add(GetPoint(minX, y + (thickness / 2)).ToVector3());
            mesh.Positions.Add(GetPoint(maxX, y + (thickness / 2)).ToVector3());
            mesh.Positions.Add(GetPoint(maxX, y - (thickness / 2)).ToVector3());
            mesh.Positions.Add(GetPoint(minX, y - (thickness / 2)).ToVector3());
            mesh.Normals.Add(Normal.ToVector3());
            mesh.Normals.Add(Normal.ToVector3());
            mesh.Normals.Add(Normal.ToVector3());
            mesh.Normals.Add(Normal.ToVector3());
            mesh.TriangleIndices.Add(i0);
            mesh.TriangleIndices.Add(i0 + 1);
            mesh.TriangleIndices.Add(i0 + 2);
            mesh.TriangleIndices.Add(i0 + 2);
            mesh.TriangleIndices.Add(i0 + 3);
            mesh.TriangleIndices.Add(i0);
        }

        private System.Windows.Media.Media3D.Point3D GetPoint(double x, double y)
        {
            return (Point3D)(Center + (WidthDirection * x) + (LengthDirection * y));
        }

        private static Vector3D FindAnyPerpendicular(Vector3D n)
        {
            n.Normalize();
            Vector3D u = Vector3D.CrossProduct(new Vector3D(0, 1, 0), n);
            if (u.LengthSquared < 1e-3)
            {
                u = Vector3D.CrossProduct(new Vector3D(1, 0, 0), n);
            }

            return u;
        }
    }
}
