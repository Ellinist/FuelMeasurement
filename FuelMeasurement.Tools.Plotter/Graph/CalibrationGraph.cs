using System;
using FuelMeasurement.Tools.Plotter.Graph.Axes;
using FuelMeasurement.Tools.Plotter.Graph.Plot;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FuelMeasurement.Tools.Plotter.Graph
{
    /// <summary>
    /// Класс графика
    /// В график могут входить:
    /// 1. Оси
    /// 2. Легенда
    /// 3. Заголовок графика
    /// 4. Набор кривых и объектов графика
    /// </summary>
    public class CalibrationGraph
    {
        /// <summary>
        /// Заголовок графика
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Канва для графика
        /// </summary>
        public Canvas Canvas { get; set; }

        /// <summary>
        /// Масштаб по оси Y
        /// </summary>
        public double ScaleY { get; set; }

        /// <summary>
        /// Название оси X
        /// </summary>
        public string NameAxisX { get; set; }

        /// <summary>
        /// Название оси Y
        /// </summary>
        public string NameAxisY { get; set; }

        /// <summary>
        /// Кривые графика (список)
        /// </summary>
        public List<GraphCurve> CurvesList { get; set; }

        /// <summary>
        /// Минимальное значение по оси X кривых графика
        /// </summary>
        public double MinCurveX { get; set; }

        /// <summary>
        /// Максимальное значение по оси X кривых графика
        /// </summary>
        public double MaxCurveX { get; set; }

        /// <summary>
        /// Минимальное значение по оси Y кривых графика
        /// </summary>
        public double MinCurveY { get; set; }

        /// <summary>
        /// Максимальное значение по оси Y кривых графика
        /// </summary>
        public double MaxCurveY { get; set; }

        /// <summary>
        /// Коэффициент масштабирования кривых на канве по оси X
        /// </summary>
        public double Kx { get; set; } // По оси абсцисс

        /// <summary>
        /// Коэффициент масштабирования кривых на канве по оси Y
        /// </summary>
        public double Ky { get; set; } // По оси ординат

        /// <summary>
        /// Левый отступ от краев канвы
        /// </summary>
        public double Left { get; set; } = 100;

        /// <summary>
        /// Верхний отступ от краев канвы
        /// </summary>
        public double Top { get; set; } = 60;

        /// <summary>
        /// Правый отступ от краев канвы
        /// </summary>
        public double Right { get; set; } = 60;

        /// <summary>
        /// Нижний отступ от краев канвы
        /// </summary>
        public double Bottom { get; set; } = 100;

        /// <summary>
        /// Кегль шрифта
        /// </summary>
        private double TextSize { get; } = 12;

        /// <summary>
        /// Семейство шрифтов
        /// </summary>
        public FontFamily FontType { get; set; } = new("Verdana");

        /// <summary>
        /// Флаг относительного построения кривой тарировки
        /// </summary>
        public bool Relative { get; set; }

        /// <summary>
        /// Сетка графика
        /// </summary>
        public GraphMesh Mesh { get; set; }

        /// <summary>
        /// Счетчик кривых графика
        /// </summary>
        public static int CurvesCounter { get; set; } = 0;

        /// <summary>
        /// Метод добавления кривой на график
        /// </summary>
        /// <param name="arrayX">Массив точек шкалы абсцисс</param>
        /// <param name="arrayY">Массив точек шкалы ординат</param>
        /// <param name="curveName">Название кривой графика</param>
        /// <param name="curveColor"></param>
        public void AddCurve(List<double> arrayX, List<double> arrayY, string curveName, Color curveColor)
        {
            CurvesCounter++;
            GraphCurve curve = new(this, arrayX, arrayY, curveName, CurvesCounter, curveColor); // Создаем экземпляр кривой
            CurvesList.Add(curve); // Добавляем кривую в список кривых
            curve.SetScales(); // Формируем масштабы графика по полученным кривым
        }

        /// <summary>
        /// Метод отрисовки графика тарировочных кривых
        /// </summary>
        public void PlotChart(bool relative)
        {
            Relative = relative;
            PlotCommon(0); // Отрисовываем общие элементы графика
            Mesh = new GraphMesh(this); // Создаем сетку графика
            Mesh.ShowMesh(0); // Отображаем сетку
            GraphAxesX axisX = new(this); // Создаем ось абсцисс
            axisX.Plot(0); // Отрисовываем ось абсцисс
            GraphAxesY axisY = new(this); // Создаем ось ординат
            axisY.Plot(0); // Отрисовываем ось ординат
        }

        /// <summary>
        /// Метод отрисовки общих элементов графика
        /// </summary>
        /// <param name="zOrder"></param>
        private void PlotCommon(int zOrder)
        {
            // Создаем текстовый блок для заголовка графика
            TextBlock header = new()
            {
                HorizontalAlignment = HorizontalAlignment.Center, // Выравнивание по центру
                Text = Title, // Текст заголовка графика
                Foreground = Brushes.Black, // Цвет текста
                FontFamily = FontType, // Семейство применяемого шрифта
                FontSize = TextSize, // Размер кегля
                FontWeight = FontWeights.ExtraLight // Вес шрифта - полужирный
            };
            // Задаем рамку для помещения в нее текста заголовка графика
            // Без нее выравнивание по центру невозможно
            if (Canvas.ActualHeight == 0 || Canvas.ActualWidth == 0) return;

            Border border = new()
            {
                Width = Canvas.ActualWidth - Left - Right, // Задаем ширину рамки
                BorderThickness = new Thickness(0, 0, 0, 0), // Рамка невидима
                Margin = new Thickness(Left, 5, 0, 0), // Позиционирование рамки
                Child = header // Помещаем текст в рамку
            };
            Panel.SetZIndex(border, zOrder);
            Canvas.Children.Add(border); // Загоняем текст заголовка графика на канву
        }

        /// <summary>
        /// Метод отрисовки тарировочных кривых графика
        /// </summary>
        /// <param name="onlySummary"></param>
        /// <param name="zOrder"></param>
        public void PlotCurves(bool onlySummary, int zOrder)
        {
            if (onlySummary)
            {
                CurvesList[^1]?.PlotCurve(zOrder, Colors.Blue); // Отрисовываем суммарную тарировочную кривую бака
            }
            else
            {
                // В цикле отрисовываем все кривые
                for (int i = 0; i < CurvesList.Count; i++)
                {
                    CurvesList[i]?.PlotCurve(zOrder, CurvesList[i].CurveColor); // Отрисовываем тарировочную кривую бака
                }
            }
        }
    }
}
