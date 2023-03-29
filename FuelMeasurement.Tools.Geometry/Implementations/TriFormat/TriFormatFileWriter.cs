using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assimp;
using FuelMeasurement.Tools.Geometry.Interfaces.TriFormat;

namespace FuelMeasurement.Tools.Geometry.Implementations.TriFormat
{
    public class TriFormatFileWriter : CustomWriter, ITriFormatFileWriter
    {
        /// <summary>
        /// Запись геометрии в формат TRI
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="scene">Модель сцены</param>
        /// <returns></returns>
        public override async Task<bool> Write(Stream stream, Scene scene)
        {
            ConvertSceneToTriFormat(scene, out List<Vector3D> vertices, out List<Face> faces, out List<Vector3D> normals);

            return await WriteTriFile(stream, vertices, faces, normals);
        }

        /// <summary>
        /// Конвертирую модель сцены в модель треугольников
        /// </summary>
        /// <param name="scene">Модель сцены</param>
        /// <param name="uniqVertices">вершины</param>
        /// <param name="uniqFaces">фэйсы</param>
        /// <param name="uniqNormals">нормали</param>
        private static void ConvertSceneToTriFormat(Scene scene, out List<Vector3D> uniqVertices,
            out List<Face> uniqFaces, out List<Vector3D> uniqNormals)
        {
            uniqVertices = new();
            uniqNormals = new();
            uniqFaces = new();

            List<int> newIndexes = new();

            var vertices = scene.Meshes[0].Vertices;

            var verticesArray = vertices.ToArray();

            // Нахожу индексы
            foreach (var vert in verticesArray)
            {
                newIndexes.Add(Array.IndexOf(verticesArray, vert));
            }

            // Нахожу уникальные вершины по индексам
            foreach (var index in newIndexes)
            {
                var vector = vertices[index];

                Vector3D? findVector = uniqVertices.FirstOrDefault(a => a == vector);

                if (findVector == null) { uniqVertices.Add(vector); }
            }

            // Переписываю face-ы
            for (int i = 0; i < scene.Meshes[0].Faces.Count; i++)
            {
                for (int j = 0; j < scene.Meshes[0].Faces[i].Indices.Count; j++)
                {
                    var index = scene.Meshes[0].Faces[i].Indices[j];

                    var vertex = vertices[index];

                    var newIndex = uniqVertices.IndexOf(vertex);

                    scene.Meshes[0].Faces[i].Indices[j] = newIndex;
                }
            }

            // Нахожу уникальные нормали
            for (int i = 0; i < scene.Meshes[0].Normals.Count; i += 3)
            {
                uniqNormals.Add(scene.Meshes[0].Normals[i]);
            }

            uniqFaces = scene.Meshes[0].Faces;
        }

        /// <summary>
        /// Запись в файл
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="vertices">вершины</param>
        /// <param name="faces">файсы</param>
        /// <param name="normals">нормали</param>
        /// <returns>результат записи</returns>
        private async Task<bool> WriteTriFile(Stream stream, List<Vector3D> vertices, List<Face> faces, List<Vector3D> normals)
        {
            using var writer = new StreamWriter(stream);

            await WriteLine(writer, string.Empty);

            await WriteLine(writer, $"{vertices.Count}");

            for (int i = 0; i < vertices.Count; i++)
            {
                await Task.Run(async () => await WriteLine(writer, VectorToString(vertices[i])));
            }

            await WriteLine(writer, string.Empty);

            await WriteLine(writer, $"{normals.Count}");

            for (int i = 0; i < normals.Count; i++)
            {
                await Task.Run(async () => await WriteLine(writer, VectorToString(normals[i])));
            }

            await WriteLine(writer, string.Empty);

            await WriteLine(writer, $"{faces.Count}");

            for (int i = 0; i < faces.Count; i++)
            {
                await Task.Run(async () => await WriteLine(writer, ListIntToString(faces[i].Indices)));
            }

            return true;
        }

        /// <summary>
        /// Преобразование вектора в строку
        /// </summary>
        /// <param name="vector">Вектор</param>
        /// <returns>Вектор в string формате</returns>
        private string VectorToString(Vector3D vector)
        {
            var x = vector.X.ToString(Nfi);
            var y = vector.Y.ToString(Nfi);
            var z = vector.Z.ToString(Nfi);

            return $"{x}{Delimiter}{y}{Delimiter}{z}";
        }

        /// <summary>
        /// Преобразование индексов в строку
        /// </summary>
        /// <param name="indices">Индексы</param>
        /// <returns>Строка</returns>
        private static string ListIntToString(List<int> indices)
        {
            var x = indices[0].ToString();
            var y = indices[1].ToString();
            var z = indices[2].ToString();

            return $"{x}{Delimiter}{y}{Delimiter}{z}";
        }

        /// <summary>
        /// Запись строки в файл
        /// </summary>
        /// <param name="writer">стрим врайтер</param>
        /// <param name="text">текст для записи</param>
        /// <returns></returns>
        private static async Task WriteLine(StreamWriter writer, string text)
        {
            await writer.WriteLineAsync(text);
        }
    }
}
