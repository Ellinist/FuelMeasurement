using FuelMeasurement.Tools.CalculationData.Models;
using System.Collections.Generic;

namespace FuelMeasurement.Tools.Compute.Interfaces
{
    /// <summary>
    /// Расчет погонной емкости
    /// </summary>
    public interface ICapacityController
    {
        CalculationModel Model { get; set; }

        #region Блок вычисления значений погонной емкости в обычном режиме
        /// <summary>
        /// Метод формирования списка кривых емкостных показаний датчиков
        /// </summary>
        /// <param name="capacityListSerieX"></param>
        /// <param name="capacityListSerieY"></param>
        /// <param name="tanks"></param>
        /// <param name="sensorsCounter"></param>
        /// <param name="sumVolume"></param>
        void SetSensorsCapacity(CalculationModel model);

        /// <summary>
        /// Метод обновления списка кривых емкостных показаний датчиков
        /// </summary>
        /// <param name="capacityListSerieX"></param>
        /// <param name="capacityListSerieY"></param>
        /// <param name="tanks"></param>
        /// <param name="sumVolume"></param>
        void UpdateSensorsCapacity();
        #endregion

        #region Блок вычисления значений погонной емкости в режиме заправки
        /// <summary>
        /// Метод формирования массивов погонной емкости в режиме заправки
        /// </summary>
        /// <param name="capacityListSerieX"></param>
        /// <param name="capacityListSerieY"></param>
        /// <param name="tanks"></param>
        /// <param name="sensorsCounter"></param>
        /// <param name="sumVolume"></param>
        void SetSensorsCapacityIn(CalculationModel model);

        /// <summary>
        /// Метод обновления массивов погонной емкости в режиме заправки
        /// </summary>
        /// <param name="capacityListSerieX"></param>
        /// <param name="capacityListSerieY"></param>
        /// <param name="tanks"></param>
        /// <param name="sumVolume"></param>
        void UpdateSensorsCapacityIn();
        #endregion

        #region Блок вычисления знаений погонной емкости в режиме выработки
        /// <summary>
        /// Метод формирования массивов погонной емкости в режиме выработки
        /// </summary>
        /// <param name="capacityListSerieX"></param>
        /// <param name="capacityListSerieY"></param>
        /// <param name="tanks"></param>
        /// <param name="sensorsCounter"></param>
        /// <param name="sumVolume"></param>
        void SetSensorsCapacityOut(CalculationModel model);

        /// <summary>
        /// Метод обновления массивов погонной емкости в режиме выработки
        /// </summary>
        /// <param name="capacityListSerieX"></param>
        /// <param name="capacityListSerieY"></param>
        /// <param name="tanks"></param>
        /// <param name="sumVolume"></param>
        void UpdateSensorsCapacityOut();
        #endregion

        /// <summary>
        /// Объявление метода вычисления массивов МПИ для конкретной комбинации углов тангажа и крена
        /// </summary>
        /// <param name="arrX"></param>
        /// <param name="arrY"></param>
        /// <param name="pitch"></param>
        /// <param name="roll"></param>
        /// <param name="tanks"></param>
        void ComputeCapacityArrays(ref double[] arrX, ref double[] arrY, double pitch, double roll, List<TankModel> tanks);

    }
}
