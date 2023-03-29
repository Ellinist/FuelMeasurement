using Assimp;
using System;

namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Класс плоскости
    /// </summary>
    public class PlaneModel
    {
        #region Параметры (свойства) общего уравнения плоскости зеркала топлива
        /// <summary>
        /// Коэффициент оси Ox
        /// </summary>
        public float A { get; }

        /// <summary>
        /// Коэффициент оси Oy
        /// </summary>
        public float B { get; }

        /// <summary>
        /// Коэффициент оси Oz
        /// </summary>
        public float C { get; }

        /// <summary>
        /// Смещение относительно центра координат (свободный член)
        /// </summary>
        public float D { get; }

        /// <summary>
        /// Свойство вектора нормали к плоскости поверхности топлива
        /// </summary>
        public Vector3D Normale => new(A, B, C);

        /// <summary>
        /// Массив из трех точек, параметрически определяющий плоскость
        /// Параметрическая плоскость
        /// </summary>
        public Vector3D[] ParametricPlane = new Vector3D[3];
        #endregion

        /// <summary>
        /// Конструктор плоскости
        /// </summary>
        /// <param name="a">параметр A</param>
        /// <param name="b">параметр B</param>
        /// <param name="c">параметр C</param>
        /// <param name="d">параметр D</param>
        public PlaneModel(float a, float b, float c, float d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            // Вычисляем вектор нормали
            float normalVector = (float)Math.Sqrt(A * A + B * B + C * C);
            // Нормируем коэффициенты по нормальному вектору
            A /= normalVector;
            B /= normalVector;
            C /= normalVector;
            D /= normalVector;
        }

        /// <summary>
        /// Конструктор (перегруженный) задания плоскости параметрическим методом
        /// </summary>
        /// <param name="p0">Точка 1</param>
        /// <param name="p1">Точка 2</param>
        /// <param name="p2">Точка 3</param>
        public PlaneModel(Vector3D p0, Vector3D p1, Vector3D p2)
        {
            ParametricPlane[0] = p0;
            ParametricPlane[1] = p1;
            ParametricPlane[2] = p2;

            // Формируем параметры общего уравнения плоскости по параметрическим данным
            A = (p1.Y - p0.Y) * (p2.Z - p0.Z) - (p2.Y - p0.Y) * (p1.Z - p0.Z);
            B = (p2.X - p0.X) * (p1.Z - p0.Z) - (p1.X - p0.X) * (p2.Z - p0.Z);
            C = (p1.X - p0.X) * (p2.Y - p0.Y) - (p2.X - p0.X) * (p1.Y - p0.Y);
            D = -A * p0.X - B * p0.Y - C * p0.Z;

            // Вычисляем нормальный вектор плоскости
            float nrm = (float)Math.Sqrt(A * A + B * B + C * C);

            // Производим нормирование параметров
            A /= nrm;
            B /= nrm;
            C /= nrm;
            D /= nrm;
        }

        /// <summary>
        /// Метод вычисления расстояния от полученной вершины до плоскости зеркала топлива
        /// Я не понимаю этот метод - загвоздка в делении на величину нормального вектора
        /// Сам вектор формируется через A, B, C, а реально он строится на основании X, Y, Z
        /// Как так?
        /// </summary>
        /// <param name="p">Координата точки</param>
        /// <returns>Расстояние от точки до плоскости</returns>
        public float CountDistance(Vector3D p)
        {
            return ((A * p.X) + (B * p.Y) + (C * p.Z) + D) / Normale.Length();
        }

        /// <summary>
        /// Метод вычисления свободного члена (по сути - это смещение D)
        /// </summary>
        /// <param name="vertex">Точка (вершина), для которой проводится расчет</param>
        /// <returns>Свободный член (смещение)</returns>
        public float GetFreeMember(Vector3D vertex)
        {
            // Используется общее уравнение плоскости
            // Ax + By + Cz = -D
            // Параметры A, B и C определяются из плоскости зеркала топлива
            return -A * vertex.X - B * vertex.Y - C * vertex.Z;
        }

        /// <summary>
        /// Метод проверки векторов на коллинеарность (соосность)
        /// </summary>
        /// <param name="a">Вектор a</param>
        /// <param name="b">Вектор b</param>
        /// <returns>true, если коллинеарны</returns>
        public static bool IsCollinear(Vector3D a, Vector3D b)
        {
            // Получаем векторное произведение векторов
            // Если результатом является нулевой вектор, то вектора коллинеарны
            // ВНИМАНИЕ! Данное условие применимо только к трехмерной ситуации
            Vector3D c = Vector3D.Cross(a, b);
            //Vector3D c = a.CrossProduct(b); // Это для Euclidian.Spatial
            //Vector3D c = Vector3D.CrossProduct(a, b); // Это для Media3D
            // Далее - проверки, если хотя бы одно из значений X, Y или Z ненулевое, то вектора неколлинеарны!!!
            if (Math.Abs(c.X) > float.Epsilon) return false;
            if (Math.Abs(c.Y) > float.Epsilon) return false;
            return Math.Abs(c.Z) <= float.Epsilon;
        }

        /// <summary>
        /// Метод вычисления точки на отрезке пересекающей его плоскостью
        /// </summary>
        /// <param name="lineSegment">Сегмент (сторона) треугольника</param>
        /// <returns>Точка пересечения сегмента с плоскостью</returns>
        public Vector3D IntersectWithLineSegment(LineSegment lineSegment)
        {
            // По большому счету - пока не понимаю механизм данного метода
            Vector3D x1 = lineSegment.EndOfSegment;
            Vector3D x2 = lineSegment.BeginOfSegment;

            // Вычисляем определитель
            double denominator = A * (x2.X - x1.X) + B * (x2.Y - x1.Y) + C * (x2.Z - x1.Z);

            double lambda = -(A * x1.X + B * x1.Y + C * x1.Z + D) / denominator;

            return new Vector3D((float)(x1.X + lambda * (x2.X - x1.X)), (float)(x1.Y + lambda * (x2.Y - x1.Y)), (float)(x1.Z + lambda * (x2.Z - x1.Z)));

            //return lambda switch
            //{
            //    > 1 or < 0 => null,
            //    _ => new Vector3D((float)(x1.X + lambda * (x2.X - x1.X)), (float)(x1.Y + lambda * (x2.Y - x1.Y)), (float)(x1.Z + lambda * (x2.Z - x1.Z))),
            //};
        }
    }
}
