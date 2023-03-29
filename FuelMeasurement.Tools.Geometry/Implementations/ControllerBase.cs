using System.Globalization;

namespace FuelMeasurement.Tools.Geometry.Implementations
{
    public abstract class ControllerBase
    {
        public static readonly char Delimiter = ' ';
        public static readonly string Separator = ".";

        public NumberFormatInfo Nfi { get; }
        public ControllerBase()
        {
            Nfi = new NumberFormatInfo() { NumberDecimalSeparator = Separator };
        }
    }
}
