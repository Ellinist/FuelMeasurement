using System.Collections.Generic;
using Assimp;
using FuelMeasurement.Model.Models.GeometryModels;
using FuelMeasurement.Tools.Geometry.Interfaces.TriFormat;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace FuelMeasurement.Tools.Geometry.Implementations.TriFormat
{
    public class TriFormatFileReader : CustomReader, ITriFormatFileReader
    {
        /// <summary>
        /// Чтение геометрии из TRI файла
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Меш</returns>
        public async override Task<MeshModel> Read(Stream stream)
        {
            return await ReadFile(stream);
        }

        /// <summary>
        /// Чтение геометрии из TRI файла
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Меш</returns>
        private static async Task<MeshModel> ReadFile(Stream stream)
        {
            using var reader = new StreamReader(stream);

            string line;

            List<string> allLinesFile = new();

            while ((line = await reader.ReadLineAsync()) != null)
            {
                allLinesFile.Add(line);
            }

            reader.Dispose();

            var result = await Task.Run(() => ReadAllLine(allLinesFile));

            return result;
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
            List<string> indexList = new();

            int startNo = 1;

            int vertexCount = int.Parse(allLinesFile[startNo]);
            startNo++;

            for (int i = 0; i < vertexCount; i++)
            {
                vertexes.Add(allLinesFile[startNo]);
                startNo++;
            }

            startNo++;

            int normalesCount = int.Parse(allLinesFile[startNo]);
            startNo++;

            for (int i = 0; i < normalesCount; i++)
            {
                normales.Add(allLinesFile[startNo]);
                startNo++;
            }

            startNo++;

            int trianglesCount = int.Parse(allLinesFile[startNo]);

            for (int i = 0; i < trianglesCount; i++)
            {
                startNo++;
                indexList.Add(allLinesFile[startNo]);
            }

            allLinesFile.Clear();

            return CreateMeshModelInFile(vertexes, normales, indexList);
        }

        /// <summary>
        /// Создание меша из файла
        /// </summary>
        /// <param name="vertexes">вершины</param>
        /// <param name="normales">нормали</param>
        /// <param name="indexList">индексы</param>
        /// <returns>Меш</returns>
        private static MeshModel CreateMeshModelInFile(List<string> vertexes, List<string> normales, List<string> indexList)
        {
            string[] spl;

            float[,] Normales = new float[normales.Count, 3];
            for (int i = 0; i < normales.Count; i++)
            {
                spl = normales[i].Split(Delimiter);
                for (int j = 0; j < 3; j++)
                {
                    Normales[i, j] = float.Parse(spl[j], CultureInfo.InvariantCulture.NumberFormat);
                }
            }

            float[,] Vertexes = new float[vertexes.Count, 3];
            for (int i = 0; i < vertexes.Count; i++)
            {
                spl = vertexes[i].Split(Delimiter);
                for (int j = 0; j < 3; j++)
                {
                    Vertexes[i, j] = float.Parse(spl[j], CultureInfo.InvariantCulture.NumberFormat);
                }
            }

            int[,] TrianglesIndices = new int[indexList.Count, 3];
            for (int i = 0; i < indexList.Count; i++)
            {
                spl = indexList[i].Split(Delimiter);
                for (int j = 0; j < 3; j++)
                {
                    TrianglesIndices[i, j] = int.Parse(spl[j], CultureInfo.InvariantCulture.NumberFormat);
                }
            }

            MeshModel mesh = new();
            List<Vector3D> vertexess = new();
            List<Vector3D> normalss = new();

            for (int i = 0; i < Vertexes.Length / 3; i++)
            {
                var item1 = Vertexes[i, 0];
                var item2 = Vertexes[i, 1];
                var item3 = Vertexes[i, 2];

                vertexess.Add(new Vector3D(item1, item2, item3));
            }

            for (int i = 0; i < TrianglesIndices.Length / 3; i++)
            {
                var index1 = TrianglesIndices[i, 0];
                var index2 = TrianglesIndices[i, 1];
                var index3 = TrianglesIndices[i, 2];

                var item1 = vertexess[index1];
                var item2 = vertexess[index2];
                var item3 = vertexess[index3];

                mesh.Triangles.Add(new Triangle(item1, item2, item3));

                Face face = new();
                face.Indices.Add(index1);
                face.Indices.Add(index2);
                face.Indices.Add(index3);

                mesh.Faces.Add(face);
            }

            for (int i = 0; i < Normales.Length / 3; i++)
            {
                var item1 = Normales[i, 0];
                var item2 = Normales[i, 1];
                var item3 = Normales[i, 2];

                normalss.Add(new Vector3D(item1, item2, item3));
            }

            for (int i = 0; i < normalss.Count; i++)
            {
                var item1 = normalss[i].X;
                var item2 = normalss[i].Y;
                var item3 = normalss[i].Z;

                mesh.Normales.Add(new Vector3D(item1, item2, item3));
            }

            return mesh;
        }
    }
}
