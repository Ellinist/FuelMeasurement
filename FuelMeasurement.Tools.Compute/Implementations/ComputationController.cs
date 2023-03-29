using FuelMeasurement.Tools.Compute.Interfaces;
using FuelMeasurement.Tools.CalculationData.Models;
using FuelMeasurement.Common.Enums;
using FuelMeasurement.Model.DTO.Models.AirplaneModels;
using NLog;
using System.Collections.Generic;
using System.Linq;
using FuelMeasurement.Tools.ComputeModule.Interfaces;
using System.Collections.ObjectModel;
using FuelMeasurement.Model.Models.GeometryModels;
using System.Windows.Media.Media3D;
using System;

namespace FuelMeasurement.Tools.Compute.Implementations
{
    /// <summary>
    /// Контроллер расчетчика
    /// </summary>
    public class ComputationController : IComputationController
    {
        private readonly IMeasureController      _measureController;
        private readonly IErrorsController       _errorsController;
        private readonly ICommonMirrorController _commonMirrorController;
        private readonly ICapacityController     _capacityController;
        private readonly IReferencedController   _referencedController;
        private static ILogger _logger;

        /// <summary>
        /// Конструктор
        /// </summary>
        public ComputationController(IMeasureController measureController,
                                     IErrorsController errorsController,
                                     ICapacityController capacityController,
                                     ICommonMirrorController commonMirrorController,
                                     IReferencedController referencedController,
                                     ILogger logger)
        {
            _measureController  = measureController;
            _errorsController   = errorsController;
            _capacityController = capacityController;
            _commonMirrorController = commonMirrorController;
            _referencedController   = referencedController;
            _logger = logger;
        }

        /// <summary>
        /// Инициализация расчетчика
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tanks"></param>
        public void GraphicsInitialize(CalculationModel model, ObservableCollection<FuelTankGeometryModel> tanks)
        {
            if (model == null || tanks == null || tanks.ToList().Count == 0) return;

            #region Формирование списка расчетных баков
            model.SelectedTanks.Clear(); // Сначала чистим список выбранных баков

            // В цикле пробегаем по всем выбранным бакам и ищем баки в полном списке баков, у которых Id тот же
            foreach (var tank in tanks)
            {
                var t = model.BranchTanks.Find(x => x.Id == tank.Id);
                if(t != null)
                {
                    model.SelectedTanks.Add(t);
                }
            }
            #endregion

            //model.ActiveSensorsInCurrentTanks = 0;
            model.SumVolumeOfCurrentTanks = 0;
            foreach (var tank in model.SelectedTanks) // Цикл по всем бакам, выбранным в UI
            {
                // В цикле с маппером проходим по всем датчикам бака
                // Здесь поменять счетчик на геометрическую модель из UI
                // Да, в параметрах надо получать геометрическую модель датчиков
                for(int i = 0; i < tank.Sensors.Count; i++)
                {
                    SensorModel sensor = new(); // Здесь берем датчик из маппера
                    if (sensor.IsActiveSensor)
                    {
                        tank.Sensors.Add(sensor);
                        //model.ActiveSensorsInCurrentTanks++;

                        // И вычисляем неизмеряемые величины для добавленного датчика
                        _measureController.CalcSensorMeasures(sensor);
                    }
                }

                model.SumVolumeOfCurrentTanks += tank.TankVolume;
                //model.BranchTanks.Add(tank);
            }

            // Этот switch выполняется однократно при формировании серии кривых (при создании модели расчетчика)
            //EstablishCurves(model, type);
        }

        /// <summary>
        /// Добавление датчика в бак
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tank"></param>
        /// <param name="sensor"></param>
        public void AddSensor(CalculationModel model, string tankId, SensorModelDTO sensor)
        {
            SensorModel _sensor = new();
            _sensor.IsActiveSensor = sensor.IsActiveSensor;
            _sensor.SensorId = sensor.Id;
            _sensor.SensorLinearCapacity = sensor.LinearCapacity;
            _sensor.Length = sensor.Length;
            var up = sensor.UpPoint;
            var down = sensor.DownPoint;
            _sensor.UpPoint = new Point3D(Math.Round(up.X, 2), Math.Round(up.Y, 2), Math.Round(up.Z, 2));
            _sensor.DownPoint = new Point3D(Math.Round(down.X, 2), Math.Round(down.Y, 2), Math.Round(down.Z, 2));

            var tank = model.SelectedTanks.Find(x => x.Id == tankId);
            if(tank != null)
            {
                _sensor.FuelTank = tank;
                tank.Sensors.Add(_sensor);
                model.ActiveSensorsInCurrentTanks++;

                // И вычисляем неизмеряемые величины для добавленного датчика
                _measureController.CalcSensorMeasures(_sensor);
            }
        }

        /// <summary>
        /// Удаление датчика в баке
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tank"></param>
        /// <param name="sensor"></param>
        public void DeleteSensor(CalculationModel model, SensorModelDTO sensor)
        {
            foreach(var tank in model.SelectedTanks)
            {
                var _sensor = tank.Sensors.Find(x => x.SensorId == sensor.Id);
                if(_sensor != null)
                {
                    tank.Sensors.Remove(_sensor);
                    model.ActiveSensorsInCurrentTanks--;
                    break;
                }
            }
            //var tank = model.SelectedTanks.Find(x => x.Id == tankId);
            //if(tank != null)
            //{
            //    tank.Sensors.Remove(sensor);
            //    model.ActiveSensorsInCurrentTanks--;

            //    // Этот switch выполняется однократно при формировании серии кривых (при создании модели расчетчика)
            //    //EstablishCurves(model, type);
            //}
        }

