using Assimp;
using System.ComponentModel;

namespace FuelMeasurement.Model.Models.GeometryModels
{
    /// <summary>
    /// Модель треугольника
    /// </summary>
    [DisplayName("Треугольник")]
    public class Triangle
    {
        public Vector3D A { get; set; }
        public Vector3D B { get; set; }
        public Vector3D C { get; set; }

        /// <summary>
        /// Кноструктор для треугольника и известной нормалью
        /// </summary>
        /// <param name="a">Вершина a</param>
        /// <param name="b">Вершина b</param>
        /// <param name="c">Вершина c</param>
        public Triangle(Vector3D a, Vector3D b, Vector3D c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
}
