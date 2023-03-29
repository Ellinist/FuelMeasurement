using FuelMeasurement.Client.UIModule.Services.Interfaces;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FuelMeasurement.Client.UIModule.Services.Implementations
{
    public class MaterialService : IMaterialService
    {
        public PhongMaterial CreateMaterial(System.Windows.Media.Color color, float opacity)
        {
            return new PhongMaterial
            {
                AmbientColor = PhongMaterials.ToColor(0.1, 0.1, 0.1, 1.0),
                DiffuseColor = new Color4(color.G, color.B, color.R, opacity),
                SpecularColor = PhongMaterials.ToColor(0.0225, 0.0225, 0.0225, 1.0),
                EmissiveColor = PhongMaterials.ToColor(0.0, 0.0, 0.0, 1.0),
                SpecularShininess = 12.8f,
                RenderShadowMap = true,
                EnableFlatShading = true
            };
        }
    }
}
