using System;
using System.Threading.Tasks;
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
    public class WriteGeometryFilesController : IWriteGeometryFilesController
    {
        private readonly ITxtFormatFileWriter _txtWriter;
        private readonly ITriFormatFileWriter _triWriter;
        private readonly ILogger _logger;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="txtWriter">Интерфейс для записи txt файлов</param>
        /// <param name="triWriter">Интерфейс для записи tri файлов</param>
        /// <param name="logger">Логгер</param>
        public WriteGeometryFilesController(ITxtFormatFileWriter txtWriter, ITriFormatFileWriter triWriter, ILogger logger)
        {
            _txtWriter = txtWriter;
            _triWriter = triWriter;
            _logger = logger;
        }

        /// <summary>
        /// Запись геометрии в файл
        /// </summary>
        /// <param name="writeObject">Объект для записи</param>
        /// <param name="filePath">Путь к файлу</param>
        /// <param name="fileExtension">Расширение файла</param>
        /// <returns></returns>
        public async Task WriteGeometryInFile(object writeObject, string filePath, string fileExtension)
        {
            await WriteFile(writeObject, filePath, fileExtension);
        }

        /// <summary>
        /// Запись геометрии в файл
        /// </summary>
        /// <param name="writeObject">Объект для записи</param>
        /// <param name="filePath">Путь к файлу</param>
        /// <param name="fileExtension">Расширение файла</param>
        /// <returns></returns>
        private async Task<bool> WriteFile(object writeObject, string filePath, string fileExtension)
        {
            // Создаётся контекст библиотеки ассимп, т.к. запись в файл stl делается через него
            using var context = new AssimpContext();

            bool result = false;

            // Получение поддерживаемых форматов библиотеки ассимп
            var supFormats = context.GetSupportedExportFormats();

            // Проверяем наш формат 
            if (SupportedFormatsHelper.CheckFormat(fileExtension, supFormats))
            {
                try
                {
                    if (writeObject is MeshModel model) // Если объект меш
                    {
                        // Конвертируем геометрию в модель сцены и записываем геометрию в файл
                        result = context.ExportFile(
                        FuelTankToSceneConverter.ConvertFuelTankGeometryModelToScene(model),
                        filePath, fileExtension, PostProcessSteps.GenerateNormals);
                    }
                    else if (writeObject is Scene scene) // Если объект сцена
                    {
                        result = context.ExportFile(scene, filePath, fileExtension); // Записываем геометрию в файл
                    }
                    else { result = false; }
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
                    if (fileExtension == FileTRI.ShortExtensions) // "tri"
                    {
                        result = await WriteTriFile(writeObject, filePath);
                    }
                    else if (fileExtension == FileTXT.ShortExtensions) // "txt"
                    {
                        result = await WriteTxtFile(writeObject, filePath);
                    }
                    else { result = false; }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            if (!context.IsDisposed)
            {
                context.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Запись геометрии в файл TXT
        /// </summary>
        /// <param name="writeObject">Объект для записи</param>
        /// <param name="filePath">Путь к файлу</param>
        /// <returns></returns>
        private async Task<bool> WriteTxtFile(object writeObject, string filePath)
        {
            bool result;

            if (writeObject is MeshModel model) // Если объект меш
            {
                // Конвертируем геометрию в модель сцены и записываем геометрию в файл
                result = await _txtWriter.Write(
                    FuelTankToSceneConverter.ConvertFuelTankGeometryModelToScene(model), filePath);
            }
            else if (writeObject is Scene scene) // Если объект сцена
            {
                // Записываем геометрию в файл
                result = await _txtWriter.Write(scene, filePath);
            }
            else 
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Запись геометрии в файл TRI
        /// </summary>
        /// <param name="writeObject">Объект для записи</param>
        /// <param name="filePath">Путь к файлу</param>
        /// <returns></returns>
        private async Task<bool> WriteTriFile(object writeObject, string filePath)
        {
            bool result;

            if (writeObject is MeshModel model) // Если объект меш
            {
                // Конвертируем геометрию в модель сцены и записываем геометрию в файл
                result = await _triWriter.Write(
                    FuelTankToSceneConverter.ConvertFuelTankGeometryModelToScene(model), filePath);
            }
            else if (writeObject is Scene scene) // Если объект сцена
            {
                // Записываем геометрию в файл
                result = await _triWriter.Write(scene, filePath);
            }
            else 
            {
                result = false;
            }

            return result;
        }
    }
}
