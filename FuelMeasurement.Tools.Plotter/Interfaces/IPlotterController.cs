using FuelMeasurement.Tools.CalculationData.Models;
using System.Collections.Generic;
using System.Windows.Controls;
using ScottPlot;

namespace FuelMeasurement.Tools.Plotter.Interfaces
{
    public interface IPlotterController
    {
        /// <summary>
        /// Отрисовка тарировочной кривой на канве
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="model"></param>
        /// <param name="tanks"></param>
        /// <param name="relative"></param>
        /// <param name="sensorSelected"></param>
        void FillTaringCanvas(Canvas canvas, CalculationModel model, bool relative, SensorModel sensorSelected, double pitch, double roll);

        /// <summary>
        /// Метод получения графика МПИ в ScottPlot
        /// </summary>
        /// <param name="model"></param>
        /// <param name="plot"></param>
        /// <param name="tanks"></param>
        void SetErrorsCurves(CalculationModel model, WpfPlot plot, List<TankModel> tanks, double pitch, double roll);

        void UpdateErrorsCurves(CalculationModel model, WpfPlot plot, List<TankModel> tanks, double pitch, double roll);

        void SetCapacityCurves(CalculationModel model, WpfPlot plot, double pitch, double roll);

        void UpdateCapacityCurves(CalculationModel model, WpfPlot plot, double pitch, double roll);



        (double pointX, double pointY, int pointIndex) GetErrorNearest(double mouseX, double mouseY);
        (double nearestVolume, double nearestError) GetErrorSeriePoints(int pointIndex);
        (double pointX, double pointY, int pointIndex) GetCapacityNearest(double mouseX, double mouseY);
        (double nearestCapacityVolume, double nearestCapacity) GetCapacitySeriePoints(int pointIndex);
    }
}