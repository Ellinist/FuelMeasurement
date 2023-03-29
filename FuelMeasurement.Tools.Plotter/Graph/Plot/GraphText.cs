using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FuelMeasurement.Tools.Plotter.Graph.Plot
{
    /// <summary>
    /// Класс текста на графике
    /// </summary>
    public class GraphText
    {
        /// <summary>
        /// График, на котором отрисовывается текст
        /// </summary>
        public CalibrationGraph Graph { get; set; }

        /// <summary>
        /// Кривая, к которой относится текст
        /// </summary>
        public GraphCurve Curve { get; set; }

        /// <summary>
        /// Текст, который выводится на графике
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Цвет выводимого текста
        /// </summary>
        public Brush TextColor { get; set; }

        /// <summary>
        /// Фон, на котором отрисовывается текст
        /// </summary>
        public Brush Background { get; set; }

        /// <summary>
        /// Стиль шрифта
        /// </summary>
        public FontFamily FontFamily { get; set; }

        /// <summary>
        /// Кегль шрифта
        /// </summary>
        public double FontSize { get; set; }

        /// <summary>
        /// Вес (толщина) шрифта
        /// </summary>
        public FontWeight FontWeight { get; set; }

        /// <summary>
        /// Позиция X отрисовки текста
        /// </summary>
        public double PositionX { get; set; }

        /// <summary>
        /// Позиция Y отрисовки текста
        /// </summary>
        public double PositionY { get; set; }

        /// <summary>
        /// Конструктор текста, отрисовываемого на графике
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="curve"></param>
        public GraphText(CalibrationGraph graph, GraphCurve curve)
        {
            Graph = graph;
            Curve = curve;
        }

        /// <summary>
        /// Метод отрисовки текста
        /// </summary>
        public void PlotText(int zOrder)
        {
            TextBlock tb = new()
            {
                Text = Text,
                Foreground = TextColor,
                Background = Background,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontWeight = FontWeight,
                Margin = new Thickness(PositionX, PositionY, 0, 0)
            };
            Panel.SetZIndex(tb, zOrder);
            Graph.Canvas.Children.Add(tb);
        }
    }
}
