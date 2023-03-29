using System;
using Assimp;

namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Класс сегмента - это отрезок между двумя точками
    /// </summary>
    public class LineSegment
    {
        /// <summary>
        /// Точка начала сегмента
        /// </summary>
        public Vector3D BeginOfSegment { get; }

        /// <summary>
        /// Точка конца сегмента
        /// </summary>
        public Vector3D EndOfSegment { get; }

        /// <summary>
        /// Свойство - длина сегмента
        /// </summary>
        public double LengthOfSegment { get; }

        /// <summary> Конструктор сегмента </summary>
        /// <param name="begin">Начальная точка сегмента</param>
        /// <param name="end">Конечная точка сегмента</param>
        public LineSegment(Vector3D begin, Vector3D end)
        {
            BeginOfSegment = begin;
            EndOfSegment = end;
            LengthOfSegment = (float)Math.Sqrt((EndOfSegment.X - BeginOfSegment.X) * (EndOfSegment.X - BeginOfSegment.X) +
                                      (EndOfSegment.Y - BeginOfSegment.Y) * (EndOfSegment.Y - BeginOfSegment.Y) +
                                      (EndOfSegment.Z - BeginOfSegment.Z) * (EndOfSegment.Z - BeginOfSegment.Z));
        }
    }
}
