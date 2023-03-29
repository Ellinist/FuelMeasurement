using Assimp;
using System;

namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Класс треугольника тарировщика
    /// </summary>
    public class Triangle
    {
        //public Vector3D A { get; set; }
        //public Vector3D B { get; set; }
        //public Vector3D C { get; set; }

        /// <summary>
        /// Массив вершин треугольника
        /// </summary>
        public readonly Vector3D[] Vertices = new Vector3D[3]; // У треугольника три вершины

        /// <summary>
        /// Массив нормалей (векторов) треугольника
        /// На всякий случай
        /// </summary>
        public Vector3D NormalVector { get; init; }

        /// <summary>
        /// Описание сторон (сегментов) треугольника
        /// </summary>
        public readonly LineSegment[] Sides = new LineSegment[3]; // У треугольника три стороны

        /// <summary>
        /// Свойство - площадь треугольника
        /// </summary>
        public float Square { get; set; }

        /// <summary>
        /// Конструктор треугольника по отдельным вершинам
        /// </summary>
        /// <param name="a">вершина A</param>
        /// <param name="b">вершина B</param>
        /// <param name="c">вершина C</param>
        public Triangle(Vector3D a, Vector3D b, Vector3D c)
        {
            Vertices[0] = a;
            Vertices[1] = b;
            Vertices[2] = c;
            // Определение сегментов (сторон) треугольника
            Sides[0] = new LineSegment(a, b);
            Sides[1] = new LineSegment(b, c);
            Sides[2] = new LineSegment(a, c);

            SetSquare(); // Вычисление площади треугольника
        }

        /// <summary>
        /// Вычисление площади треугольника по формуле Герона
        /// </summary>
        private void SetSquare()
        {
            // Вычисляется периметр треугольника
            var p = (Sides[0].LengthOfSegment + Sides[1].LengthOfSegment + Sides[2].LengthOfSegment) / 2;

            // Вычисляется площадь треугольника
            Square = (float)Math.Sqrt(Math.Abs(p * (p - Sides[0].LengthOfSegment) * (p - Sides[1].LengthOfSegment) * (p - Sides[2].LengthOfSegment)));
        }
    }
}
