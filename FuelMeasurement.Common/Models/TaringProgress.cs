namespace FuelMeasurement.Common.Models
{
    /// <summary>
    /// Класс прогресса тарировки
    /// </summary>
    public class TaringProgress
    {
        public int ProgressMinimum { get; set; }
        public int ProgressMaximum { get; set; }
        public int CurrentStep { get; set; }
    }
}
