using Assimp;
using System.ComponentModel;
using FuelMeasurement.Common.Enums;

namespace FuelMeasurement.Model.Models.GeometryModels
{
    [DisplayName("Геометрия модели")]
    public class MeshModel
    {
        public List<Triangle> Triangles { get; set; } = new();

        public List<Face> Faces { get; set; } = new();

        public List<Vector3D> Normales { get; set; } = new();

        public MeshType TankType { get; set; }
    }
}