using FuelMeasurement.Common.Enums;
using FuelMeasurement.Common.SupportedFileFormats;
using FuelMeasurement.Tools.CalculationData.Models;
using FuelMeasurement.Tools.FileManager.Interfaces;
using FuelMeasurement.Tools.ReportModule.Interfaces;
using FuelMeasurement.Tools.ReportModule.Models;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace FuelMeasurement.Tools.ReportModule.Implementations
{
    public class ReportController : IReportController
    {
        private readonly IReportModelCreator _reportModelCreator;
        private readonly IWordReportController _wordReportController;
        private readonly IFileFilterManager _fileFilterManager;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="reportModelCreator"></param>
        /// <param name="wordReportController"></param>
        public ReportController(
            IReportModelCreator reportModelCreator,
            IWordReportController wordReportController,
            IFileFilterManager fileFilterManager
            )
        {
            _reportModelCreator   = reportModelCreator;
            _wordReportController = wordReportController;
            _fileFilterManager = fileFilterManager ?? throw new ArgumentNullException(nameof(fileFilterManager));
        }

        /// <summary>
        /// Создание отчета по самолету
        /// </summary>
        /// <param name="tankViewList"></param>
        /// <param name="canvas"></param>
        /// <param name="reportType"></param>
        /// <param name="model"></param>
        /// <param name="branchTanks"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public void CreateAirplaneReport(List<ImageReportModel> tankViewList,
                                         Canvas canvas,
                                         ReportTypeEnum reportType,
                                         CalculationModel model,
                                         List<TankModel> branchTanks,
                                         string file)
        {
            switch (reportType)
            {
                case ReportTypeEnum.WordReport:
                    AirplaneReportModel reportModel = _reportModelCreator.CreateAirplaneReportModel(tankViewList, canvas, model, branchTanks); // Создание модели
                    _wordReportController.CreateAirplaneWordDocument(file, reportModel);
                    break;
                case ReportTypeEnum.PdfReport:
                    // For the future!
                    break;
            }
        }

        /// <summary>
        /// Создание отчета по топливному баку
        /// </summary>
        /// <param name="tankViewList"></param>
        /// <param name="canvas"></param>
        /// <param name="reportType"></param>
        /// <param name="model"></param>
        /// <param name="tanks"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public void CreateTankReport(List<ImageReportModel> tankViewList,
                                     Canvas canvas,
                                     ReportTypeEnum reportType,
                                     CalculationModel model,
                                     List<TankModel> tanks,
                                     string file)
        {
            switch (reportType)
            {
                case ReportTypeEnum.WordReport:
                    TankReportModel reportModel = _reportModelCreator.CreateTankReportModel(tankViewList, canvas, model, tanks[^1]);
                    _wordReportController.CreateTankWordDocument(file, reportModel);
                    break;
                case ReportTypeEnum.PdfReport:
                    // For the future!
                    break;
            }
        }

        /// <summary>
        /// Получение пути сохранения файла отчета
        /// </summary>
        /// <returns></returns>
        public string GetFilePath()
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new()
            {
                Filter = _fileFilterManager.GetFileFilter(typeof(FileDOCX)),   /*; $"{FileDOCX.Description} | *{FileDOCX.Extension}"*/
                DefaultExt = FileDOCX.Extension
            };

            string path = string.Empty;

            if (saveFileDialog.ShowDialog() == true)
            {
                path = saveFileDialog.FileName;
            }

            return path;
        }
    }
}
