using Assimp;

namespace FuelMeasurement.Tools.Geometry.Helpers
{
    public static class SupportedFormatsHelper
    {
        /// <summary>
        /// Метод проверки формата файла геометрии
        /// </summary>
        /// <param name="fileExtension">расширение файла</param>
        /// <param name="supportedFormat">Массив поддерживаемых форматов библиотеки Assimp</param>
        /// <returns>True / false</returns>
        public static bool CheckFormat(string fileExtension, object supportedFormat)
        {
            if (supportedFormat as string[] != null)
            {
                return CheckImportFileFormat(fileExtension, (string[])supportedFormat);
            }
            else
            {
                return CheckExportFileFormat(fileExtension, (ExportFormatDescription[])supportedFormat);
            }
        }

        /// <summary>
        /// Метод проверки формата файла для импорта
        /// </summary>
        /// <param name="fileExtension">Расширение файла</param>
        /// <param name="supportedFormats">Массив поддерживаемых форматов библиотеки Assimp</param>
        /// <returns>True / false</returns>
        private static bool CheckImportFileFormat(string fileExtension, string[] supportedFormats)
        {
            for (int i = 0; i < supportedFormats.Length; i++)
            {
                if (fileExtension == supportedFormats[i])
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Метод проверки формата файла для экспорта
        /// </summary>
        /// <param name="fileExtension">Расширение файла</param>
        /// <param name="supportedFormats">Массив поддерживаемых форматов библиотеки Assimp</param>
        /// <returns>True / false</returns>
        private static bool CheckExportFileFormat(string fileExtension, ExportFormatDescription[] supportedFormats)
        {
            for (int i = 0; i < supportedFormats.Length; i++)
            {
                if (fileExtension == supportedFormats[i].FormatId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
