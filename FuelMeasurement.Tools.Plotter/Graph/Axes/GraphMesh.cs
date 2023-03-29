using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FuelMeasurement.Tools.Plotter.Graph.Axes
{
    /// <summary>
    /// Класс сетки графика
    /// </summary>
    public class GraphMesh
    {
        #region Свойства класса сетки графика
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
        public FontFamily FontType { get; set; } = new("Verdana");

        ///// <summary>
        ///// Число вертикалей сетки
        ///// </summary>
        //private int Verticals { get; set; }

        ///// <summary>
        ///// Число горизонталей сетки
        ///// </summary>
        //private int Horizontals { get; set; }

        /// <summary>
        /// Пунктирная линия
        /// </summary>
        private DoubleCollection DottedLine { get; } = new DoubleCollection(2) { 2, 6 };
        #endregion

        #region Методы класса сетки графика
        /// <summary>
        /// Конструктор сетки
        /// По умолчанию - по 10 линий вертикально и горизонтально
        /// </summary>
        public GraphMesh(CalibrationGraph _graph)
        {
            Graph = _graph;
        }

        /// <summary>
        /// Метод отображения сетки
        /// </summary>
        public void ShowMesh(int zOrder)
        {
            PlotVerticals(zOrder);   // Вызываем метод отрисовки вертикальных линий сетки
            PlotHorizontals(zOrder); // Вызываем метод отрисовки горизонтальных линий сетки
        }

        /// <summary>
        /// Метод отображения горизонталей
        /// </summary>
        private void PlotHorizontals(int zOrder)
        {
            // Вызываем метод получения шага приращения вертикальных линий и значения приращения
            var (stepCount, stepSlice, measureResult, firstStep) = GetCurrentHorizontalSlices();

            Line l1 = new Line(); // Верхняя горизонталь
            Line l2 = new Line(); // Нижняя горизонталь - будет жирная - обозначает ось абсцисс
            l1.X1 = l2.X1 = Graph.Left - 22;
            l2.X2 = l1.X2 = Graph.Canvas.ActualWidth - Graph.Right;
            l1.Y1 = l1.Y2 = Graph.Top;
            l2.Y1 = l2.Y2 = Graph.Canvas.ActualHeight - Graph.Bottom;
            l1.Stroke = l2.Stroke = Brushes.BlueViolet;
            Panel.SetZIndex(l1, zOrder);
            Graph.Canvas.Children.Add(l1);
            Panel.SetZIndex(l2, zOrder);
            Graph.Canvas.Children.Add(l2);

            decimal measure1 = decimal.Round((decimal)Graph.MaxCurveY, 1);
            TextBlock tb1 = new TextBlock() // Верхняя линия графика - показания датчика
            {
                Text = $"{measure1}",
                Foreground = Brushes.Black,
                FontFamily = FontType,
                FontSize = TextSize,
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(50, Graph.Top - 0.7 * TextSize, 0, 0)
            };
            Panel.SetZIndex(tb1, zOrder);
            Graph.Canvas.Children.Add(tb1);
            decimal measure2 = decimal.Round((decimal)Graph.MinCurveY, 1);
            TextBlock tb2 = new TextBlock() // Нижняя линия графика - показания датчика
            {
                Text = $"{measure2}",
                Foreground = Brushes.Black,
                FontFamily = FontType,
                FontSize = TextSize,
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(50, Graph.Canvas.ActualHeight - Graph.Bottom - 0.7 * TextSize, 0, 0)
            };
            Panel.SetZIndex(tb2, zOrder);
            Graph.Canvas.Children.Add(tb2);


            //double slice = (Graph.Canvas.ActualHeight - Graph.Top - Graph.Bottom) / (Horizontals - 1);
            var otn1 = Math.Abs(firstStep) / (Graph.MaxCurveY - Graph.MinCurveY);
            var otn2 = (Graph.Canvas.ActualHeight - Graph.Top - Graph.Bottom) * otn1;

            #region Вывод горизонтальных линий
            if (!Graph.Relative && firstStep > 0)
            {
                // Цикл отрисовки горизонтальных линий графика
                for (int i = 0; i <= stepCount; i++)
                {
                    Line h = new Line // v - от слова horizontal
                    {
                        StrokeDashArray = DottedLine,
                        X1 = Graph.Left - 22,
                        X2 = Graph.Canvas.ActualWidth - Graph.Right,
                        Stroke = Brushes.DimGray
                    };
                    h.Y1 = h.Y2 = Graph.Canvas.ActualHeight - Graph.Bottom - i * stepSlice - otn2;

                    if (h.Y1 < l1.Y1 || h.Y1 > l2.Y1) continue;

                    Panel.SetZIndex(h, zOrder);
                    Graph.Canvas.Children.Add(h);
                }
            }
            else
            {
                // Цикл отрисовки горизонтальных линий графика
                for (int i = 0; i <= stepCount + 1; i++)
                {
                    Line h = new Line // v - от слова horizontal
                    {
                        StrokeDashArray = DottedLine,
                        X1 = Graph.Left - 22,
                        X2 = Graph.Canvas.ActualWidth - Graph.Right,
                        Stroke = Brushes.DimGray
                    };
                    h.Y1 = h.Y2 = Graph.Canvas.ActualHeight - Graph.Bottom - (i - 1) * stepSlice - otn2;

                    if (h.Y1 < l1.Y1 || h.Y1 > l2.Y1) continue;

                    Panel.SetZIndex(h, zOrder);
                    Graph.Canvas.Children.Add(h);
                }
            }
            #endregion

            decimal measure;
            double t;
            #region Вывод текста для горизонтальных линий
            if (!Graph.Relative && firstStep > 0)
            {
                for (int i = 0; i <= stepCount; i++)
                {
                    measure = decimal.Round((decimal)(Graph.MinCurveY - firstStep + measureResult * i), 0);

                    if (measure > measure1/* || measure > measure2*/) continue;

                    t = i * stepSlice;
                    TextBlock tb = new TextBlock()
                    {
                        Text = $"{measure}",
                        Foreground = Brushes.Black,
                        FontFamily = FontType,
                        FontSize = TextSize,
                        FontWeight = FontWeights.Normal,
                        Margin = new Thickness(10, Graph.Canvas.ActualHeight - Graph.Bottom - t - otn2 - 0.7 * TextSize, 0, 0)
                    };
                    Panel.SetZIndex(tb, zOrder);
                    Graph.Canvas.Children.Add(tb);
                }
            }
            else
            {
                for (int i = 0; i <= stepCount; i++)
                {
                    measure = decimal.Round((decimal)(Graph.MinCurveY - firstStep + measureResult * i), 0);

                    if (measure > measure1 || measure < measure2) continue;

                    TextBlock tb = new TextBlock()
                    {
                        Text = $"{measure}",
                        Foreground = Brushes.Black,
                        FontFamily = FontType,
                        FontSize = TextSize,
                        FontWeight = FontWeights.Normal,
                        Margin = new Thickness(10, Graph.Canvas.ActualHeight - Graph.Bottom - i * stepSlice - otn2 - 0.7 * TextSize, 0, 0)
                    };
                    Panel.SetZIndex(tb, zOrder);
                    Graph.Canvas.Children.Add(tb);
                }
            }
            #endregion
        }

        /// <summary>
        /// Метод отображения вертикалей
        /// </summary>
        private void PlotVerticals(int zOrder)
        {
            // Вызываем метод получения шага приращения вертикальных линий и значения приращения
            var (stepCount, stepSlice, measureResult) = GetCurrentVerticalSlices();

            Line l1 = new(); // Левая вертикаль
            Line l2 = new(); // Правая вертикаль
            l1.X1 = l1.X2 = Graph.Left; // Сдвиг оси ординат относительно холста
            l1.Y1 = l2.Y1 = Graph.Top;  // Верхняя позиция вертикальной линии сетки
            l2.Y2 = l1.Y2 = Graph.Canvas.ActualHeight - Graph.Bottom + 22;
            l2.X1 = l2.X2 = Graph.Canvas.ActualWidth - Graph.Right;

            l1.Stroke = l2.Stroke = Brushes.DimGray;
            l1.StrokeDashArray = DottedLine;
            l1.Stroke = new SolidColorBrush(Colors.Red);
            l2.Stroke = new SolidColorBrush(Colors.DarkBlue);

            Panel.SetZIndex(l1, zOrder);
            Graph.Canvas.Children.Add(l1); // Выводим левую вертикальную линию
            Panel.SetZIndex(l2, zOrder);
            Graph.Canvas.Children.Add(l2); // Выводим правую вертикальную линию

            #region Блок вывода вертикальных линий с округленными значениями
            for (int i = 1; i <= stepCount; i++)
            {
                Line v = new()
                {
                    Y1 = Graph.Top,
                    StrokeDashArray = DottedLine,
                    Y2 = Graph.Canvas.ActualHeight - Graph.Bottom + 22,
                    Stroke = Brushes.DimGray
                };
                v.X1 = v.X2 = (i * stepSlice) + Graph.Left;
                Panel.SetZIndex(v, zOrder);
                Graph.Canvas.Children.Add(v);
            }
            #endregion

            #region Блок вывода текста (значения) левой вертикальной линии
            decimal measure = decimal.Round((decimal)Graph.MinCurveX, 0);
            TextBlock tb2 = new()
            {
                Text = $"{measure}",
                Foreground = Brushes.Black,
                FontFamily = FontType,
                FontSize = TextSize,
                FontWeight = FontWeights.Normal,
                RenderTransform = new RotateTransform(-90, 0, 0),
                Margin = new Thickness(Graph.Left - TextSize / 2, Graph.Canvas.ActualHeight - 10, 0, 0)
            };
            Panel.SetZIndex(tb2, zOrder);
            Graph.Canvas.Children.Add(tb2);
            #endregion

            #region Текст значений оси X для правой вертикальной линии (реальное показание объема)
            measure = decimal.Round((decimal)Graph.MaxCurveX, 1);
            TextBlock tb1 = new()
            {
                Text = $"{measure}",
                Foreground = Brushes.Black,
                FontFamily = FontType,
                FontSize = TextSize,
                FontWeight = FontWeights.Bold,
                RenderTransform = new RotateTransform(-90, 0, 0),
                Margin = new Thickness(Graph.Canvas.ActualWidth - Graph.Right - TextSize / 2, Graph.Canvas.ActualHeight - 40, 0, 0)
            };
            Panel.SetZIndex(tb1, zOrder);
            Graph.Canvas.Children.Add(tb1);
            #endregion

            #region Текст значений оси X для промежуточных вертикальных линий
            for (int i = 1; i <= stepCount; i++)
            {
                measure = decimal.Round((decimal)(measureResult * i), 0);
                TextBlock tb = new()
                {
                    Text = $"{measure}",
                    Foreground = Brushes.Black,
                    FontFamily = FontType,
                    FontSize = TextSize,
                    FontWeight = FontWeights.Normal,
                    RenderTransform = new RotateTransform(-90, 0, 0),
                    Margin = new Thickness(i * stepSlice + Graph.Left - TextSize / 2, Graph.Canvas.ActualHeight - 10, 0, 0)
                };
                Panel.SetZIndex(tb, zOrder);
                Graph.Canvas.Children.Add(tb);
            }
            #endregion
        }

        /// <summary>
        /// Метод, возвращающий число вертикальных линий, шаг сдвига линий и шаг объема
        /// </summary>
        /// <returns>Кортеж из трех параметров</returns>
        private (int, double, double) GetCurrentVerticalSlices()
        {
            #region Вычисляемые переменные
            int slicesNo = 0; // Количество вертикальных линий
            double sliceResult = 0; // Шаг сдвига вертикальных линий
            double measureResult = 0; // Шаг значения объема
            #endregion

            // Считаем реальный размер , выделенный под график
            var realCanvas = Graph.Canvas.ActualWidth - Graph.Left - Graph.Right;

            // Вспомогательные переменные
            double multiple;
            int wholeMultiple;
            double stepSlices;

            switch (Graph.MaxCurveX)
            {
                case >= 10f and < 100f: // Объем бака лежит в пределах 10-100 литров
                    multiple = Graph.MaxCurveX / 1; // Число для деления
                    wholeMultiple = (int)(multiple / 10);      // Целочисленное количество

                    sliceResult = wholeMultiple * realCanvas / multiple;

                    measureResult = Graph.MaxCurveX * wholeMultiple / multiple;
                    stepSlices = realCanvas / 10 * (10 * wholeMultiple / multiple);

                    slicesNo = (int)(realCanvas / stepSlices);
                    break;
                case >= 100f and < 1000f: // Объем бака лежит в пределах 100-1_000 литров
                    multiple = Graph.MaxCurveX / 10; // Число для деления
                    wholeMultiple = (int)(multiple / 10);      // Целочисленное количество

                    sliceResult = wholeMultiple * realCanvas / multiple;

                    measureResult = Graph.MaxCurveX * wholeMultiple / multiple;
                    stepSlices = realCanvas / 10 * (10 * wholeMultiple / multiple);

                    slicesNo = (int)(realCanvas / stepSlices);
                    break;
                case >= 1_000f and < 10_000f: // Объем бака лежит в пределах 1_000-10_000 литров
                    multiple = Graph.MaxCurveX / 100; // Число для деления
                    wholeMultiple = (int)(multiple / 10);      // Целочисленное количество

                    sliceResult = wholeMultiple * realCanvas / multiple;

                    measureResult = Graph.MaxCurveX * wholeMultiple / multiple;
                    stepSlices = realCanvas / 10 * (10 * wholeMultiple / multiple);

                    slicesNo = (int)(realCanvas / stepSlices);
                    break;
                case >= 10_000f and < 100_000f: // Объем бака лежит в пределах 10_000-100_000 литров
                    multiple = Graph.MaxCurveX / 1_000; // Число для деления
                    wholeMultiple = (int)(multiple / 10);      // Целочисленное количество

                    sliceResult = wholeMultiple * realCanvas / multiple;

                    measureResult = Graph.MaxCurveX * wholeMultiple / multiple;
                    stepSlices = realCanvas / 10 * (10 * wholeMultiple / multiple);

                    slicesNo = (int)(realCanvas / stepSlices);
                    break;
                case >= 100_000f and < 1_000_000f: // Объем бака лежит в пределах 100_000-1_000_000 литров
                    multiple = Graph.MaxCurveX / 10_000; // Число для деления
                    wholeMultiple = (int)(multiple / 10);      // Целочисленное количество

                    sliceResult = wholeMultiple * realCanvas / multiple;

                    measureResult = Graph.MaxCurveX * wholeMultiple / multiple;
                    stepSlices = realCanvas / 10 * (10 * wholeMultiple / multiple);

                    slicesNo = (int)(realCanvas / stepSlices);
                    break;
            }
            return (slicesNo, sliceResult, measureResult);
        }

        /// <summary>
        /// Метод, возвращающий число горизонтальных линий, шаг сдвига линий и шаг объема
        /// </summary>
        /// <returns>Кортеж из трех параметров</returns>
        private (int, double, double, double) GetCurrentHorizontalSlices()
        {
            #region Вычисляемые переменные
            int slicesNo = 0; // Количество вертикальных линий
            double sliceResult = 0; // Шаг сдвига вертикальных линий
            double measureResult = 0; // Шаг значения объема
            #endregion

            // Считаем реальный размер , выделенный под график
            var realCanvas = Graph.Canvas.ActualHeight - Graph.Top - Graph.Bottom;

            // Вспомогательные переменные
            double multiple;
            int wholeMultiple;
            double stepSlices;
            double absDelta = Math.Abs(Graph.MaxCurveY - Graph.MinCurveY);
            double firstStep = 0;

            switch (absDelta)
            {
                case >= 10f and < 100f: // Объем бака лежит в пределах 10-100 литров
                    multiple = absDelta / 1; // Число для деления
                    wholeMultiple = (int)(multiple / 10);      // Целочисленное количество
                    firstStep = Graph.MinCurveY % 1;

                    sliceResult = wholeMultiple * realCanvas / multiple;

                    measureResult = absDelta * wholeMultiple / multiple;
                    stepSlices = realCanvas / 10 * (10 * wholeMultiple / multiple);

                    slicesNo = (int)(realCanvas / stepSlices);
                    break;
                case >= 100f and < 1000f: // Объем бака лежит в пределах 100-1_000 литров
                    multiple = absDelta / 10; // Число для деления
                    wholeMultiple = (int)(multiple / 10);      // Целочисленное количество
                    firstStep = Graph.MinCurveY % 10;

                    sliceResult = wholeMultiple * realCanvas / multiple;

                    measureResult = absDelta * wholeMultiple / multiple;
                    stepSlices = realCanvas / 10 * (10 * wholeMultiple / multiple);

                    slicesNo = (int)(realCanvas / stepSlices);
                    break;
                case >= 1_000f and < 10_000f: // Объем бака лежит в пределах 1_000-10_000 литров
                    multiple = absDelta / 100; // Число для деления
                    wholeMultiple = (int)(multiple / 10);      // Целочисленное количество
                    firstStep = Graph.MinCurveY % 100;

                    sliceResult = wholeMultiple * realCanvas / multiple;

                    measureResult = absDelta * wholeMultiple / multiple;
                    stepSlices = realCanvas / 10 * (10 * wholeMultiple / multiple);

                    slicesNo = (int)(realCanvas / stepSlices);
                    break;
                case >= 10_000f and < 100_000f: // Объем бака лежит в пределах 10_000-100_000 литров
                    multiple = absDelta / 1_000; // Число для деления
                    wholeMultiple = (int)(multiple / 10);      // Целочисленное количество
                    firstStep = Graph.MinCurveY % 1000;

                    sliceResult = wholeMultiple * realCanvas / multiple;

                    measureResult = absDelta * wholeMultiple / multiple;
                    stepSlices = realCanvas / 10 * (10 * wholeMultiple / multiple);

                    slicesNo = (int)(realCanvas / stepSlices);
                    break;
                case >= 100_000f and < 1_000_000f: // Объем бака лежит в пределах 100_000-1_000_000 литров
                    multiple = absDelta / 10_000; // Число для деления
                    wholeMultiple = (int)(multiple / 10);      // Целочисленное количество
                    firstStep = Graph.MinCurveY % 10000;

                    sliceResult = wholeMultiple * realCanvas / multiple;

                    measureResult = absDelta * wholeMultiple / multiple;
                    stepSlices = realCanvas / 10 * (10 * wholeMultiple / multiple);

                    slicesNo = (int)(realCanvas / stepSlices);
                    break;
            }
            return (slicesNo, sliceResult, measureResult, firstStep);
        }
        #endregion
    }
}
