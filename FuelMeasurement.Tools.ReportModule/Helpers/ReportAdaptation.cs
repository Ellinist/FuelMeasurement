namespace FuelMeasurement.Tools.ReportModule.Helpers
{
    public static class ReportAdaptation
    {
        /// <summary>
        /// Перевод миллиметров в нужные единицы
        /// </summary>
        /// <param name="mm"></param>
        /// <returns></returns>
        public static ushort FromMillimeters(this int mm)
        {
            return (ushort)(mm * 5.67);
        }
    }
}
