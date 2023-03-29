using Assimp;
using FuelMeasurement.Model.Models.GeometryModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace FuelMeasurement.Tools.CalculationData.Models
{
    /// <summary>
    /// Модель топливного бака в классе вычислителя
    /// </summary>
    public class TankModel
    {
        /// <summary>
        /// Константа - дельта для сравнения вершин
        /// </summary>
        private const float Triangledelta = 0.01f; // Допустимая погрешность - составляет одну сотую миллиметра

        /// <summary>
        /// Идентификатор топливного бака в модели вычислителя
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Имя топливного бака в метавселенной
        /// </summary>
        public string TankName { get; set; }

        public Color Color { get; set; }

        ///// <summary>
        ///// Номер топливного бака в метавселенной
        ///// </summary>
        //public int TankNumber { get; set; } // Пока никак не формируется - думать!

        /// <summary>
        /// Список датчиков топливного бака
        /// </summary>
        public List<SensorModel> Sensors { get; set; } = new();

        /// <summary>
        /// Список мордочек
        /// </summary>
        public List<Face> FacesList { get; set; } = new();

        /// <summary>
        /// Список треугольников, определяющих модель топливного бака
        /// </summary>
        public Triangle[] TankTriangles { get; set; }

        /// <summary>
        /// Список нормалей треугольников (пока неясно, необходимо ли использовать)
        /// </summary>
        public List<Vector3D> Normales { get; set; } = new();

        /// <summary>
        /// Массив вершин треугольников топливного бака
        /// </summary>
        public Vector3D[] Vertices { get; set; }

        /// <summary>
        /// Список тарировочных данных по всему полю углов
        /// </summary>
        public List<TarResult> TarResultList { get; set; } = new();

        /// <summary>
        /// Объем топливного бака
        /// </summary>
        public double TankVolume { get; set; }

        /// <summary>
        /// Пустой конструктор для маппера
        /// </summary>
        public TankModel() { }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="tank"></param>
        /// <param name="id"></param>
        public TankModel(MeshModel tank, string name, string id)
        {
            Id = id;
            TankName = name;

            TankTriangles = new Triangle[tank.Triangles.Count];
            Normales = tank.Normales;
            var verticesCounter = 0; // Назначаем счетчик вершин ответственным за работу
            Vertices = new Vector3D[tank.Triangles.Count * 3];
            // Бежим по входным данным
            for (var i = 0; i < tank.Triangles.Count; i++)
            {
                FacesList.Add(tank.Faces[i]); // Добавляем мордочку
                var trVertices = new Vector3D[3];
                trVertices[0] = tank.Triangles[i].A;
                trVertices[1] = tank.Triangles[i].B;
                trVertices[2] = tank.Triangles[i].C;
                TankTriangles[i] = new Triangle(trVertices[0], trVertices[1], trVertices[2]);
                Vertices[verticesCounter + 0] = tank.Triangles[i].A;
                Vertices[verticesCounter + 1] = tank.Triangles[i].B;
                Vertices[verticesCounter + 2] = tank.Triangles[i].C;
                verticesCounter += 3; // Увеличиваем счетчик
            }
        }

        /// <summary>
        /// Метод получения тарировочной кривой
        /// </summary>
        /// <returns></returns>
        public TarResult GetCurve(double pitch, double roll, int nodes, double scale)
        {
            TarResult result = new();
            if (nodes < 2)
            {
                result.ResultAxisX = null;
                result.ResultAxisY = null;
                result.RelativeResultAxisY = null;
                return result;
            }
            // Вызываем метод построения зеркала топлива
            // ВНИМАНИЕ! Параметр D изначально равен 0 - нет смещения относительно центра координат
            var fuelMirror = GetFuelMirror(pitch, roll);
            // Вызов метода формирования срезов бака плоскостями зеркала топлива
            // (по количеству расчетных точек и по разнесению вершин бака)
            // Возвращаемый массив содержит в себе свободные члены общего уравнения плоскости - параметр D
            float[] plotCurveY = GetFuelSlices(result, fuelMirror, nodes, out Vector3D[] focusList);
            // Создаем два вектора для отображения расчетной тарировочной кривой (оси Ox и Oy)
            var plotCurveX = new double[nodes]; // Вектор значений по оси Ox
            // Открываем цикл расчета - бежим по количеству узлов (срезов)
            for (var sliceIndex = 0; sliceIndex < nodes; sliceIndex++)
            {
                plotCurveX[sliceIndex] = 0; // Присваиваем для текущей записи (для текущего узла) нулевое значение
                // Строим новое зеркало топлива - для выбранного среза - передаем в него значение свободного члена
                // Также в качестве аргументов передаем параметры A, B и C, которые определяются углами тангажа и крена
                PlaneModel currentPlane = new(fuelMirror.A, fuelMirror.B, fuelMirror.C, plotCurveY[sliceIndex]);
                // Бежим по всем треугольникам нашей модели топливного бака
                foreach (var triangle in TankTriangles)
                {
                    // Вызываем метод расчета объема гипотетической прирамиды для текущего среза и треугольника
                    // гипотетический потому, что пирамида может и не создаваться (в зависимости от условий)
                    // Суммируем полученный объем с предыдущими занесенными значениями для данного среза
                    plotCurveX[sliceIndex] += CountPyramidVolume(triangle, currentPlane, focusList[sliceIndex]);
                }
                plotCurveX[sliceIndex] /= scale; // Переводим кубические параметры в нужные единицы
                plotCurveX[sliceIndex] = plotCurveX[sliceIndex];
            }

            // Формируем вектор значений измеряемой величины топливного бака по количеству расчетных узлов
            for (var i = 0; i < nodes; i++)
            {
                result.ResultAxisY.Add((float)Math.Round(plotCurveY[i], 2));
                result.RelativeResultAxisY.Add(result.ResultAxisY[^1] - result.ResultAxisY[0]);
                result.ResultAxisX.Add((float)Math.Round(plotCurveX[i], 2));
            }
            return result;
        }

        /// <summary>
        /// Метод формирования массива свободных членов
        /// Метод высчитывает умозрительные разрезы топливного бака в зависимости от его геометрических параметров
        /// Расчет проводится по всем вершинам бака
        /// </summary>
        /// <param name="fuelMirror"></param>
        /// <param name="nodes">Количество расчетных точек</param>
        /// <param name="focusList">Возвращаемый параметр (список фокусных точек)</param>
        /// <param name="result"></param>
        /// <returns>Массив свободных членов</returns>
        public float[] GetFuelSlices(TarResult result, PlaneModel fuelMirror, int nodes, out Vector3D[] focusList)
        {
            // Массив свободных членов (срезов топливного бака плоскостью зеркала топлива)
            float[] freeMember = new float[nodes];

            // Создаем массив удаленности вершин от заданного зеркала топлива
            var vertexesDistanceY = new double[Vertices.Length]; // Это массив для расчета вершин по оси Oy

            // Массив перебираемых фокусов пирамид
            focusList = new Vector3D[nodes];

            // Пробегаем в цикле по всем вершинам модели топливного бака
            // и вычисляем индексы удаления вершин
            for (var i = 0; i < Vertices.Length; i++)
            {
                // Заполняем массив
                vertexesDistanceY[i] = -fuelMirror.CountDistance(Vertices[i]);
            }

            // Ищем самые удаленные вершины (в одну и в другую стороны) и находим их индексы
            var minVertexY = vertexesDistanceY.Min(); // Ищем самую удаленную в минус вершину по оси Oy (вертикаль)
            var minIndexY = vertexesDistanceY.ToList().IndexOf(minVertexY);
            var maxVertexY = vertexesDistanceY.Max(); // Ищем самую удаленную в плюс вершину по оси Oy (вертикаль)
            var maxIndexY = vertexesDistanceY.ToList().IndexOf(maxVertexY);

            result.TankBottom = Vertices[minIndexY];
            result.TankTop = Vertices[maxIndexY];

            // Финальная часть метода - формирование массива свободных членов
            // Нулевой свободный член (первая точка) равен смещению максимальной отрицательной вершины относительно зеркала топлива
            freeMember[0] = fuelMirror.GetFreeMember(Vertices[minIndexY]);
            focusList[0] = Vertices[minIndexY]; // Записываем стартовый фокус - для первого среза
            // Предпоследний свободный член равен смещению максимальной положительной вершины относительно зеркала топлива
            freeMember[nodes - 1] = fuelMirror.GetFreeMember(Vertices[maxIndexY]);
            focusList[nodes - 1] = Vertices[maxIndexY]; // Записываем конечный фокус - для последнего среза

            // Если число точек (срезов) больше двух (то есть, есть промежуточные срезы)
            // то для каждого такого среза формируем промежуточное значение пропорционально
            if (nodes <= 2) return freeMember; // Возвращаем массив свободных членов (срезов топливного бака)
            {
                // Идем циклом по всем промежуточным срезам (NB! на начальную и конечную точки!!!)
                for (int i = 1; i < nodes - 1; i++)
                {
                    // Считаем с использованием среднего арифметического
                    freeMember[i] = freeMember[0] + i * ((freeMember[nodes - 1] - freeMember[0]) / (nodes - 1));
                    focusList[i] = new Vector3D
                    (
                        focusList[0].X + (focusList[nodes - 1].X - focusList[0].X) * i / (nodes - 1),
                        focusList[0].Y + (focusList[nodes - 1].Y - focusList[0].Y) * i / (nodes - 1),
                        focusList[0].Z + (focusList[nodes - 1].Z - focusList[0].Z) * i / (nodes - 1)
                    );
                }
            }
            return freeMember; // Возвращаем массив свободных членов (срезов топливного бака)
        }

        /// <summary>
        /// Метод расчета объема пирамиды на основании треугольника
        /// </summary>
        /// <param name="tr">Треугольник</param>
        /// <param name="plane">Текущая плоскость зеркала топлива</param>
        /// <param name="focus">Фокусная точка (текущая)</param>
        /// <returns>Объем пирамиды по заданному треугольнику</returns>
        protected static float CountPyramidVolume(Triangle tr, PlaneModel plane, Vector3D focus)
        {
            // Вырожденный случай - все вершины треугольника лежат в одной с фокусом плоскости
            if (IsAllVertexesInPlane(tr, focus)) return 0.0f; // Пирамиду не создаем и не считаем

            // Считаем отношение вершин треугольника по отношению к плоскости зеркала топлива (расстояние)
            double vertex0 = plane.CountDistance(tr.Vertices[0]);
            double vertex1 = plane.CountDistance(tr.Vertices[1]);
            double vertex2 = plane.CountDistance(tr.Vertices[2]);
            if (vertex0 <= 0 && vertex1 <= 0 && vertex2 <= 0)  // Случай, когда все треугольники над зеркалом топлива или в нем
            {
                return 0.0f; // Ничего не делаем - треугольник выше уровня зеркала топлива или лежит в нем
            }

            #region Все треугольники лежат ниже зеркала топлива
            if (vertex0 >= 0 && vertex1 >= 0 && vertex2 >= 0)  // Случай, когда треугольники целиком под зеркалом топлива
            {
                return GetPyramidVolume(tr, focus); // Вычисляем объем пирамиды
            }
            #endregion

            #region Ситуации, когда под зеркалом топлива две вершины (работаем с двумя пирамидами)
            if (vertex0 < 0 && vertex1 > 0 && vertex2 > 0)     // Вершина 0 над зеркалом топлива
            {
                // Головная вершина - Vertex[0] *** Считаем объемы усеченной и полной пирамид
                return GetPyramidVolume(tr, focus) - GetTruncatedPyramidVolume(plane, tr, 0, 1, 2, focus); // Возвращаем посчитанный объем
            }

            if (vertex1 < 0 && vertex0 > 0 && vertex2 > 0)     // Вершина 1 над зеркалом топлива
            {
                // Головная вершина - Vertex[1] *** Считаем объемы усеченной и полной пирамид
                return GetPyramidVolume(tr, focus) - GetTruncatedPyramidVolume(plane, tr, 1, 2, 0, focus); // Возвращаем посчитанный объем
            }

            if (vertex2 < 0 && vertex0 > 0 && vertex1 > 0)     // Вершина 2 над зеркалом топлива
            {
                // Головная вершина - Vertex[2] *** Считаем объемы усеченной и полной пирамид
                return GetPyramidVolume(tr, focus) - GetTruncatedPyramidVolume(plane, tr, 2, 0, 1, focus); // Возвращаем посчитанный объем
            }
            #endregion

            #region Ситуация, когда под зеркалом топлива одна вершина
            if (vertex0 > 0 && vertex1 < 0 && vertex2 < 0) // Под зеркалом топлива вершина номер 0
            {
                // Головная вершина - Vertex[0] *** Считаем объем усеченной пирамиды
                return GetTruncatedPyramidVolume(plane, tr, 0, 1, 2, focus);
            }

            if (vertex1 > 0 && vertex0 < 0 && vertex2 < 0) // Под зеркалом топлива вершина номер 1
            {
                // Головная вершина - Vertex[1] *** Считаем объем усеченной пирамиды
                return GetTruncatedPyramidVolume(plane, tr, 1, 2, 0, focus);
            }

            if (vertex2 > 0 && vertex0 < 0 && vertex1 < 0) // Под зеркалом топлива вершина номер 2
            {
                // Головная вершина - Vertex[2] *** Считаем объем усеченной пирамиды
                return GetTruncatedPyramidVolume(plane, tr, 2, 0, 1, focus);
            }

            // Сюда никогда не должны дойти - но Студия потребовала возврат
            return 0.0f;
            #endregion
        }

        /// <summary>
        /// Расчет объема пирамиды
        /// </summary>
        /// <param name="triangle">Треугольник - основание пирамиды</param>
        /// <param name="focus">Фокус пирамиды</param>
        /// <returns>Объем пирамиды</returns>
        protected static float GetPyramidVolume(Triangle triangle, Vector3D focus)
        {
            // Проверка на совпадение вершин треугольника
            if (Math.Abs(triangle.Vertices[0].X - triangle.Vertices[1].X) < Triangledelta &&
                Math.Abs(triangle.Vertices[1].X - triangle.Vertices[2].X) < Triangledelta)
            {
                if (Math.Abs(triangle.Vertices[0].Y - triangle.Vertices[1].Y) < Triangledelta &&
                    Math.Abs(triangle.Vertices[1].Y - triangle.Vertices[2].Y) < Triangledelta)
                {
                    if (Math.Abs(triangle.Vertices[0].Z - triangle.Vertices[1].Z) < Triangledelta &&
                        Math.Abs(triangle.Vertices[1].Z - triangle.Vertices[2].Z) < Triangledelta)
                    {
                        // Все вершины треугольника совпадают
                        return 0.0f;
                    }
                }
            }

            Pyramid pyramid = new Pyramid(triangle, focus); // Создаем усеченную пирамиду
            return pyramid.Volume() * pyramid.Sign; // Вычисляем объем пирамиды с учетом смачиваемости поверхности и возвращаем посчитанный объем
        }

        /// <summary>
        /// Метод вычисления объема усеченной пирамиды
        /// </summary>
        /// <param name="plane">Плоскость зеркала топлива</param>
        /// <param name="tr">Усекаемый треугольник</param>
        /// <param name="indexV0">Головная вершина</param>
        /// <param name="indexV1">Вершина 1</param>
        /// <param name="indexV2">Вершина 2</param>
        /// <param name="focus">Фокусная точка</param>
        /// <returns>Объем усеченной пирамиды</returns>
        protected static float GetTruncatedPyramidVolume(PlaneModel plane, Triangle tr, int indexV0, int indexV1, int indexV2, Vector3D focus)
        {
            // Вершина с индексом indexV0 под зеркалом топлива - строим усеченную пирамиду (cоздаем вектора-сегменты)
            // Головная вершина - Vertex[indexV0]
            LineSegment segment1 = new(tr.Vertices[indexV0], tr.Vertices[indexV1]);
            LineSegment segment2 = new(tr.Vertices[indexV0], tr.Vertices[indexV2]);

            // Высчитываем новые точки усеченной пирамиды на плоскости зеркала топлива
            Vector3D point1 = plane.IntersectWithLineSegment(segment1);
            Vector3D point2 = plane.IntersectWithLineSegment(segment2);

            // Проверка на совпадение новых точек усеченной пирамиды
            if (Math.Abs((double)(point1.X - point2.X)) < Triangledelta &&
                Math.Abs((double)(point1.Y - point2.Y)) < Triangledelta &&
                Math.Abs((double)(point1.Z - point2.Z)) < Triangledelta)
            {
                return 0.0f;
            }

            // Создаем новый треугольник - основание усеченной пирамиды (Внимание! Знак копируем из головного треугольника)
            // Вершина усеченного треугольника - головная вершина Vertex[indexV0]
            Triangle truncatedTriangle = new Triangle(tr.Vertices[indexV0], point1, point2)
            {
                NormalVector = tr.NormalVector // Копирование нормали - знак смачиваемости поверхности
            };

            return GetPyramidVolume(truncatedTriangle, focus); // Возвращаем посчитанный объем усеченной пирамиды
        }

        /// <summary>
        /// Метод проверки, лежат ли все вершины (включая фокус) в одной плоскости
        /// </summary>
        /// <param name="triangle">Треугольник</param>
        /// <param name="focus">Фокус</param>
        /// <returns>Истина, если все вершины в одной плоскости</returns>
        protected static bool IsAllVertexesInPlane(Triangle triangle, Vector3D focus)
        {
            // Создаем плоскость по треугольнику
            var vec1 = triangle.Vertices[0] - triangle.Vertices[1];
            var vec2 = triangle.Vertices[0] - triangle.Vertices[2];
            if (PlaneModel.IsCollinear(vec1, vec2))
            {
                return true; // Вектора коллинеарны - плоскость невозможно создать
            }
            PlaneModel trianglePlane = new(triangle.Vertices[0], triangle.Vertices[1], triangle.Vertices[2]);
            return Math.Abs(trianglePlane.CountDistance(focus)) <= double.Epsilon;
        }

        /// <summary>
        /// Метод вычисления объема топливного бака
        /// </summary>
        /// <param name="coefficient">Коэффициент перевода в литры</param>
        /// <returns>Значение объема топливного бака</returns>
        public double GetVolume(float coefficient)
        {
            TarResult result = new();
            var mirror = GetFuelMirror(0.0, 0.0); // Плановое создание зеркала топлива
            var slices = GetFuelSlices(result, mirror, 2, out Vector3D[] focusList); // Углы тангажа и крена нулевые
            var currentPlane = new PlaneModel(mirror.A, mirror.B, mirror.C, slices[1]);
            float tempVolume = 0.0f;
            // Пробегаем по всем треугольникам для верхнего среза бака
            foreach (var triangle in TankTriangles)
            {
                // Вызываем метод расчета объема гипотетической прирамиды для текущего среза и треугольника
                // гипотетический потому, что пирамида может и не создаваться (в зависимости от условий)
                // Суммируем полученный объем с предыдущими занесенными значениями для данного среза
                tempVolume += CountPyramidVolume(triangle, currentPlane, focusList[1]);
            }

            return Math.Round(tempVolume / coefficient, 2); // Возвращаем значение объема
        }

        /// <summary>
        /// Метод построения зеркала топлива
        /// </summary>
        /// <param name="pitch">Угол тангажа</param>
        /// <param name="roll">Угол атаки</param>
        /// <returns>Плоскость зеркала топлива</returns>
        public PlaneModel GetFuelMirror(double pitch, double roll)
        {
            float pitchRadian = pitch.ToRadians();
            float rollRadian = roll.ToRadians();
            return new PlaneModel((float)Math.Sin(pitchRadian),
                             (float)(-Math.Cos(pitchRadian) * Math.Cos(rollRadian)),
                             (float)(-Math.Sin(rollRadian) * Math.Cos(pitchRadian)), 0.0f);
        }

        #region Неизмеряемые объемы бака в литрах и процентах
        public double DownUnmeasurableVolume;
        public double DownPercent;
        public double UpUnmeasurableVolume;
        public double UpPercent;
        #endregion

        /// <summary>
        /// Счетчик неактивных датчиков бака
        /// </summary>
        public int InactiveSensorCounter { get; set; }

        public TankGroupModel SelectedTankGroupInModel { get; set; }
        public TankGroupModel SelectedTankGroupOutModel { get; set; }

        /// <summary>
        /// Получение дельты объема, охваченного датчиком, для заданного индекса углов
        /// </summary>
        /// <returns></returns>
        public double GetDeltaVolume(double start, double stop, TankModel tank, int angleIndex)
        {
            return GetVolumeSlice(stop, tank, angleIndex) - GetVolumeSlice(start, tank, angleIndex);
        }

        /// <summary>
        /// Метод вычисления приращения объема
        /// </summary>
        /// <param name="angleIndex">Индекс комбинации углов</param>
        /// <param name="sliceValue">Величина рассчитываемого среза топливного бака</param>
        /// <param name="tank">Топливный бак</param>
        /// <returns></returns>
        public double GetVolumeSlice(double sliceValue, TankModel tank, int angleIndex)
        {
            int t = tank.TarResultList[angleIndex].ResultAxisY.FindIndex(item => item >= sliceValue);
            if (t == 0) t++;

            double sc = (sliceValue - tank.TarResultList[angleIndex].ResultAxisY[t - 1]) /
                        (tank.TarResultList[angleIndex].ResultAxisY[t] - tank.TarResultList[angleIndex].ResultAxisY[t - 1]);

            return (tank.TarResultList[angleIndex].ResultAxisX[t] - tank.TarResultList[angleIndex].ResultAxisX[t - 1]) * sc + tank.TarResultList[angleIndex].ResultAxisX[t - 1];
        }
    }
}