        /// <summary>
        /// Изменение положения и параметров датчика
        /// </summary>
        /// <param name="model">Модель вычислителя</param>
        /// <param name="tanks">Список выбранных для расстановки баков</param>
        /// <param name="tank">Бак, которому принадлежит перемещаемый датчик</param>
        /// <param name="sensor">Перемещаемый датчик</param>
        /// <param name="type">Выбранный тип расчетной кривой</param>
        public void MoveSensor(CalculationModel model, SensorModel sensor, Point3D up, Point3D down)
        {
            sensor.UpPoint = new Point3D(Math.Round(up.X, 2), Math.Round(up.Y, 2), Math.Round(up.Z, 2));
            sensor.DownPoint = new Point3D(Math.Round(down.X, 2), Math.Round(down.Y, 2), Math.Round(down.Z, 2));
            _measureController.CalcSensorMeasures(sensor);


            // И вычисляем неизмеряемые величины для датчика, у которого изменились параметры


            //// Этот switch выполняется многократно при движении датчика - меняются значения массивов
            //switch (type)
            //{
            //    case CurvesTypeEnum.UsualErrors:
            //        _errorsController.UpdateTankErrors();
            //        break;
            //    case CurvesTypeEnum.FuelInErrors:
            //        _errorsController.UpdateTankFuelInErrors();
            //        break;
            //    case CurvesTypeEnum.FuelOutErrors:
            //        _errorsController.UpdateTankFuelOutErrors();
            //        break;
            //    case CurvesTypeEnum.CommonMirrorErrors:
            //        _commonMirrorController.UpdateCommonMirrorErrors();
            //        break;
            //    case CurvesTypeEnum.FuelOutMirrorErrors:
            //        _commonMirrorController.UpdateBackCommonMirrorErrors();
            //        break;
            //    case CurvesTypeEnum.UsualReferencedErrors:
            //        _referencedController.UpdateReferencedErrors();
            //        break;
            //    case CurvesTypeEnum.FuelInReferencedErrors:
            //        _referencedController.UpdateReferencedErrorsIn();
            //        break;
            //    case CurvesTypeEnum.FuelOutReferencedErrors:
            //        _referencedController.UpdateReferencedErrorsOut();
            //        break;
            //    case CurvesTypeEnum.UsualCapacity:
            //        _capacityController.UpdateSensorsCapacity();
            //        break;
            //    case CurvesTypeEnum.FuelInCapacity:
            //        _capacityController.UpdateSensorsCapacityIn();
            //        break;
            //    case CurvesTypeEnum.FuelOutCapacity:
            //        _capacityController.UpdateSensorsCapacityOut();
            //        break;
            //    //case CurvesTypeEnum.Calibrations:
            //    //    break;
            //}
        }

        ///// <summary>
        ///// Думать - подойдет ли для всех случаев - Calibrations убрать ?????????????
        ///// </summary>
        ///// <param name="type"></param>
        //private void EstablishCurves(CalculationModel model, CurvesTypeEnum type)
        //{
        //    switch (type)
        //    {
        //        case CurvesTypeEnum.UsualErrors: // МПИ для обычного режима
        //            _errorsController.ComputeTankErrors(model);
        //            break;
        //        case CurvesTypeEnum.FuelInErrors: // МПИ для режима заправки
        //            _errorsController.ComputeTankFuelInErrors(model);
        //            break;
        //        case CurvesTypeEnum.FuelOutErrors: // МПИ для режима выработки
        //            _errorsController.ComputeTankFuelOutErrors(model);
        //            break;
        //        case CurvesTypeEnum.CommonMirrorErrors: // МПИ по общему зеркалу топлива (обычный режим и заправка)
        //            _commonMirrorController.ComputeCommonMirrorErrors(model);
        //            break;
        //        case CurvesTypeEnum.FuelOutMirrorErrors: // МПИ по общему зеркалу топлива (режим выработки)
        //            _commonMirrorController.ComputeBackCommonMirrorErrors(model);
        //            break;
        //        case CurvesTypeEnum.UsualReferencedErrors: // МПИ для обычного режима по опорным углам
        //            _referencedController.SetReferencedErrors(model, _errorsController, _capacityController);
        //            break;
        //        case CurvesTypeEnum.FuelInReferencedErrors: // МПИ для режима заправки по опорным углам
        //            _referencedController.SetReferencedErrorsIn(model, _errorsController, _capacityController);
        //            break;
        //        case CurvesTypeEnum.FuelOutReferencedErrors: // МПИ для режима выработки по опорным углам
        //            _referencedController.SetReferencedErrorsOut(model, _errorsController, _capacityController);
        //            break;
        //        case CurvesTypeEnum.UsualCapacity: // Кривая емкости для обычного режима
        //            _capacityController.SetSensorsCapacity(model);
        //            break;
        //        case CurvesTypeEnum.FuelInCapacity: // Кривая емкости для режима заправки
        //            _capacityController.SetSensorsCapacityIn(model);
        //            break;
        //        case CurvesTypeEnum.FuelOutCapacity: // Кривая емкости для режима выработки
        //            _capacityController.SetSensorsCapacityOut(model);
        //            break;
        //        //case CurvesTypeEnum.Calibrations:
        //        //    break;
        //    }
        //}
    }
}
