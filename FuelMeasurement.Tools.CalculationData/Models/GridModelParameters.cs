using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FuelMeasurement.Tools.CalculationData.Models
{
    public class GridModelParameters
    {
        public Point3D Center { get; }
        public double GridWidth { get; }
        public double GridLength { get; }
        public double GridMinorDistance { get; }
        public double GridMajorDistance { get; }
        public double GridThickness { get; }
        public Brush Fill { get; set; } = Brushes.Black;
        public Vector3D Normal { get; set; } = new Vector3D(0, 1, 0);
        public Vector3D GridLengthDirection { get; set; } = new Vector3D(1, 0, 0);

        public GridModelParameters(Point3D center, double gridWidth, double gridLength,
            double gridMinorDistance, double gridMajorDistance, double gridThickness, Vector3D normal)
        {
            Center = center;
            GridWidth = gridWidth;
            GridLength = gridLength;
            GridMinorDistance = gridMinorDistance;
            GridMajorDistance = gridMajorDistance;
            GridThickness = gridThickness;
            Normal = normal;
        }
    }
}
