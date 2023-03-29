using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FuelMeasurement.Tools.Plotter.Graph.Plot
{
    public class GraphCurve
    {
        /// <summary>
        /// Экземпляр класса графика
        /// </summary>
        private CalibrationGraph Graph { get; }

        /// <summary>
        /// Точки оси абсцисс
        /// </summary>
        public List<double> ArrayX { get; set; }

        /// <summary>
        /// Точки оси ординат
        /// </summary>
        public List<double> ArrayY { get; set; }

        /// <summary>
        /// Название кривой (топливного бака)
        /// Для суммарной кривой - название суммарной кривой
        /// Для отдельных кривых - название топливного бака
        /// </summary>
        public string CurveName { get; }

        /// <summary>
        /// Номер кривой
        /// </summary>
        private int CurveNumber { get; }

        /// <summary>
        /// Максимальное значение правой стороны кривой
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// Максимальное значение верхней стороны кривой
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// Максимальное значение нижней стороны кривой
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// Цвет кривой
        /// </summary>
        public Color CurveColor { get; set; } = Colors.White;

        /// <summary>
        /// Толщина линии
        /// </summary>
        public double LineThickness { get; set; } = 1.8;

        /// <summary>
        /// Отображаются ли окружности в точках перегиба
        /// </summary>
        public bool IsPointVisible { get; set; } = true;

        /// <summary>
        /// Диаметр точки кривой
        /// </summary>
        public double PointDiameter { get; set; } = 6;

        /// <summary>
        /// Толщина контура точки кривой
        /// </summary>
        public double PointThickness { get; set; } = 1.1;

        /// <summary>
        /// Цвет заполнения точки кривой
        /// </summary>
        public Brush PointFillColor { get; set; } = Brushes.Yellow;

        /// <summary>
        /// Цвет контура эллипса
        /// </summary>
        public Brush EllipseContour { get; set; } = Brushes.Black;

        /// <summary>
        /// Кегль шрифта
        /// </summary>
        public double TextSize { get; set; } = 11;

        /// <summary>
        /// Семейство шрифтов
        /// </summary>
        public FontFamily FontType { get; set; } = new FontFamily("Verdana"); // Шрифт, запрещенный ГОСТом

        /// <summary>
        /// Относительный сдвиг положения объекта в пределах графика
        /// </summary>
        public double ElementShift { get; set; }

        /// <summary>
        /// Конструктор кривой
        /// </summary>
        /// <param name="graph">График</param>
        /// <param name="arrayX">Массив точек оси абсцисс</param>
        /// <param name="arrayY">Массив точек оси ординат</param>
        /// <param name="curveName">Название кривой</param>
        /// <param name="curveNumber"></param>
        /// <param name="curveColor"></param>
        public GraphCurve(CalibrationGraph graph, List<double> arrayX, List<double> arrayY, string curveName, int curveNumber, Color curveColor)
        {
            Graph = graph;
            ArrayX = arrayX;
            ArrayY = arrayY;
            CurveName = curveName;
            CurveNumber = curveNumber;
            CurveColor = curveColor;
        }

        /// <summary>
        /// Метод установки масштабов графика по созданным в нем кривым
        /// </summary>
        public void SetScales()
        {
            MaxX = ArrayX.Max();
            MaxY = ArrayY.Max();
            MinY = ArrayY.Min();
            if (Graph.CurvesList.Count == 1)
            {
                // Для первой кривой графика устанавливаем минимальные и максимальные значения
                Graph.MinCurveX = ArrayX.Min();
                Graph.MaxCurveX = ArrayX.Max();
                Graph.MinCurveY = ArrayY.Min();
                Graph.MaxCurveY = ArrayY.Max();
            }
            else
            {
                SetMinMaxValues(); // Для остальных кривых вычисляем минимальные и максимальные значения
            }

            // Высчитываем коэффициенты масштабирования кривых на канве графика
            if (Graph.MaxCurveX - Graph.MinCurveX > double.Epsilon)
            {
                Graph.Kx = (Graph.Canvas.ActualWidth - (Graph.Left + Graph.Right)) / (Graph.MaxCurveX - Graph.MinCurveX);
            }

            if (Graph.MaxCurveY - Graph.MinCurveY > double.Epsilon)
            {
                Graph.Ky = (Graph.Canvas.ActualHeight - (Graph.Top + Graph.Bottom)) / (Graph.MaxCurveY - Graph.MinCurveY);
            }
        }

        /// <summary>
        /// Метод вычисления минимальных и максимальных значений кривых графика
        /// </summary>
        private void SetMinMaxValues()
        {
            // Для остальных кривых вычисляем общие минимальные и максимальные значения
            if (Graph.MinCurveX > ArrayX.Min()) Graph.MinCurveX = ArrayX.Min();
            if (Graph.MaxCurveX < ArrayX.Max()) Graph.MaxCurveX = ArrayX.Max();
            if (Graph.MinCurveY > ArrayY.Min()) Graph.MinCurveY = ArrayY.Min();
            if (Graph.MaxCurveY < ArrayY.Max()) Graph.MaxCurveY = ArrayY.Max();
        }

        /// <summary>
        /// Метод отображения кривой
        /// </summary>
        /// <param name="curveNumber">Номер тарировочной кривой</param>
        /// <param name="zOrder">Z-индекс отрисовки кривой</param>
        /// <param name="tankColor">Цвет кривой</param>
        public void PlotCurve(int zOrder, Color tankColor)
        {
            // Тарировочная кривая строится полилинией - потом перейти на линию безье
            Polyline poly = new Polyline
            {
                Points = new PointCollection(), // Задаем коллекцию точек
                //Stroke = CurveColor == Colors.White ? GraphLineColor.GetColor(curveNumber) : CurveColor, // Получаем цвет кривой
                Stroke = new SolidColorBrush(tankColor),
                StrokeThickness = LineThickness, // Толщина линии
                UseLayoutRounding = true // Фиг знает, что это за параметр
            };
            for (int i = 0; i < ArrayX.Count; i++) // В цикле строим полилинию
            {
                poly.Points.Add(new Point((ArrayX[i] * Graph.Kx) + Graph.Left,
                  Graph.Canvas.ActualHeight - (ArrayY[i] * Graph.ScaleY - Graph.MinCurveY) * Graph.Ky - Graph.Bottom));
            }

            Panel.SetZIndex(poly, zOrder);
            Graph.Canvas.Children.Add(poly); // Полилинию кидаем на канву
            for (int i = 0; i < ArrayX.Count; i++) // Пробегаем в цикле по всем точкам (отрисовываем окружности)
            {
                double X = ArrayX[i] * Graph.Kx + Graph.Left;
                double Y = Graph.Canvas.ActualHeight - (ArrayY[i] * Graph.ScaleY - Graph.MinCurveY) * Graph.Ky - Graph.Bottom;
                if (IsPointVisible)
                {
                    Ellipse ellipse = new Ellipse
                    {
                        Width = PointDiameter,
                        Height = PointDiameter,
                        Stroke = EllipseContour,
                        StrokeThickness = PointThickness,
                        Fill = PointFillColor,
                        Margin = new Thickness(X - PointDiameter / 2, Y - PointDiameter / 2, 0, 0)
                    };
                    Panel.SetZIndex(ellipse, zOrder);
                    Graph.Canvas.Children.Add(ellipse);
                }

                if (i != ArrayX.Count - 1) continue;
                TextBlock tb = new TextBlock
                {
                    Text = CurveName,
                    Foreground = Brushes.Black,
                    FontFamily = FontType,
                    FontSize = TextSize,
                    FontWeight = FontWeights.Normal,
                    Margin = new Thickness(X - PointDiameter / 2, Y - PointDiameter * 2, 0, 0)
                };
                Panel.SetZIndex(tb, zOrder);
                Graph.Canvas.Children.Add(tb);
            }
        }
    }
}
