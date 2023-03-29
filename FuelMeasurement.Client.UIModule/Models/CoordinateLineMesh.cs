using FuelMeasurement.Common.Enums;
using FuelMeasurement.Client.UIModule.Infrastructure;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System;

namespace FuelMeasurement.Client.UIModule.Models
{
    public class CoordinateLineMesh : MeshDataModel
    {
        private GeometryInfoMesh3D _geometry;
        public GeometryInfoMesh3D Geometry
        {
            get => _geometry;
            set => Set(ref _geometry, value);
        }

        private Material _material;
        public Material Material
        {
            get
            {
                return _material;
            }
            set => Set(ref _material, value);
        }

        private BillboardSingleText3D _billboard;
        public BillboardSingleText3D Billboard
        {
            get => _billboard;
            set => Set(ref _billboard, value);
        }

        public override MeshType MeshType => MeshType.Line;

        public CoordinateLineMesh(
            PhongMaterial material,
            CoordinateLineType lineType
            ) : base(Guid.NewGuid().ToString(), "")
        {
            CreateGeometry(lineType, MeshType);
            Material = material;
        }

        private void CreateGeometry(CoordinateLineType lineType, MeshType meshType)
        {
            var endPoint = new Vector3();
            string text = string.Empty;

            switch (lineType)
            {
                case CoordinateLineType.X:
                    endPoint = CoordinateLineDefaultValues.LineXEndPoint;
                    text = "X";
                    break;
                case CoordinateLineType.Y:
                    endPoint = CoordinateLineDefaultValues.LineYEndPoint;
                    text = "Y";
                    break;
                case CoordinateLineType.Z:
                    endPoint = CoordinateLineDefaultValues.LineZEndPoint;
                    text = "Z";
                    break;
            }

            MeshBuilder builder = new MeshBuilder(true, true);
            builder.AddArrow(
                CoordinateLineDefaultValues.LineStartPoint,
                endPoint,
                CoordinateLineDefaultValues.DefaultDiamert
                );

            Geometry = new GeometryInfoMesh3D(builder.ToMesh(), Id, meshType, Id);

            Billboard = new BillboardSingleText3D
            {
                TextInfo = new TextInfo(text, endPoint),
                FontSize = 30,
                FontColor = Color.White,
                FontStyle = SharpDX.DirectWrite.FontStyle.Normal,
            };
        }
    }
}
