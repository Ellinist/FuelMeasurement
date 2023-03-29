using Assimp;
using FuelMeasurement.Common.SupportedFileFormats;
using FuelMeasurement.Model.Models.GeometryModels;
using FuelMeasurement.Tools.Geometry.Helpers;
using FuelMeasurement.Tools.Geometry.Interfaces;
using FuelMeasurement.Tools.Geometry.Interfaces.TriFormat;
using FuelMeasurement.Tools.Geometry.Interfaces.TxtFormat;
using NLog;

namespace FuelMeasurement.Tools.Geometry.Implementations
{
    /// <summary>
    /// Контроллер чтения геометрии
    /// </summary>
    public class ReadGeometryFileController : IReadGeometryFileController
    {
        private readonly ITxtFormatFileReader _txtReader;
        private readonly ITriFormatFileReader _triReader;
        private readonly ILogger _logger;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="txtReader">Интерфейс для чтения txt файлов</param>
        /// <param name="triReader">Интерфейс для чтения txt файлов</param>
        /// <param name="logger">Логгер</param>
        public ReadGeometryFileController(ITxtFormatFileReader txtReader, ITriFormatFileReader triReader, ILogger logger)
        {
            _txtReader = txtReader;
            _triReader = triReader;
            _logger = logger;
        }

        /// <summary>
        /// Чтение геометрии из файла
        /// </summary>
        /// <param name="file">Путь к файлу</param>
        /// <returns>Модель геометрии</returns>
        public async Task<FuelTankGeometryModel?> ReadGeometry(string file)
        {
            return await ReadGeometryInFile(file);
        }

        /// <summary>
        /// Чтение геометрии из файла
        /// </summary>
        /// <param name="file">Путь к файлу</param>
        /// <returns></returns>
        private async Task<FuelTankGeometryModel?> ReadGeometryInFile(string file)
        {
            using var context = new AssimpContext();

            // Получение поддерживаемых форматов библиотеки ассимп
            var fileExtension = Path.GetExtension(file).ToLower();

            MeshModel? meshModel = null;

            // Проверяем наш формат 
            if (SupportedFormatsHelper.CheckFormat(fileExtension, context.GetSupportedImportFormats()))
            {
                try 
                {
                    meshModel = await ReadFile(context, file); 
                }
                catch (Exception ex) 
                {
                    _logger.Error(ex);
                }
            }
            else // Если формат файла не поддерживается, то проверяем его на наши форматы, т.е. txt и tri
            {
                try
                {
                    if (fileExtension == FileTRI.Extension) // "tri"
                    {
                        meshModel = await ReadTriFile(file);
                    }
                    else if (fileExtension == FileTXT.Extension) // "txt"
                    {
                        meshModel = await ReadTxtFile(file);
                    }
                    else 
                    {
                        meshModel = null; 
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            if (meshModel != null)
            {
                FuelTankGeometryModel tank = new(
                    fileExtension, 
                    Path.GetFileName(file),
                    file)
                {
                    Mesh = meshModel
                };

                return tank;
            }

            return null;
        }

        /// <summary>
        /// Чтение из TXT файла
        /// </summary>
        /// <param name="file">Путь к файлу</param>
        /// <returns>Меш</returns>
        private async Task<MeshModel> ReadTxtFile(string file)
        {
            return await _txtReader.Read(file);
        }

        /// <summary>
        /// Чтение из TRI файла
        /// </summary>
        /// <param name="file">Путь к файлу</param>
        /// <returns>Меш</returns>
        private async Task<MeshModel> ReadTriFile(string file)
        {
            return await _triReader.Read(file);
        }

        /// <summary>
        /// Чтение файла библиотекой ассимп
        /// </summary>
        /// <param name="context">Контекст библиотеки ассимп</param>
        /// <param name="file">Путь к файлу</param>
        /// <returns>Меш</returns>
        private async Task<MeshModel?> ReadFile(AssimpContext context, string file)
        {
            var scene = context.ImportFile(file);

            if (!context.IsDisposed)
            {
                context.Dispose();
            }

            MeshModel? result = null;

            if (scene != null)
            {
                result = await Task.Run(() => FuelTankToSceneConverter.ConvertSceneToFuelTankModel(scene));
            }

            return result;
        }
    }
}
