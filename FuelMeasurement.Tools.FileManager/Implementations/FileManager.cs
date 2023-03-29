using FuelMeasurement.Common.DialogModule.Interfaces;
using FuelMeasurement.Common.SupportedFileFormats;
using FuelMeasurement.Model.DTO.Models.ProjectModels;
using FuelMeasurement.Tools.FileManager.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Tools.FileManager.Implementations
{
    public class FileManager : IFileManager
    {
        private readonly IDialogServices _dialogService;
        private readonly IXMLManager _XMLManager;
        private readonly IProjectPathManager _pathManager;
        private readonly IFileFilterManager _fileFilterManager;

        public FileManager(
            IDialogServices dialogService, 
            IXMLManager XMLManager,
            IProjectPathManager pathManager,
            IFileFilterManager fileFilterManager
            )
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _XMLManager = XMLManager ?? throw new ArgumentNullException(nameof(XMLManager));
            _pathManager = pathManager ?? throw new ArgumentNullException(nameof(pathManager));
            _fileFilterManager = fileFilterManager ?? throw new ArgumentNullException(nameof(fileFilterManager));
        }

        public (ProjectModelDTO, List<string>) LoadProjectXML()
        {
            ProjectModelDTO project = null;
            List<string> errors = new ();

            _dialogService.OpenFileDialog(
                _fileFilterManager.GetFileFilter(typeof(FileXML)),
                false,
                dialogResult =>
                {
                    if (dialogResult.Any())
                    {
                        foreach (var file in dialogResult)
                        {
                            var result = _XMLManager.Load(file);
                            errors = result.Item2;
                            if (!result.Item2.Any())
                            {
                                project = result.Item1;
                            }
                        }
                    }
                });

            return (project, errors);
        }

        public (ProjectModelDTO, List<string>) LoadProjectZIP()
        {
            ProjectModelDTO project = null;
            List<string> errors = new();

            _dialogService.OpenFileDialog(
                _fileFilterManager.GetFileFilter(typeof(FileZip)), 
                false,
                dialogResult => 
                {
                    if (dialogResult.Any())
                    {
                        foreach (var file in dialogResult)
                        {
                            using (ZipArchive archive = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.GetEncoding(866)))
                            {
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    var tempPath = _pathManager.GetTempPath();

                                    //entry.ExtractToFile(tempPath);
                                }
                            }

                            // После открытия архива и переноса файлов из архива в TEMP
                            // Сделать дисериализацию из файлов xml и вернуть
                        }
                    }
                });

            return (project, errors);
        }

        public bool SaveCurrentProject(ProjectModelDTO project)
        {
            bool result = false;

            _dialogService.SaveFileDialog(
                _fileFilterManager.GetFileFilter(typeof(FileZip)),
                project.Name,
                dialogResult => 
                {
                    if (!string.IsNullOrWhiteSpace(dialogResult))
                    {
                        try
                        {
                            ZipFile.CreateFromDirectory(_pathManager.GetTempPath(), dialogResult, CompressionLevel.Fastest, true);
                            result = true;
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }
                });

            return result;
        }
    }
}
