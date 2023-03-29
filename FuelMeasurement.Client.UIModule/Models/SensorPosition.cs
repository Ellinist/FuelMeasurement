using HelixToolkit.SharpDX.Core.Model;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.UIModule.Models
{
    public class SensorPosition : ObservableObject
    {
        public Vector3 UpPoint
        {
            get => _upPoint;
            set => Set(ref _upPoint, value);
        }

        private Vector3 _upPoint;

        public Vector3 DownPoint
        {
            get => _downPoint;
            set => Set(ref _downPoint, value);
        }
        private Vector3 _downPoint;

        public Vector3 InTankUpPoint
        {
            get => _inTankUpPoint;
            set => Set(ref _inTankUpPoint, value);
        }

        private Vector3 _inTankUpPoint;

        public Vector3 InTankDownPoint
        {
            get => _inTankDownPoint;
            set => Set(ref _inTankDownPoint, value);
        }

        private Vector3 _inTankDownPoint;

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


        public SensorPosition(
            Vector3 upPoint,
            Vector3 downPoint,
            Vector3 inTankUpPoint,
            Vector3 inTankDownPoint,
            float upPointIndent,
            float downPointIndent
            )
        {
            UpPoint = upPoint;
            DownPoint = downPoint;

            InTankUpPoint = inTankUpPoint;
            InTankDownPoint = inTankDownPoint;

            UpPointIndent = upPointIndent;
            DownPointIndent = downPointIndent;
        }
    }
}
