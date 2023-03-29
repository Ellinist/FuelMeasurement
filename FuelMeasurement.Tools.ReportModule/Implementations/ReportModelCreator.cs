using FuelMeasurement.Tools.CalculationData.Models;
using FuelMeasurement.Tools.ReportModule.Interfaces;
using FuelMeasurement.Tools.ReportModule.Models;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace FuelMeasurement.Tools.ReportModule.Implementations
{
    /// <summary>
    /// Класс создания модели отчета
    /// </summary>
    public class ReportModelCreator : IReportModelCreator
    {
        private readonly IBitmapHelper _bitmapReportHelper;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="bitmapReportHelper"></param>
        public ReportModelCreator(IBitmapHelper bitmapReportHelper)
        {
            _bitmapReportHelper = bitmapReportHelper;
        }

        /// <summary>
        /// Создание модели отчета по самолету целиком
        /// </summary>
        /// <param name="tankViewList">Список моделей отчетов изображений топливных баков</param>
        /// <param name="canvas">Канва с тарировочной кривой</param>
        /// <param name="model">Модель вычислителя</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <returns></returns>
        public AirplaneReportModel CreateAirplaneReportModel(List<ImageReportModel> tankViewList, Canvas canvas, CalculationModel model, List<TankModel> tanks)
        {
            #region Создание изображения МПИ
            ImageReportModel errorsImage = _bitmapReportHelper.ErrorsGraphToBitmap(model, tanks);
            #endregion

            #region Создание изображения тарировочной кривой
            ImageReportModel curveImage = _bitmapReportHelper.TaringCurveToBitmap(canvas);
            #endregion

            List<TankReportModel> trm = new List<TankReportModel>();
            foreach (var t in tanks)
            {
                trm.Add(CreateTankReportModel(tankViewList, canvas, model, t));
            }
            
            AirplaneReportModel airplaneReportModel = new AirplaneReportModel(model.CurrentBranch.AirplaneName, tankViewList, trm, errorsImage, curveImage);
            return airplaneReportModel;
        }

        /// <summary>
        /// Создание модели отчета по одному выбранному баку
        /// </summary>
        /// <param name="tankViewList"></param>
        /// <param name="canvas"></param>
        /// <param name="model">Модель вычислителя</param>
        /// <param name="tank"></param>
        /// <returns></returns>
        public TankReportModel CreateTankReportModel(List<ImageReportModel> tankViewList, Canvas canvas, CalculationModel model, TankModel tank)
        {
            #region Создание изображения МПИ
            ImageReportModel errorsImage = _bitmapReportHelper.ErrorsGraphToBitmap(model, tank);
            #endregion

            #region Создание изображения тарировочной кривой
            List<TankModel> tm = new()
            {
                tank
            };
            ImageReportModel curveImage = _bitmapReportHelper.TaringCurveToBitmap(canvas);
            #endregion

            List<SensorReportModel> sensors = new();
            for (int j = 0; j < tank.Sensors.Count; j++)
            {
                sensors.Add(CreateSensorReportModel(tank.Sensors[j]));
            }

            TarResult result = tank.TarResultList.Find(x => x.Pitch == model.CurrentBranch.AnglesModel.ReferencedPitch
                                                         && x.Roll == model.CurrentBranch.AnglesModel.ReferencedRoll);

            return new TankReportModel(tank.TankName, tank.TankVolume,
                                       result.ResultAxisX, result.ResultAxisY, result.RelativeResultAxisY,
                                       tank.UpUnmeasurableVolume, tank.DownUnmeasurableVolume,
                                       tank.UpPercent, tank.DownPercent,
                                       sensors, tank.InactiveSensorCounter,
                                       curveImage, errorsImage, tankViewList[^1]);
        }

        /// <summary>
        /// Создает модель датчика для отчёта
        /// </summary>
        /// <param name="sensor">Датчик</param>
        /// <returns></returns>
        private static SensorReportModel CreateSensorReportModel(SensorModel sensor)
        {
            return new SensorReportModel(sensor.SensorName, sensor.Length,
                       new PointReportModel(sensor.UpPoint.ToString()),
                       new PointReportModel(sensor.DownPoint.ToString()),
                       sensor.UpIndent, sensor.DownIndent, sensor.GroupName,
                       Math.Round(sensor.LowFuelLevel, 2), Math.Round(sensor.HighFuelLevel, 2),
                       Math.Round(sensor.LowFuelVolume, 2), Math.Round(sensor.HighFuelVolume, 2));
        }
    }
}
