using FuelMeasurement.Client.UIModule.Services.Interfaces;
using FuelMeasurement.Common.Enums;
using FuelMeasurement.Model.Models.GeometryModels;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System;
using System.Windows;

namespace FuelMeasurement.Client.UIModule.Models
{
    public delegate void SensorChangePositionEvent(string id, SensorPosition newPosition);

    public class FuelSensorMesh : MeshDataModel
    {
        private readonly IIndentService _indentService;

        //private static readonly int _sensorThetaDiv = 16;
        private static readonly double _arrowHeadLength = 0;
        private static readonly double _arrowDiameter = 5;
        private static readonly int _arrowThetaDiv = 4;
        private static readonly double _sphereRadius = 8;
        private static readonly int _sphereThetaDiv = 8;
        private static readonly int _spherePhiDiv = 8;
        //private static readonly int _manipulationSphereThetaDiv = 8;
        //private static readonly int _manipulationSpherePhiaDiv = 8;

        public string FuelTankId
        {
            get => _fuelTankId;
            set => Set(ref _fuelTankId, value);
        }
        private string _fuelTankId;

        public override MeshType MeshType => MeshType.FuelSensor;

        public event SensorChangePositionEvent SensorChangePosition;

        #region IsSelect

        private bool _isSelected;

        public void Select()
        {
            if (!_isSelected)
            {
                _isSelected = true;
                RaisePropertyChanged(nameof(Material));
                RaisePropertyChanged(nameof(Material2));
            }
        }

        public void UnSelect()
        {
            if (_isSelected)
            {
                _isSelected = false;
                RaisePropertyChanged(nameof(Material));
                RaisePropertyChanged(nameof(Material2));
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
                if (_isSelected)
                    return CreateSelectedMatherial();

                return _material;
            }
            set => Set(ref _material, value);
        }

        public double Diameter
        {
            get => _diameter;
            set => Set(ref _diameter, value);
        }
        private double _diameter;

        public SensorPosition SensorPosition
        {
            get => _sensorPosition;
            set => Set(ref _sensorPosition, value);
        }

        private SensorPosition _sensorPosition;

        

        public Material Material2
        {
            get
            {
                if (_isSelected)
                    return CreateSelectedMatherial2();

                return _material2;
            }
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


        public float UpPointIndent
        {
            get => _upPointIndent;
            set => Set(ref _upPointIndent, value);
        }

        private float _upPointIndent;

        public float DownPointIndent
        {
            get => _downPointIndent;
            set => Set(ref _downPointIndent, value);
        }

        private float _downPointIndent;

        public float Length
        {
            get => length;
            set => Set(ref length, value);
        }

        private float length;

        public double LinearCapacity
        {
            get => linearCapacity;
            set => Set(ref linearCapacity, value);
        }

        private double linearCapacity;
        #endregion

        // Этот конструктор для датчиков, которые создаём руками в программе
        public FuelSensorMesh(
            string id,
            string name,
            PhongMaterial material,
            Vector3 inTankUpPoint,
            Vector3 inTankDownPoint,
            IIndentService indentService,
            string fueltTankId,
            double sensorDiam = 50,
            float upPointIndent = 15,
            float downPointIndent = 15
            ) : base(id, name)
        {
            Material = material;

            Diameter = sensorDiam;
            UpPointIndent = upPointIndent;
            DownPointIndent = downPointIndent;
            FuelTankId = fueltTankId;
            _indentService = indentService ?? throw new ArgumentNullException(nameof(indentService));

            _indentService.Indent(
                inTankUpPoint,
                inTankDownPoint, 
                UpPointIndent, 
                DownPointIndent, 
                out var upPoint, 
                out var downPoint
                );

            SensorPosition = new SensorPosition(upPoint, downPoint, inTankUpPoint, inTankDownPoint, UpPointIndent, DownPointIndent);
            Length = (upPoint - downPoint).Length();
            LinearCapacity = 0.1; // Потом брать из настроек/справочника/священного писания или конституции
            CreateGeometry(Id, Name, MeshType);
        }

        // Этот конструктор для датчиков, которые уже были созданны 
        public FuelSensorMesh(
            string id,
            string name,
            PhongMaterial material,
            Vector3 upPoint,
            Vector3 downPoint,
            Vector3 inTankUpPoint,
            Vector3 inTankDownPoint,
            IIndentService indentService,
            string fueltTankId,
            double sensorDiam = 50,
            float upPointIndent = 15,
            float downPointIndent = 15
            ) : base(id, name)
        {
            Material = material;

            Diameter = sensorDiam;
            UpPointIndent = upPointIndent;
            DownPointIndent = downPointIndent;
            FuelTankId = fueltTankId;

            _indentService = indentService ?? throw new ArgumentNullException(nameof(indentService));

            SensorPosition = new SensorPosition(upPoint, downPoint, inTankUpPoint, inTankDownPoint, UpPointIndent, DownPointIndent);
            Length = (upPoint - downPoint).Length();
            LinearCapacity = 0.1; // Потом брать из настроек/справочника/священного писания или конституции
            CreateGeometry(Id, Name, MeshType);
        }

        public bool TrySetNewPosition(Vector3 inTankUpPoint, Vector3 inTankDownPoint)
        {
            _indentService.Indent(
                inTankUpPoint,
                inTankDownPoint, 
                UpPointIndent, 
                DownPointIndent, 
                out var upPoint, 
                out var downPoint
                );

            SensorPosition.UpPoint = upPoint;
            SensorPosition.DownPoint = downPoint;
            SensorPosition.InTankUpPoint = inTankUpPoint;
            SensorPosition.InTankDownPoint = inTankDownPoint;

            Length = (upPoint - downPoint).Length();
            CreateGeometry(Id, Name, MeshType);

            SensorChangePosition?.Invoke(Id, SensorPosition);

            return true;
        }

        private void CreateGeometry(string id, string name, MeshType meshType)
        {
            MeshBuilder builder = new(true);
            builder.AddCylinder(SensorPosition.UpPoint, SensorPosition.DownPoint, Diameter, 12, true, true);

            builder.AddArrow(SensorPosition.UpPoint, SensorPosition.InTankUpPoint, _arrowDiameter, _arrowHeadLength, _arrowThetaDiv);
            builder.AddArrow(SensorPosition.DownPoint, SensorPosition.InTankDownPoint, _arrowDiameter, _arrowHeadLength, _arrowThetaDiv);

            builder.AddSphere(SensorPosition.InTankUpPoint, _sphereRadius, _sphereThetaDiv, _spherePhiDiv);
            builder.AddSphere(SensorPosition.InTankDownPoint, _sphereRadius, _sphereThetaDiv, _spherePhiDiv);

            Geometry = new GeometryInfoMesh3D(builder.ToMesh(), id, meshType, id);

            Billboard = new BillboardSingleText3D
            {
                TextInfo = new TextInfo(name, new Vector3(
                    SensorPosition.InTankUpPoint.X,
                    SensorPosition.InTankUpPoint.Y + 50,
                    SensorPosition.InTankUpPoint.Z)),
                FontSize = 30,
                FontColor = Color.White,
                FontStyle = SharpDX.DirectWrite.FontStyle.Normal,
            };
        }

        private Material CreateSelectedMatherial()
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

        private Material CreateSelectedMatherial2()
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
