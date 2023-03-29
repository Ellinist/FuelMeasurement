namespace FuelMeasurement.Tools.ReportModule.Models
{
    /// <summary>
    /// Класс модели изображения универсальный
    /// Применяется для:
    /// - изображения топливного бака
    /// - изображения МПИ
    /// - изображения тарировочной кривой
    /// - выпекания пирожков
    /// </summary>
    public class ImageReportModel
    {
        public byte[] Image { get; }
        public long ImageWidth { get; }
        public long ImageHeight { get; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="img"></param>
        /// <param name="imageWidth"></param>
        /// <param name="imageHeight"></param>
        public ImageReportModel(byte[] img, long imageWidth, long imageHeight)
        {
            Image       = img;
            ImageWidth  = imageWidth;
            ImageHeight = imageHeight;
        }
    }
}
