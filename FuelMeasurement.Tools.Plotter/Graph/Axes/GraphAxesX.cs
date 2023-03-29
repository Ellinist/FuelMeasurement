using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FuelMeasurement.Tools.Plotter.Graph.Axes
{
    /// <summary>
    /// Класс оси X
    /// </summary>
    public class GraphAxesX
    {
        #region Свойства класса оси абсцисс
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
        private FontFamily FontType { get; set; } = new("Verdana");

        /// <summary>
        /// Текст описания (назначения) оси X
        /// </summary>
        private string DescriptionAxisX { get; set; }

        /// <summary>
        /// Сдвиг оси X при изменении значения нуля (по оси Y) графика
        /// </summary>
        private double VerticalShift { get; set; }
        #endregion

        #region Методы класса оси абсцисс
        /// <summary>
        /// Конструктор оси абсцисс
        /// </summary>
        /// <param name="graph">График</param>
        public GraphAxesX(CalibrationGraph graph)
        {
            Graph = graph;
        }

        /// <summary>
        /// Метод отрисовки оси абсцисс
        /// </summary>
        /// <param name="zOrder"></param>
        public void Plot(int zOrder)
        {
            // Коэффициент сдвига оси абсцисс с тем, чтобы она всегда была на нуле (ну, почти всегда)
            double k = (Graph.MinCurveY < 0) ? (-Graph.MinCurveY / (Graph.MaxCurveY - Graph.MinCurveY)) : 0;
            if (Graph.MaxCurveY < 0) k = 1;
            VerticalShift = (Graph.Canvas.ActualHeight - Graph.Bottom - Graph.Top) * k;
            DescriptionAxisX = Graph.NameAxisX;
            AddTextToAxisX(zOrder);
            AddLinesToAxisX(zOrder);
        }

        /// <summary>
        /// Метод добавления текста для оси абсцисс
        /// </summary>
        /// <param name="zOrder"></param>
        private void AddTextToAxisX(int zOrder)
        {
            #region Текст-заголовок оси X
            TextBlock axisName = new()
            {
                Text = "Ось\n Ox",
                Foreground = Brushes.Black,
                FontFamily = FontType,
                FontSize = TextSize,
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(Graph.Canvas.ActualWidth - Graph.Right * 0.65, Graph.Canvas.ActualHeight - Graph.Bottom * 0.8 - VerticalShift, 0, 0)
            };
            Panel.SetZIndex(axisName, zOrder);
            Graph.Canvas.Children.Add(axisName);
            #endregion

            #region Текст, описывающий назначение оси X
            TextBlock axisDescription = new()
            {
                Text = DescriptionAxisX,
                Foreground = Brushes.Black,
                FontFamily = FontType,
                FontSize = TextSize,
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(Graph.Canvas.ActualWidth - Graph.Right * 0.9, Graph.Canvas.ActualHeight - Graph.Bottom * 1.5 - VerticalShift, 0, 0)
            };
            Panel.SetZIndex(axisDescription, zOrder);
            Graph.Canvas.Children.Add(axisDescription);
            #endregion
        }

        /// <summary>
        /// Метод добавления линий для оси X
        /// </summary>
        /// <param name="zOrder"></param>
        private void AddLinesToAxisX(int zOrder)
        {
            Line abscissa = new()
            {
                X1 = Graph.Left - 22,
                X2 = Graph.Canvas.ActualWidth - Graph.Right * 0.17,
                Stroke = Brushes.MediumBlue,
                StrokeThickness = 1.2
            };
            // Горизонталь - будет жирная - обозначает ось абсцисс
            abscissa.Y1 = abscissa.Y2 = Graph.Canvas.ActualHeight - Graph.Bottom - VerticalShift;
            Panel.SetZIndex(abscissa, zOrder);
            Graph.Canvas.Children.Add(abscissa);
            // Рисуем стрелки
            Line arrowUp = new()
            {
                X1 = Graph.Canvas.ActualWidth - Graph.Right * 0.25,
                X2 = Graph.Canvas.ActualWidth - Graph.Right * 0.12,
                Y2 = Graph.Canvas.ActualHeight - Graph.Bottom - VerticalShift,
                Y1 = abscissa.Y1 - Graph.Right * 0.05,
                Stroke = abscissa.Stroke,
                StrokeThickness = abscissa.StrokeThickness
            };
            Panel.SetZIndex(arrowUp, zOrder);
            Graph.Canvas.Children.Add(arrowUp);
            Line arrowDown = new()
            {
                X1 = arrowUp.X1,
                X2 = arrowUp.X2,
                Y2 = Graph.Canvas.ActualHeight - Graph.Bottom - VerticalShift,
                Y1 = abscissa.Y1 + Graph.Right * 0.05,
                Stroke = abscissa.Stroke,
                StrokeThickness = abscissa.StrokeThickness
            };
            Panel.SetZIndex(arrowDown, zOrder);
            Graph.Canvas.Children.Add(arrowDown);
        }
        #endregion
    }
}
