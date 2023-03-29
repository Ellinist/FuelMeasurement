using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace FuelMeasurement.Tools.CalculationData.Models
{
    public class GeometryMaterial
    {
        public Material Material { get; set; }

        public Material BackMaterial { get; set; }

        public Material FuelTankBackMaterial { get; set; } = null;

        public GeometryMaterial(Material material, Material backMaterial)
        {
            Material = material;
            BackMaterial = backMaterial;
        }

        public GeometryMaterial()
        {

        }
    }
}
