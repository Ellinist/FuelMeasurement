using System.IO;
using System.Threading.Tasks;
using Assimp;
using FuelMeasurement.Common.SupportedFileFormats;
using FuelMeasurement.Tools.Geometry.Interfaces.TxtFormat;

namespace FuelMeasurement.Tools.Geometry.Implementations.TxtFormat
{
    public class TxtFormatFileWriter : CustomWriter, ITxtFormatFileWriter
    {
        /// <summary>
        /// Запись в TXT файл 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="scene"></param>
        /// <returns></returns>
        public override async Task<bool> Write(Stream stream, Scene scene)
        {
            return await WriteTxtFile(stream, scene);
        }

        /// <summary>
        /// Запись в TXT файл
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="scene">Модель сцены</param>
        /// <returns>результат</returns>
        private async Task<bool> WriteTxtFile(Stream stream, Scene scene)
        {
            var mesh = scene.Meshes[0];

            using var writer = new StreamWriter(stream);

            await WriteLine(writer, FileTXT.FileHeader);
            await WriteLine(writer, FileTXT.FileHeaderVersion);
            await WriteLine(writer, FileTXT.FileHeaderString3);
            await WriteLine(writer, FileTXT.FileHeaderString4);

            int normalCount = 0;

            for (int i = 0; i < mesh.Vertices.Count; i += 3)
            {
                var index1 = i + 0;
                var index2 = i + 1;
                var index3 = i + 2;

                await WriteLine(writer, FileTXT.FileCountVertices);

                await WriteLine(writer, VectorToString(mesh.Vertices[index1]));

                await WriteLine(writer, VectorToString(mesh.Vertices[index2]));

                await WriteLine(writer, VectorToString(mesh.Vertices[index3]));

                if (mesh.Normals.Count == mesh.Vertices.Count)
                {
                    await WriteLine(writer, VectorToString(mesh.Normals[index1]));
                }
                else
                {
                    await WriteLine(writer, VectorToString(mesh.Normals[normalCount]));
                }

                normalCount++;
            }

            return true;
        }


        /// <summary>
        /// Преобразование вектора в строку
        /// </summary>
        /// <param name="vector">Вектор</param>
        /// <returns></returns>
        private string VectorToString(Vector3D vector)
        {
            var x = vector.X.ToString(Nfi);
            var y = vector.Y.ToString(Nfi);
            var z = vector.Z.ToString(Nfi);

            return $"{x}{Delimiter}{y}{Delimiter}{z}";
        }

        /// <summary>
        /// Запист строки 
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
