namespace FuelMeasurement.Common.SupportedFileFormats
{
    public class FileTXT : FileBase
    {
        public const string Description = "Unigraphics";
        public const string Extension = ".txt";
        public const string ShortExtensions = "txt";

        public const string FileHeader = "VERICUT-model";
        public const string FileHeaderVersion = "version-2.0";
        public const string FileHeaderString3 = "1";
        public const string FileHeaderString4 = "0 1 1";
        public const string FileCountVertices = "3";
    }
}
