using FuelMeasurement.Tools.CalculationData.Models;
using System.Collections.Generic;

namespace FuelMeasurement.Tools.Compute.Interfaces
{
    /// <summary>
    /// Расчет неизмеряемых (измеряемых) объемов, длин и т.п.
    /// </summary>
    public interface IMeasureController
    {
        /// <summary>
        /// Вычисление измеряемых/неизмеряемых сущностей для всех баков и их датчиков
        /// </summary>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="referencedPitch">Опорный угол тангажа</param>
        /// <param name="referencedRoll">Опорный угол крена</param>
        void CalcSensorsMeasures(List<TankModel> tanks);

        /// <summary>
        /// Вычисление измеряемых/неизмеряемых сущностей для группы датчиков
        /// </summary>
        /// <param name="sensorGroup"></param>
        /// <param name="referencedPitch"></param>
        /// <param name="referencedRoll"></param>
        void CalcSensorGroupMeasures(List<SensorModel> sensorGroup);

        /// <summary>
        /// Вычисление неизмеряемых сущностей для датчика
        /// </summary>
        /// <param name="sensor">Датчик</param>
        /// <param name="referencedPitch">Опорный угол тангажа</param>
        /// <param name="referencedRoll">Опорный угол крена</param>
        void CalcSensorMeasures(SensorModel sensor);

        /// <summary>
        /// Вычисление неизмеряемых объемов бака
        /// </summary>
        /// <param name="tank">Топливный бак</param>
        /// <param name="pitch">Угол тангажа</param>
        /// <param name="roll">Угол крена</param>
        void CalcTankUnmeasurableValues(CalculationModel model, TankModel tank, double pitch, double roll);

        /// <summary>
        /// Получение низа и верха датчика для конкретных углов
        /// </summary>
        /// <param name="sensor">Датчик</param>
        /// <param name="pitch">Угол тангажа</param>
        /// <param name="roll">Угол крена</param>
        /// <returns></returns>
        SensorData GetSensorData(SensorModel sensor, double pitch, double roll);
    }
}
