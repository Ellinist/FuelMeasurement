using HelixToolkit.Wpf.SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FuelMeasurement.Client.UIModule.Services.Interfaces
{
    public interface IMaterialService
    {
        PhongMaterial CreateMaterial(Color color, float opacity);
    }
}
