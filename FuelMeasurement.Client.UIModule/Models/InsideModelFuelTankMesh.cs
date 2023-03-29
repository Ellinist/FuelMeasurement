using FuelMeasurement.Common.Enums;
using FuelMeasurement.Model.Models.GeometryModels;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.UIModule.Models
{
    public class InsideModelFuelTankMesh : MeshDataModel
    {
        private GeometryInfoMesh3D _geometry;
        private Material _material;

        public GeometryInfoMesh3D Geometry
        {
            get => _geometry;
            set => Set(ref _geometry, value);
        }
        public Material Material
        {
            get => _material;
            set => Set(ref _material, value);
        }

        public InsideModelFuelTankMesh(
            PhongMaterial material,
            PhongMaterial material2,
            FuelTankGeometryModel geometry) 
            : base(geometry.Id, geometry.FuelTankName)
        {
            CreateGeometry(geometry, geometry.Id, geometry.FuelTankName, MeshType);
            Material = material;
            Visibility = System.Windows.Visibility.Visible;
        }

        public override MeshType MeshType => MeshType.InsideModel;

        private void CreateGeometry(FuelTankGeometryModel geometry, string id, string name, MeshType meshType)
        {
            MeshBuilder builder = new MeshBuilder(true, true);

            geometry.Mesh.Triangles.ForEach(triangle =>
            {
                builder.AddTriangle(
                    new Vector3(triangle.A.X, triangle.A.Y, triangle.A.Z),
                    new Vector3(triangle.B.X, triangle.B.Y, triangle.B.Z),
                    new Vector3(triangle.C.X, triangle.C.Y, triangle.C.Z)
                    );
            });

            Geometry = new GeometryInfoMesh3D(builder.ToMesh(), id, meshType, id);
            Geometry.ReturnMultipleHitsOnHitTest = false;
        }
    }
}
