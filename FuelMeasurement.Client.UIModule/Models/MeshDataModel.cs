using FuelMeasurement.Common.Enums;
using HelixToolkit.SharpDX.Core.Model;
using System.Windows;
using System.Windows.Media.Media3D;

namespace FuelMeasurement.Client.UIModule.Models
{
    public abstract class MeshDataModel : ObservableObject
    {
        protected MeshDataModel(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public abstract MeshType MeshType { get; }

        public string Id { get; init; }

        public string Name { get; init; }

        public Transform3D Transform
        {
            get => _transform;
            set => Set(ref _transform, value);
        }
        private Transform3D _transform;

        public Visibility Visibility
        {
            get => _visibility;
            set => Set(ref _visibility, value);
        }
        private Visibility _visibility;
    }
}
