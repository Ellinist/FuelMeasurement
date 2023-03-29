using Assimp;
using System;

namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Класс пирамиды (предназначен для создания многогранников)
    /// </summary>
    public class Pyramid
    {
        /// <summary>
        /// Размерность пирамиды
        /// </summary>
        private const int pyramidVertices = 4;

        /// <summary>
        /// Треугольник - базис пирамиды (основание пирамиды)
        /// </summary>
        public Triangle BaseOfPyramid { get; set; }

        /// <summary>
        /// Точка - фокус пирамиды
        /// </summary>
        public Vector3D FocusOfPyramid { get; set; }

        /// <summary>
        /// Массив вершин пирамиды (у пирамиды четыре вершины - у нас именно такая пирамида)
        /// Но! Проверить возможные варианты - иначе можно сразу определять размер
        /// </summary>
        public Vector3D[] BaseVertices { get; set; }

        /// <summary>
        /// Высота нормали от плоскости основания треугольника до фокусной точки пирамиды
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Знак смачиваемой поверхности основания пирамиды
        /// Если знак равен 1, то поверхность смачиваемая, если 0 - то сухая
        /// </summary>
        public int Sign { get; set; }

        /// <summary>
        /// Конструктор пирамиды с вычислением знака смачиваемости
        /// </summary>
        /// <param name="baseOfPyramid">базовый треугольник (основание пирамиды)</param>
        /// <param name="focusOfPyramid">фокус пирамиды</param>
        public Pyramid(Triangle baseOfPyramid, Vector3D focusOfPyramid)
        {
            BaseOfPyramid = baseOfPyramid; // Задаем основание пирамиды (это треугольник)
            FocusOfPyramid = focusOfPyramid; // Задаем фокус пирамиды (это вершина пирамиды)

            // Создаем пирамиду из четырех вершин (три - в основании и одна - фокус (вершина пирамиды))
            BaseVertices = new Vector3D[pyramidVertices]
            {
                BaseOfPyramid.Vertices[0], BaseOfPyramid.Vertices[1], BaseOfPyramid.Vertices[2], FocusOfPyramid
            };

            // При создании пирамиды вычисляем расстояние от ее фокуса до плоскости основания треугольника
            SetHeight();

            // Вычисляем знак смачиваемости
            // Положительный знак - поверхность смочена - объем считаем
            // Отрицательный знак - поверхность сухая - объем считаем, но вычитаем.
            SetSign();
        }

        /// <summary>
        /// Метод установки знака учета посчитанного объема пирамиды
        /// Это метод определения смоченный ли рассматриваемый треугольник
        /// Если знак -1, то треугольник сухой
        /// Если знак +1, то треугольник смоченный
        /// Если знак равен 0, то фокус пирамиды лежит в одной с треугольником плоскости
        /// Этого не должно произойти - отсекается на взлете!
        /// </summary>
        private void SetSign()
        {
            // Создаем вектор - от нулевой вершины треугольника до фокуса считаемой пирамиды
            var focusVector = BaseOfPyramid.Vertices[0] - FocusOfPyramid;

            // Построим вектор нормали к плоскости треугольника, направленный внутрь бака
            // Сначала построим два вспомогательных вектора - в плоскости треугольника
            // Это вектора от точки фокуса до 2-й вершины (при почасовом обходе изнутри бака) и от точки фокуса до первой вершины
            var firstVector = BaseOfPyramid.Vertices[0] - BaseOfPyramid.Vertices[2];
            var secondVector = BaseOfPyramid.Vertices[0] - BaseOfPyramid.Vertices[1];
            // Произведение (геометрическое - Внимание! Не скалярное!) первого вектора на второй дает вектор нормали, направленный внутрь бака
            var innerNormal = Vector3D.Cross(firstVector, secondVector);

            // Вычисляем знак
            Sign = -Math.Sign(focusVector.X * innerNormal.X +
                              focusVector.Y * innerNormal.Y +
                              focusVector.Z * innerNormal.Z);
        }

        /// <summary>
        /// Метод вычисления объема пирамиды
        /// </summary>
        /// <returns>Объем пирамиды</returns>
        public float Volume() => (BaseOfPyramid.Square * Height) / 3;

        /// <summary>
        /// Метод вычисления высоты нормали от плоскости до фокуса пирамиды
        /// </summary>
        private void SetHeight()
        {
            // Создание плоскости зеркала топлива для выбранного треугольника по его основанию
            var basePlane = new PlaneModel(BaseOfPyramid.Vertices[0], BaseOfPyramid.Vertices[1], BaseOfPyramid.Vertices[2]);

            // Вычисление расстояния от фокуса пирамиды до плоскости основания выбранного треугольника
            Height = Math.Abs(basePlane.CountDistance(FocusOfPyramid));
        }
    }
}
