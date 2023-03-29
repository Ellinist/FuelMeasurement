using FuelMeasurement.Client.UIModule.Services.Interfaces;
using ScottPlot;
using ScottPlot.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.UIModule.Services.Implementations
{
    public class PlotterSettingsService : IPlotterSettingsService
    {
        public void ConfigurePlotter(WpfPlot plot, QualityMode quality, bool XAxisVisible, bool YAxisVisible, bool X1AxisVisible, bool Y1AxisVisible)
        {
            plot.Plot.Grid(lineStyle: LineStyle.None);
            plot.Plot.Style(Style.Control);

            plot.Configuration.Quality = quality;
            plot.Configuration.UseRenderQueue = true;
            plot.RightClicked -= plot.DefaultRightClickEvent;

            plot.Plot.XAxis.IsVisible = XAxisVisible;
            plot.Plot.YAxis.IsVisible = YAxisVisible;
            plot.Plot.YAxis2.IsVisible = X1AxisVisible;
            plot.Plot.XAxis2.IsVisible = Y1AxisVisible;

            plot.Configuration.AxesChangedEventEnabled = false;

            plot.Configuration.Pan = false;
            plot.Configuration.Zoom = false;
            plot.Configuration.DoubleClickBenchmark = false;
            plot.Configuration.RightClickDragZoom = false;
            plot.Configuration.MiddleClickDragZoom = false;
        }
    }
}
