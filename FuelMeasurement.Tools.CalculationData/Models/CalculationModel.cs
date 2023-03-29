using System.Collections.Generic;
using FuelMeasurement.Common.Enums;

namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Класс модели вычислителя
    /// </summary>
    public class CalculationModel
    {
        #region Параметры ветви проекта для тарировки и иных вычислений
        /// <summary>
        /// Поле углов ветви проекта
        /// </summary>
        public List<AnglesPair> AngleFields { get; set; }
        #endregion

        #region вычисляемые параметры
        /// <summary>
        /// Список массивов - ось X графика (объем)
        /// </summary>
        public List<double[]> ListSerieX { get; set; } = new();
        /// <summary>
        /// Список массивов - ось Y графика (погрешность или емкость)
        /// </summary>
        public List<double[]> ListSerieY { get; set; } = new();
        #endregion

        /// <summary>
        /// Текущая ветвь проекта
        /// </summary>
        public BranchModel CurrentBranch { get; set; }

        /// <summary>
        /// Применение отчета к типу объекта (бак или самолет)
        /// </summary>
        public ReportObjectsEnum CurrentReport { get; set; }

        #region Все, что касается топливных баков для расчетов
        /// <summary>
        /// Список топливных баков ветки проекта
        /// Здесь будут все топливные баки ветки
        /// </summary>
        public List<TankModel> BranchTanks { get; set; } = new();

        /// <summary>
        /// Массив топливных баков, для которых проведена тарировка
        /// </summary>
        public List<TankModel> SelectedTanks { get; set; } = new();

        /// <summary>
        /// Список списков топливных баков, разбитых по группам последовательности заправки
        /// </summary>
        public List<List<TankModel>> ListOfTanksList { get; } = new();
        #endregion

        /// <summary>
        /// Число активных датчиков в выбранных баках для расстановки
        /// </summary>
        public int ActiveSensorsInCurrentTanks { get; set; }

        /// <summary>
        /// Список датчиков в группе заправки или выработки
        /// </summary>
        public List<int> SensorsInGroup { get; set; } = new();

        /// <summary>
        /// Суммарный объем выбранных баков для расстановки
        /// </summary>
        public double SumVolumeOfCurrentTanks { get; set; }

        /// <summary>
        /// Конструктор модели вычислений - данный класс должен создаваться до момента вычислений (любых)
        /// </summary>
        /// <param name="project"></param>
        public CalculationModel(BranchModel branch)
        {
            CurrentBranch = branch;
            AngleFields = new List<AnglesPair>(); // Создаем поле углов

            SetAnglesField(); // Заполняем поле углов
        }

        /// <summary>
        /// Метод установки поля углов с учетом опорных углов
        /// Пока старый метод
        /// </summary>
        private void SetAnglesField()
        {
            if (CurrentBranch == null) return; // Нет ножек, нет и конфетки
            AngleFields.Clear(); // На всякий (что значит на всякий? обязательно!) случай очищаем поле углов

            // Начальное значение угла тангажа - прыгаем от минусов
            double immutablePitch = CurrentBranch.AnglesModel.ReferencedPitch;
            //decimal immutableRoll = AirplaneModel.AirplaneConfiguration.ReferencedRoll;

            // В цикле бежим по углам тангажа
            for (var p = CurrentBranch.AnglesModel.MinPitch; p <= CurrentBranch.AnglesModel.MaxPitch; p += CurrentBranch.AnglesModel.PitchStep)
            {
                // Начальное значение угла крена (для каждого перебираемого угла тангажа)
                double immutableRoll = CurrentBranch.AnglesModel.MinRoll;
                if (AngleFields.Count != 0)
                {
                    if (p < CurrentBranch.AnglesModel.MaxPitch &&
                        p + CurrentBranch.AnglesModel.PitchStep > CurrentBranch.AnglesModel.MaxPitch)
                    {
                        // Если следующий шаг привел бы в выходу за пределы макса по тангажу, то тутова работаем
                        SetRollSet(immutableRoll, p, false, true);

                        // И добавляем последний угол тангажа - он равен максимальному значению
                        SetRollSet(immutableRoll, CurrentBranch.AnglesModel.MaxPitch, false, true);
                    }
                    else // Если пока еще бежим в пределах мин-макс
                    {
                        // Если угол тангажа меньше, чем перебираемый угол и меньше, чем предыдущий угол плюс шаг
                        if (CurrentBranch.AnglesModel.ReferencedPitch <
                            immutablePitch + CurrentBranch.AnglesModel.PitchStep &&
                            CurrentBranch.AnglesModel.ReferencedPitch > p - CurrentBranch.AnglesModel.PitchStep)
                        {
                            // Всаживаем опорный угол тангажа
                            SetRollSet(immutableRoll, CurrentBranch.AnglesModel.ReferencedPitch, false, true);

                            // И всаживаем следующий по циклу
                            SetRollSet(immutableRoll, p, false, true);
                            immutablePitch = p;
                        }
                        else
                        {
                            // Опорный угол не представился возможным - работаем по циклу
                            SetRollSet(immutableRoll, p, false, true);
                            immutablePitch = p;
                        }
                    }
                }
                else
                {
                    // Стартовая точка по тангажу
                    SetRollSet(immutableRoll, p, false, true);
                    immutablePitch = p;
                }
            }

            //TODO Учесть опорные углы - пока не знаю как - тики в слайдере одинаковы (нужно переопределение контрола)
            // Определяем количество расчетных углов тангажа и крена
            CurrentBranch.AnglesModel.PitchQuantity = (int)((CurrentBranch.AnglesModel.MaxPitch - CurrentBranch.AnglesModel.MinPitch + CurrentBranch.AnglesModel.PitchStep) / CurrentBranch.AnglesModel.PitchStep);
            CurrentBranch.AnglesModel.RollQuantity = (int)((CurrentBranch.AnglesModel.MaxRoll - CurrentBranch.AnglesModel.MinRoll + CurrentBranch.AnglesModel.RollStep) / CurrentBranch.AnglesModel.RollStep);
        }

        /// <summary>
        /// Частный метод пробежки по углам крена
        /// По сути - копия цикла для тангажа
        /// </summary>
        /// <param name="immutableRoll"></param>
        /// <param name="p">Угол тангажа</param>
        /// <param name="pitchInsert">Вставлять ли опорный тангаж</param>
        /// <param name="firstNode">Первый ли узел</param>
        private void SetRollSet(double immutableRoll, double p, bool pitchInsert, bool firstNode)
        {
            for (var r = CurrentBranch.AnglesModel.MinRoll; r <= CurrentBranch.AnglesModel.MaxRoll; r += CurrentBranch.AnglesModel.RollStep)
            {
                if (!firstNode)
                {
                    if (r < CurrentBranch.AnglesModel.MaxRoll &&
                        r + CurrentBranch.AnglesModel.RollStep > CurrentBranch.AnglesModel.MaxRoll)
                    {
                        AnglesPair s = new()
                        {
                            Pitch = pitchInsert ? CurrentBranch.AnglesModel.ReferencedPitch : p,
                            Roll = r
                        };
                        AngleFields.Add(s);
                        immutableRoll = r;

                        // И добавляем последний угол крена
                        s = new AnglesPair
                        {
                            Pitch = pitchInsert ? CurrentBranch.AnglesModel.ReferencedPitch : p,
                            Roll = CurrentBranch.AnglesModel.MaxRoll
                        };
                        AngleFields.Add(s);
                    }
                    else
                    {
                        if (CurrentBranch.AnglesModel.ReferencedRoll <
                            immutableRoll + CurrentBranch.AnglesModel.RollStep &&
                            CurrentBranch.AnglesModel.ReferencedRoll > r - CurrentBranch.AnglesModel.RollStep)
                        {
                            AnglesPair s = new()
                            {
                                Pitch = pitchInsert ? CurrentBranch.AnglesModel.ReferencedPitch : p,
                                Roll = CurrentBranch.AnglesModel.ReferencedRoll
                            };
                            AngleFields.Add(s);

                            s = new AnglesPair()
                            {
                                Pitch = pitchInsert ? CurrentBranch.AnglesModel.ReferencedPitch : p,
                                Roll = r
                            };
                            AngleFields.Add(s);
                            immutableRoll = r;
                        }
                        else
                        {
                            AnglesPair s = new()
                            {
                                Pitch = pitchInsert ? CurrentBranch.AnglesModel.ReferencedPitch : p,
                                Roll = r
                            };
                            AngleFields.Add(s);
                            immutableRoll = r;
                        }
                    }
                }
                else
                {
                    AnglesPair s = new()
                    {
                        Pitch = p,
                        Roll = r
                    };
                    AngleFields.Add(s);
                    immutableRoll = r;
                    firstNode = false;
                }
            }
        }
    }
}