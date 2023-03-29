using SharpDX;
using System.Windows.Media;

namespace FuelMeasurement.Client.UIModule.Infrastructure
{
    public class CoordinateLineDefaultValues
    {
        public static Vector3 LineStartPoint = new Vector3(0,0,0);
        public static Vector3 LineXEndPoint = new Vector3(50000, 0, 0);
        public static Vector3 LineYEndPoint = new Vector3(0, 50000, 0);
        public static Vector3 LineZEndPoint = new Vector3(0, 0, 50000);
        public static System.Windows.Media.Color LineXColor = Colors.Red;
        public static System.Windows.Media.Color LineYColor = Colors.Green;
        public static System.Windows.Media.Color LineZColor = Colors.Blue;
        public static double DefaultDiamert = 50;
    }
}
