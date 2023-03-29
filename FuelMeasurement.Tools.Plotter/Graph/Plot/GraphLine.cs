using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FuelMeasurement.Tools.Plotter.Graph.Plot
{
    /// <summary>
    /// Класс линии на графике
    /// </summary>
    public class GraphLine
    {
        /// <summary>
        /// График, в котором отрисовывается линия
        /// </summary>
        public CalibrationGraph Graph { get; set; }

        /// <summary>
        /// Кривая, к которой принадлежит линия
        /// </summary>
        public GraphCurve Curve { get; set; }

        /// <summary>
        /// Цвет линии
        /// </summary>
        public Brush LineColor { get; set; }

        /// <summary>
        /// Толщина линии
        /// </summary>
        public double LineWeight { get; set; }

        /// <summary>
        /// Стартовая координата X
        /// </summary>
        public double StartX { get; set; }

        /// <summary>
        /// Конечная координата X
        /// </summary>
        public double StopX { get; set; }

        /// <summary>
        /// Стартовая координата Y
        /// </summary>
        public double StartY { get; set; }

        /// <summary>
        /// Конечная координата Y
        /// </summary>
        public double StopY { get; set; }

        /// <summary>
        /// Конструктор линии графика
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="curve"></param>
        public GraphLine(CalibrationGraph graph, GraphCurve curve)
        {
            Graph = graph;
            Curve = curve;
        }

        /// <summary>
        /// Метод отрисовки линии
        /// </summary>
        public void PlotLine(int zOrder)
        {
            Line line = new()
            {
                X1 = StartX,
                X2 = StopX,
                Y1 = StartY,
                Y2 = StopY,
                Stroke = LineColor,
                StrokeThickness = LineWeight
            };
            Panel.SetZIndex(line, zOrder);
            Graph.Canvas.Children.Add(line);
        }
    }
}
