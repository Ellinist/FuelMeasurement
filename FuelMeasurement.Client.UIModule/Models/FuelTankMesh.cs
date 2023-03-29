using FuelMeasurement.Common.Enums;
using FuelMeasurement.Model.Models.GeometryModels;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using Material = HelixToolkit.Wpf.SharpDX.Material;

namespace FuelMeasurement.Client.UIModule.Models
{
    public class FuelTankMesh : MeshDataModel
    {
        #region IsSelect

        private bool _isSeleted;

        public void Select()
        {
            if (!_isSeleted)
            {
                _isSeleted = true;
                RaisePropertyChanged(nameof(Material));
            }
        }

        public void UnSelect()
        {
            if (_isSeleted)
            {
                _isSeleted = false;
                RaisePropertyChanged(nameof(Material));
            }
        }

        #endregion

        #region GeometryInfo

        private GeometryInfoMesh3D _geometry;
        private Material _material;
        private Material _material2;

        private BillboardSingleText3D _billboard;
        private Visibility _visibilityBillboard;

        public GeometryInfoMesh3D Geometry
        {
            get => _geometry;
            set => Set(ref _geometry, value);
        }

        public Material Material
        {
            get
            {
                if (_isSeleted)
                    return CreateSelectedMaterial();

                return _material;
            }
            set => Set(ref _material, value);
        }

        public Material Material2
        {
            get => _material2;
            set => Set(ref _material2, value);
        }

        public BillboardSingleText3D Billboard
        {
            get => _billboard;
            set => Set(ref _billboard, value);
        }

        public Visibility VisibilityBillboard
        {
            get => _visibilityBillboard;
            set => Set(ref _visibilityBillboard, value);
        }

        #endregion


        public List<HeatMapZoneModel> UpZones
        {
            get => _upZones;
            set => Set(ref _upZones, value);
        }

        private List<HeatMapZoneModel> _upZones = new();

        public List<HeatMapZoneModel> DownZones
        {
            get => _downZones;
            set => Set(ref _downZones, value);
        }

        private List<HeatMapZoneModel> _downZones = new();

        public override MeshType MeshType => MeshType.FuelTank;

        public FuelTankMesh(
            PhongMaterial material,
            PhongMaterial material2,
            FuelTankGeometryModel geometry
            ) : base(geometry.Id, geometry.FuelTankName)
        {
            CreateGeometry(geometry, geometry.Id, geometry.FuelTankName, MeshType);
            Material = material;
            Material2 = material2;
            Visibility = Visibility.Visible;
        }
        
        private void CreateGeometry(FuelTankGeometryModel geometry, string id, string name, MeshType meshType)
        {
            MeshBuilder builder = new(true, true);

            geometry.Mesh.Triangles.ForEach(triangle => 
            {
                builder.AddTriangle(
                    new Vector3(triangle.A.X, triangle.A.Y, triangle.A.Z),
                    new Vector3(triangle.B.X, triangle.B.Y, triangle.B.Z),
                    new Vector3(triangle.C.X, triangle.C.Y, triangle.C.Z)
                    );
            });

            Geometry = new GeometryInfoMesh3D(builder.ToMesh(), id, meshType, id)
            {
                ReturnMultipleHitsOnHitTest = true
            };

            Billboard = new BillboardSingleText3D
            {
                TextInfo = new TextInfo(name, new Vector3(
                    Geometry.BoundingSphere.Center.X, 
                    Geometry.BoundingSphere.Center.Y + 500, 
                    Geometry.BoundingSphere.Center.Z)),
                FontSize = 30,
                FontColor = Color.White,
                FontStyle = SharpDX.DirectWrite.FontStyle.Normal,
            };
        }

        private Material CreateSelectedMaterial()
        {
            if (_material is PhongMaterial phongMaterial)
            {
                Color4 correctedColor = phongMaterial.DiffuseColor;

                return new PhongMaterial
                {
                    AmbientColor = phongMaterial.AmbientColor,
                    DiffuseColor = phongMaterial.DiffuseColor,
                    SpecularColor = phongMaterial.SpecularColor,
                    EmissiveColor =
                        PhongMaterials.ToColor(
                            correctedColor.Red,
                            correctedColor.Green,
                            correctedColor.Blue,
                            1.0
                            ),
                };
            }

            return null;
        }

        /// <summary>
        /// Этот элемент пока не используется
        /// </summary>
        /// <returns></returns>
        private Material CreateSelectedMaterial2()
        {
            if (_material2 is PhongMaterial phongMaterial)
            {
                var correctedColor = phongMaterial.DiffuseColor;

                return new PhongMaterial
                {
                    AmbientColor = phongMaterial.AmbientColor,
                    DiffuseColor = phongMaterial.DiffuseColor,
                    SpecularColor = phongMaterial.SpecularColor,

                    EmissiveColor =
                        PhongMaterials.ToColor(
                            correctedColor.Red,
                            correctedColor.Green,
                            correctedColor.Blue,
                            1.0
                            ),
                };
            }

            return null;
        }
    }
}
