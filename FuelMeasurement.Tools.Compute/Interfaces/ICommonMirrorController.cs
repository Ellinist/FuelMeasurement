using FuelMeasurement.Tools.CalculationData.Models;

namespace FuelMeasurement.Tools.Compute.Interfaces
{
    /// <summary>
    /// Расчет МПИ по общему зеркалу топлива
    /// </summary>
    public interface ICommonMirrorController
    {
        /// <summary>
        /// Метод вычисления массивов МПИ для режима общего зеркала топлива (общий режим или режим заправки)
        /// </summary>
        /// <param name="project">Текущий проект</param>
        /// <param name="listX">Список массивов оси абсцисс (объем)</param>
        /// <param name="listY">Список массивов оси ординат (погрешность в %)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="sensorsNo">Количество активных датчиков во всех топливных баках</param>
        /// <param name="volume">Суммарный объем топливных баков</param>
        void ComputeCommonMirrorErrors(CalculationModel model);

        /// <summary>
        /// метод обновления МПИ по общему зеркалу топлива (общий режим или режим заправки)
        /// </summary>
        /// <param name="listX">Список массивов оси абсцисс (объем)</param>
        /// <param name="listY">Список массивов оси ординат (погрешность в %)</param>
        /// <param name="tanks">Список топливных баков</param>
        void UpdateCommonMirrorErrors();

        /// <summary>
        /// Метод вычисления массивов МПИ для режима общего зеркала топлива (режим выработки)
        /// </summary>
        /// <param name="project">Текущий проект</param>
        /// <param name="listX">Список массивов оси абсцисс (объем)</param>
        /// <param name="listY">Список массивов оси ординат (погрешность в %)</param>
        /// <param name="tanks">Список топливных баков</param>
        /// <param name="sensorsNo">Количество активных датчиков во всех топливных баках</param>
        /// <param name="volume">Суммарный объем топливных баков</param>
        void ComputeBackCommonMirrorErrors(CalculationModel model);

        /// <summary>
        /// Метод обновления массивов МПИ для режима общего зеркала топлива (режим выработки)
        /// </summary>
        /// <param name="listX">Список массивов оси абсцисс (объем)</param>
        /// <param name="listY">Список массивов оси ординат (погрешность в %)</param>
        /// <param name="tanks">Список топливных баков</param>
        void UpdateBackCommonMirrorErrors();
    }
}
