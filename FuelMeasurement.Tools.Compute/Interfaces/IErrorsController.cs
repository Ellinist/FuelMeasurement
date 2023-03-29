using FuelMeasurement.Tools.CalculationData.Models;
using System.Collections.Generic;

namespace FuelMeasurement.Tools.Compute.Interfaces
{
    /// <summary>
    /// Расчет методической погрешности измерений
    /// </summary>
    public interface IErrorsController
    {
        CalculationModel Model { get; set; }

        #region Блок методов вычисления МПИ в обычном режиме
        /// <summary>
        /// Метод формирования списка кривых МПИ для общего режима
        /// </summary>
        /// <param name="model">Модель вычислителя</param>
        void ComputeTankErrors(CalculationModel model);

        /// <summary>
        /// Метод обновления МПИ для общего режима
        /// </summary>
        void UpdateTankErrors();
        #endregion

        #region Блок вычисления МПИ для режима заправки
        /// <summary>
        /// Метод формирования массивов МПИ для режима заправки
        /// </summary>
        /// <param name="model">Модель вычислителя</param>
        void ComputeTankFuelInErrors(CalculationModel model);

        /// <summary>
        /// Метод обновления массивов МПИ для режима заправки
        /// </summary>
        void UpdateTankFuelInErrors();
        #endregion

        #region Блок вычисления МПИ в режиме выработки
        /// <summary>
        /// Метод формирования массивов МПИ для режима выработки
        /// </summary>
        /// <param name="model">Модель вычислителя</param>
        void ComputeTankFuelOutErrors(CalculationModel model);

        /// <summary>
        /// Метод обновления массивов МПИ для режима выработки
        /// </summary>
        void UpdateTankFuelOutErrors();
        #endregion

        /// <summary>
        /// Метод формирования массивов кривой погрешности для нескольких баков 
        /// </summary>
        /// <param name="arrX">Расчетный массив оси абсцисс (объем)</param>
        /// <param name="arrY">Расчетный массив оси ординат (погрешность в %)</param>
        /// <param name="pitch">Угол тангажа</param>
        /// <param name="roll">Угол крена</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="volume">Объем топливных баков вышеуказанного списка</param>
        void ComputeErrorArrays(ref double[] arrX, ref double[] arrY, double pitch, double roll, List<TankModel> tanks, double volume);
    }
}
