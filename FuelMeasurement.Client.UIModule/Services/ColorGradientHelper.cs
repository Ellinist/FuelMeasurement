using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FuelMeasurement.Client.UIModule.Services
{
    public static class ColorGradientHelper
    {
        public static IEnumerable<Color> GetColorGradient(Color from, Color to, int totalNumberOfColors)
        {
            if (totalNumberOfColors < 2)
            {
                totalNumberOfColors = 2;
            }

            double diffA = to.A - from.A;
            double diffR = to.R - from.R;
            double diffG = to.G - from.G;
            double diffB = to.B - from.B;

            var steps = totalNumberOfColors - 1;

            var stepA = diffA / steps;
            var stepR = diffR / steps;
            var stepG = diffG / steps;
            var stepB = diffB / steps;

            yield return from;

            for (var i = 1; i < steps; ++i)
            {
                yield return Color.FromArgb(
                    (byte)c(from.A, stepA),
                    (byte)c(from.R, stepR),
                    (byte)c(from.G, stepG),
                    (byte)c(from.B, stepB));

                int c(int fromC, double stepC)
                {
                    return (int)Math.Round(fromC + stepC * i);
                }
            }

            yield return to;
        }
    }
}
