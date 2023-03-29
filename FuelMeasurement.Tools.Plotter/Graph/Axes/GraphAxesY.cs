using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FuelMeasurement.Tools.Plotter.Graph.Axes
{
    /// <summary>
    /// Класс оси Y
    /// </summary>
    public class GraphAxesY
    {
        #region Свойства класса оси ординат
        /// <summary>
        /// Экземпляр класса графика
        /// </summary>
        private CalibrationGraph Graph { get; }

        /// <summary>
        /// Кегль шрифта
        /// </summary>
        private double TextSize { get; } = 10;

        /// <summary>
        /// Семейство шрифтов
        /// </summary>
        public FontFamily FontType { get; set; } = new FontFamily("Verdana");

        /// <summary>
        /// Текст описания (назначения) оси Y
        /// </summary>
        private string DescriptionAxisY { get; set; }
        #endregion

        #region Методы класса оси ординат
        /// <summary>
        /// Конструктор оси ординат
        /// </summary>
        /// <param name="_graph">График</param>
        public GraphAxesY(CalibrationGraph _graph)
        {
            Graph = _graph;
        }

        /// <summary>
        /// Метод отрисовки оси ординат
        /// </summary>
        /// <param name="_zOrder"></param>
        public void Plot(int _zOrder/*, int horizontals*/)
        {
            DescriptionAxisY = Graph.NameAxisY;
            AddLinesToAxisY(_zOrder);
            AddTextToAxisY(_zOrder/*, horizontals*/);
        }

        /// <summary>
        /// Метод добавления текста для оси ординат
        /// </summary>
        /// <param name="zOrder"></param>
        /// <param name="horizontals"></param>
        private void AddTextToAxisY(int zOrder/*, int horizontals*/)
        {
            TextBlock axisName = new TextBlock
            {
                Text = "Ось\n Oy",
                Foreground = Brushes.Black,
                FontFamily = FontType,
                FontSize = TextSize,
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(Graph.Left - 50, Graph.Top * 0.1, 0, 0)
            };
            Panel.SetZIndex(axisName, zOrder);
            Graph.Canvas.Children.Add(axisName);
            TextBlock axisDescription = new TextBlock
            {
                Text = DescriptionAxisY,
                Foreground = Brushes.Black,
                FontFamily = FontType,
                FontSize = TextSize,
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(Graph.Left + 20, Graph.Top * 0.1, 0, 0)
            };
            Panel.SetZIndex(axisDescription, zOrder);
            Graph.Canvas.Children.Add(axisDescription);
        }

        /// <summary>
        /// Метод добавления линий оси ординат
        /// </summary>
        /// <param name="zOrder"></param>
        private void AddLinesToAxisY(int zOrder)
        {
            Line ordinate = new Line
            {
                Y1 = Graph.Top * 0.17,                              // Левая вертикаль
                Y2 = Graph.Canvas.ActualHeight - Graph.Bottom + 22, // Верхняя позиция оси ординат
                Stroke = Brushes.MediumBlue,                        // Цвет ординаты
                StrokeThickness = 1.2                               // Толщина ординаты
            };
            ordinate.X1 = ordinate.X2 = Graph.Left; // Сдвиг оси ординат относительно холста
            Panel.SetZIndex(ordinate, zOrder);
            Graph.Canvas.Children.Add(ordinate);
            // Рисуем стрелки
            Line arrowLeft = new Line
            {
                X1 = Graph.Left,
                X2 = Graph.Left - Graph.Top * 0.05,
                Y2 = Graph.Top * 0.25,
                Y1 = ordinate.Y1,
                Stroke = ordinate.Stroke,
                StrokeThickness = ordinate.StrokeThickness
            };
            Panel.SetZIndex(arrowLeft, zOrder);
            Graph.Canvas.Children.Add(arrowLeft);
            Line arrowRight = new Line
            {
                X1 = Graph.Left,
                X2 = Graph.Left + Graph.Top * 0.05,
                Y2 = Graph.Top * 0.25,
                Y1 = ordinate.Y1,
                Stroke = ordinate.Stroke,
                StrokeThickness = ordinate.StrokeThickness
            };
            Panel.SetZIndex(arrowRight, zOrder);
            Graph.Canvas.Children.Add(arrowRight);
        }
        #endregion

        /// <summary>
        /// Метод отрисовки оси ординат
        /// </summary>
        /// <param name="zOrder"></param>
        /// <param name="horizontals"></param>
        public void PlotErr(int zOrder, int horizontals, double scale)
        {
            DescriptionAxisY = Graph.NameAxisY;
            AddLinesToAxisY(zOrder);
            AddErrTextToAxisY(zOrder, horizontals, scale);
        }

        /// <summary>
        /// Метод добавления текста для оси ординат
        /// </summary>
        /// <param name="zOrder"></param>
        /// <param name="horizontals"></param>
        private void AddErrTextToAxisY(int zOrder, int horizontals, double scale)
        {
            TextBlock axisName = new TextBlock
            {
                Text = "Ось\n Oy",
                Foreground = Brushes.Black,
                FontFamily = FontType,
                FontSize = TextSize,
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(Graph.Left - 50, Graph.Top * 0.1, 0, 0)
            };
            Panel.SetZIndex(axisName, zOrder);
            Graph.Canvas.Children.Add(axisName);
            TextBlock axisDescription = new TextBlock
            {
                Text = DescriptionAxisY,
                Foreground = Brushes.Black,
                FontFamily = FontType,
                FontSize = TextSize,
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(Graph.Left + 20, Graph.Top * 0.1, 0, 0)
            };
            Panel.SetZIndex(axisDescription, zOrder);
            Graph.Canvas.Children.Add(axisDescription);

            decimal measure = decimal.Round((decimal)scale, 3);
            TextBlock tb1 = new TextBlock() // Верхняя линия графика - показания датчика
            {
                Text = $"{measure}",
                Foreground = Brushes.Black,
                FontFamily = FontType,
                FontSize = TextSize,
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(10, Graph.Top - TextSize / 2, 0, 0)
            };
            Panel.SetZIndex(tb1, zOrder);
            Graph.Canvas.Children.Add(tb1);
            measure = decimal.Round((decimal)scale, 3);
            TextBlock tb2 = new TextBlock() // Нижняя линия графика - показания датчика
            {
                Text = $"{-measure}",
                Foreground = Brushes.Black,
                FontFamily = FontType,
                FontSize = TextSize,
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(10, Graph.Canvas.ActualHeight - Graph.Bottom - TextSize / 2, 0, 0)
            };
            Panel.SetZIndex(tb2, zOrder);
            Graph.Canvas.Children.Add(tb2);
            double slice = (Graph.Canvas.ActualHeight - Graph.Top - Graph.Bottom) / (horizontals - 1);
            for (int i = 1; i < horizontals - 1; i++)
            {
                measure = decimal.Round((decimal)(scale - (2 * scale) / (horizontals - 1) * i), 3);
                TextBlock tb = new TextBlock()
                {
                    Text = $"{measure}",
                    Foreground = Brushes.Black,
                    FontFamily = FontType,
                    FontSize = TextSize,
                    FontWeight = FontWeights.Normal,
                    Margin = new Thickness(10, i * slice + Graph.Top - TextSize / 2, 0, 0)
                };
                Panel.SetZIndex(tb, zOrder);
                Graph.Canvas.Children.Add(tb);
            }
        }
    }
}
