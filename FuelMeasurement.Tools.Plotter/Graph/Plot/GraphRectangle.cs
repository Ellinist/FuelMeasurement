using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FuelMeasurement.Tools.Plotter.Graph.Plot
{
    /// <summary>
    /// Класс объекта - прямоугольника
    /// </summary>
    public class GraphRectangle
    {
        /// <summary>
        /// График, на котором отрисовывается прямоугольник
        /// </summary>
        public CalibrationGraph Graph { get; set; }

        /// <summary>
        /// Кривая, к которой принадлежит прямоугольник
        /// </summary>
        public GraphCurve Curve { get; set; }

        /// <summary>
        /// Координата X левого верхнего угла
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Координата Y левого верхнего угла
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Высота прямоугольника
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Ширина прямоугольника
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Толщина 
        /// </summary>
        public double BorderWidth { get; set; }

        /// <summary>
        /// Цвет рамки прямоугольника
        /// </summary>
        public Brush BorderColor { get; set; }

        /// <summary>
        /// Цвет фона прямоугольника
        /// </summary>
        public Brush BackgroundColor { get; set; }

        /// <summary>
        /// Непрозрачность прямоугольника
        /// </summary>
        public double RectangleOpacity { get; set; }

        /// <summary>
        /// Конструктор прямоугольника
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="curve"></param>
        public GraphRectangle(CalibrationGraph graph, GraphCurve curve)
        {
            Graph = graph;
            Curve = curve;
        }

        /// <summary>
        /// Метод отрисовки прямоугольника
        /// </summary>
        public void PlotRectangle(int zOrder)
        {
            if (Graph.Canvas.ActualHeight == 0 || Graph.Canvas.ActualWidth == 0) return;
            // Создаем контур прямоугольника
            Rectangle contour = new()
            {
                Margin = new Thickness(X, Y, 0, 0),
                Stroke = BorderColor,
                StrokeThickness = BorderWidth,
                Height = Height,
                Width = Width
            };

            // Создаем заливку прямоугольника
            Rectangle rect = new()
            {
                Margin = new Thickness(X, Y, 0, 0),
                Fill = BackgroundColor,
                Opacity = RectangleOpacity,
                Height = Height,
                Width = Width,
                //Name = name
            };
            Panel.SetZIndex(rect, zOrder);
            Panel.SetZIndex(contour, zOrder);
            Graph.Canvas.Children.Add(rect);
            Graph.Canvas.Children.Add(contour);
        }
    }
}
