using ScottPlot;
using ScottPlot.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.UIModule.Services.Interfaces
{
    public interface IPlotterSettingsService
    {
        void ConfigurePlotter(
            WpfPlot plot, 
            QualityMode quality, 
            bool XAxisVisible,
            bool YAxisVisible, 
            bool X1AxisVisible, 
            bool Y1AxisVisible
            );
    }
}
