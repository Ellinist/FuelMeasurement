using System.Collections.Generic;
using Assimp;
using FuelMeasurement.Model.Models.GeometryModels;
using FuelMeasurement.Tools.Geometry.Interfaces.TxtFormat;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace FuelMeasurement.Tools.Geometry.Implementations.TxtFormat
{
    public class TxtFormatFileReader : CustomReader, ITxtFormatFileReader
    {
        /// <summary>
        /// Чтение из TXT файла
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Меш</returns>
        public override async Task<MeshModel> Read(Stream stream)
        {
            return await ReadFile(stream);
        }

        /// <summary>
        /// Чтение из TXT файла
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Меш</returns>
        private static async Task<MeshModel> ReadFile(Stream stream)
        {
            using var reader = new StreamReader(stream);

            string? line;

            List<string> allLinesFile = new();

            while ((line = await reader.ReadLineAsync()) != null)
            {
                allLinesFile.Add(line);
            }

            reader.Dispose();

            return await Task.Run(() => ReadAllLine(allLinesFile));
        }

        /// <summary>
        /// Чтение всех строки файла
        /// </summary>
        /// <param name="allLinesFile">Список строк</param>
        /// <returns>Меш</returns>
        private static MeshModel ReadAllLine(List<string> allLinesFile)
        {
            List<string> vertexes = new();
            List<string> normales = new();

            for (int i = 0; i < 4; i++)
            {
                allLinesFile.Remove(allLinesFile[0]);
            }

            for (int i = 0; i < allLinesFile.Count; i += 5)
            {
                vertexes.Add(allLinesFile[i + 1]);
                vertexes.Add(allLinesFile[i + 2]);
                vertexes.Add(allLinesFile[i + 3]);

                normales.Add(allLinesFile[i + 4]);
            }

            return CreateMeshModelInFile(vertexes, normales);
        }

        /// <summary>
        /// Создание меша из файла
        /// </summary>
        /// <param name="vertexes">вершины</param>
        /// <param name="normales">нормали</param>
        /// <returns>Меш</returns>
        private static MeshModel CreateMeshModelInFile(List<string> vertexes, List<string> normales)
        {
            MeshModel mesh = new();

            for (int i = 0; i < vertexes.Count; i += 3)
            {
                var index1 = i + 0;
                var index2 = i + 1;
                var index3 = i + 2;

                mesh.Triangles.Add(VectorsToTriangle(
                    StringArrayToVector3D(vertexes[index1].Split(Delimiter)),
                    StringArrayToVector3D(vertexes[index2].Split(Delimiter)),
                    StringArrayToVector3D(vertexes[index3].Split(Delimiter))));

                mesh.Faces.Add(VerticesToFace(index1, index2, index3));
            }

            for (int i = 0; i < normales.Count; i++)
            {
                var n = StringArrayToVector3D(normales[i].Split(Delimiter));

                mesh.Normales.Add(n);
                mesh.Normales.Add(n);
                mesh.Normales.Add(n);
            }

            return mesh;
        }

        /// <summary>
        /// Создание фэйса из индексов
        /// </summary>
        /// <param name="index1">1й индекс</param>
        /// <param name="index2">2й индекс</param>
        /// <param name="index3">3й индекс</param>
        /// <returns>Фэйс</returns>
        private static Face VerticesToFace(int index1, int index2, int index3)
        {
            var face = new Face();
            face.Indices.Add(index1);
            face.Indices.Add(index2);
            face.Indices.Add(index3);

            return face;
        }

        /// <summary>
        /// Создание треугольника из 3х векторов
        /// </summary>
        /// <param name="v1">1й вектор</param>
        /// <param name="v2">2й вектор</param>
        /// <param name="v3">3й вектор</param>
        /// <returns>Треугольник</returns>
        private static Triangle VectorsToTriangle(Vector3D v1, Vector3D v2, Vector3D v3)
        {
            return new Triangle(v1, v2, v3);
        }

        /// <summary>
        /// Преобразование массива строк в вектор
        /// </summary>
        /// <param name="array">массив строк</param>
        /// <returns>Вектор</returns>
        private static Vector3D StringArrayToVector3D(string[] array)
        {
            var p1 = float.Parse(array[0], CultureInfo.InvariantCulture.NumberFormat);
            var p2 = float.Parse(array[1], CultureInfo.InvariantCulture.NumberFormat);
            var p3 = float.Parse(array[2], CultureInfo.InvariantCulture.NumberFormat);

            return new Vector3D(p1, p2, p3);
        }
    }
}
