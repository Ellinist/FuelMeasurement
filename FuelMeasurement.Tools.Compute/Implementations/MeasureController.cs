using FuelMeasurement.Tools.Compute.Interfaces;
using Assimp;
using FuelMeasurement.Tools.CalculationData.Models;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FuelMeasurement.Tools.Compute.Implementations
{
    /// <summary>
    /// Контроллер вычисления измеряемых/неизмеряемых объемов
    /// Потом продумать, как отказаться от прокидывания углов
    /// </summary>
    public class MeasureController : IMeasureController
    {
        const double DELTA = 0.01;

        /// <summary>
        /// Вычисление измеряемых/неизмеряемых сущностей для всех баков и их датчиков
        /// </summary>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="referencedPitch">Опорный угол тангажа</param>
        /// <param name="referencedRoll">Опорный угол крена</param>
        public void CalcSensorsMeasures(List<TankModel> tanks)
        {
            foreach (var tank in tanks)
            {
                // Формируем данные по измеряемым (и неизмеряемым) уровням и объемам датчиков
                // Для каждого бака формируем данные по датчикам
                foreach (var sensor in tank.Sensors)
                {
                    sensor.SensorMeasuresList.Clear(); // Пока чистим так

                    CalcSensorMeasures(sensor);

                    // Тщательно продумать вопрос расчета данных по активным датчикам
                    //if (sensor.IsActiveSensor) // Считаем только для активных датчиков
                    //{
                        
                    //}
                }
            }
        }

        /// <summary>
        /// Вычисление измеряемых/неизмеряемых сущностей для группы датчиков
        /// Этот метод применяется тогда, когда сразу несколько датчиков меняют свои параметры
        /// </summary>
        /// <param name="sensorGroup"></param>
        /// <param name="referencedPitch"></param>
        /// <param name="referencedRoll"></param>
        public void CalcSensorGroupMeasures(List<SensorModel> sensorGroup)
        {
            foreach(var sensor in sensorGroup)
            {
                sensor.SensorMeasuresList.Clear(); // Пока чистим так

                CalcSensorMeasures(sensor);

                //if (sensor.IsActiveSensor) // Считаем только для активных датчиков
                //{
                    
                //}
            }
        }

        /// <summary>
        /// Вычисление неизмеряемых сущностей для датчика
        /// </summary>
        /// <param name="sensor">Датчик</param>
        /// <param name="referencedPitch">Опорный угол тангажа</param>
        /// <param name="referencedRoll">Опорный угол крена</param>
        public void CalcSensorMeasures(SensorModel sensor)
        {
            sensor.SensorMeasuresList.Clear(); // Перед вычислениями удаляем предыдущие данные датчика

            // Цикл по суперпозициям углов (определенных ранее при вызове тарировщика)
            for (int i = 0; i < sensor.FuelTank.TarResultList.Count; i++)
            {
                //// Создаем экземпляр данных по датчикам для суперпозиции углов тангажа и крена
                SensorMeasuresModel measures = new(sensor);

                // Вычисляем быстрые коэффициенты для углов
                double kPitch = Math.Round(sensor.FuelTank.TarResultList[i].Pitch.ToRadians(), 2);
                double kRoll  = Math.Round(sensor.FuelTank.TarResultList[i].Roll.ToRadians(), 2);

                Assimp.Vector3D bottomP = new((float)sensor.DownPoint.X,
                                              (float)sensor.DownPoint.Y,
                                              (float)sensor.DownPoint.Z);

                Assimp.Vector3D topP = new((float)sensor.UpPoint.X,
                                           (float)sensor.UpPoint.Y,
                                           (float)sensor.UpPoint.Z);

                PlaneModel plane = new((float)Math.Sin(kPitch),
                                      ((float)-Math.Cos(kPitch) * (float)Math.Cos(kRoll)),
                                      ((float)-Math.Sin(kRoll) * (float)Math.Cos(kPitch)), 0);

                // Создаем экземпляр измеряемых и неизмеряемых параметров датчика
                // Также считаем координату низа бака для относительных величин
                double tankLower = -plane.CountDistance(sensor.FuelTank.TarResultList[i].TankBottom);
                double tankUpper = -plane.CountDistance(sensor.FuelTank.TarResultList[i].TankTop);
                double lower     = -plane.CountDistance(bottomP);
                double upper     = -plane.CountDistance(topP);

                measures.TankModel = sensor.FuelTank;
                measures.Pitch = sensor.FuelTank.TarResultList[i].Pitch;
                measures.Roll  = sensor.FuelTank.TarResultList[i].Roll;
                measures.Lower = lower;
                measures.Upper = upper;
                measures.RelativeLower = lower - tankLower;
                measures.RelativeUpper = upper - tankLower;
                measures.VolumeDown = sensor.FuelTank.GetVolumeSlice(lower, sensor.FuelTank, i);
                measures.VolumeUp = sensor.FuelTank.TankVolume - sensor.FuelTank.GetVolumeSlice(upper, sensor.FuelTank, i);

                sensor.SensorMeasuresList.Add(measures); // Добавляем в список данных по датчику
            }
        }

        /// <summary>
        /// Вычисление неизмеряемых объемов бака
        /// </summary>
        /// <param name="tank">Топливный бак</param>
        /// <param name="pitch">Угол тангажа</param>
        /// <param name="roll">Угол крена</param>
        public void CalcTankUnmeasurableValues(CalculationModel model, TankModel tank, double pitch, double roll)
        {
            // Если датчиков в баке нет, то возвращаем нули
            if (tank.Sensors.Count == 0)
            {
                tank.DownUnmeasurableVolume = 0;
                tank.DownPercent = 0;
                tank.UpUnmeasurableVolume = 0;
                tank.UpPercent = 0;
                return;
            };

            // Ищем самый нижний и самый верхний датчики
            // Для их поиска создаем список датчиков с их характеристиками
            List<SensorData> sensorsList = new List<SensorData>();
            //int inactiveSensorCounter = 0;
            // Формируем данные датчиков
            foreach (var sensor in tank.Sensors)
            {
                // На случай, если потребуется выключать из учета неактивные датчики
                if (sensor.IsActiveSensor)
                {
                    sensorsList.Add(GetSensorData(sensor, pitch, roll));
                }
                else
                {
                    sensor.FuelTank.InactiveSensorCounter++;
                }
            }

            if (sensorsList.Count == 0) return;

            var low = sensorsList.Min(x => x.SensorLower);
            var up  = sensorsList.Max(x => x.SensorUpper);

            int angleIndex = model.AngleFields.FindIndex(p => Math.Abs(p.Pitch - pitch) < DELTA && Math.Abs(p.Roll - roll) < DELTA);

            var volumeDown = tank.GetVolumeSlice(low, tank, angleIndex);
            var volumeUp   = tank.TankVolume - tank.GetVolumeSlice(up, tank, angleIndex);

            tank.DownUnmeasurableVolume = Math.Round(volumeDown, 2);
            tank.DownPercent = Math.Round(volumeDown / tank.TankVolume * 100, 2);
            tank.UpUnmeasurableVolume = Math.Round(volumeUp, 2);
            tank.UpPercent = Math.Round(volumeUp / tank.TankVolume * 100, 2);
        }

        /// <summary>
        /// Получение низа и верха конкретного датчика для конкретных углов
        /// </summary>
        /// <param name="sensor">Датчик</param>
        /// <param name="pitch">Угол тангажа</param>
        /// <param name="roll">Угол крена</param>
        /// <returns></returns>
        public SensorData GetSensorData(SensorModel sensor, double pitch, double roll)
        {
            SensorData sd = new();

            // Вычисляем быстрые коэффициенты для углов
            double kPitch = Math.Round((pitch * Math.PI / 180), 2);
            double kRoll = Math.Round((roll * Math.PI / 180), 2);

            Vector3D bottomP = new(
                               (float)sensor.DownPoint.X,
                               (float)sensor.DownPoint.Y,
                               (float)sensor.DownPoint.Z);

            Vector3D topP = new(
                            (float)sensor.UpPoint.X,
                            (float)sensor.UpPoint.Y,
                            (float)sensor.UpPoint.Z);

            PlaneModel plane = new(
                               (float)Math.Sin(kPitch),
                               ((float)-Math.Cos(kPitch) * (float)Math.Cos(kRoll)),
                               ((float)-Math.Sin(kRoll)  * (float)Math.Cos(kPitch)), 0);

            double lower = -plane.CountDistance(bottomP);
            double upper = -plane.CountDistance(topP);

            sd.Sensor = sensor;
            sd.SensorLower = lower;
            sd.SensorUpper = upper;

            return sd;
        }
    }
}
