using FuelMeasurement.Common.Enums;

namespace FuelMeasurement.Model.Models.GeometryModels
{
    public class FuelTankGeometryModel
    {
        public string Id { get; set; }

        public MeshModel? Mesh { get; set; }

        /// <summary>
        /// Формат файла
        /// </summary>
        public string InFileExtension { get; set; }

        /// <summary>
        /// Название топливного бака
        /// </summary>
        public string FuelTankName { get; set; }

        /// <summary>
        /// Путь к файлу
        /// </summary>
        public string FilePath { get; set; }

        public TankGeometryType Type { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public FuelTankGeometryModel(string inFileExtension, string fuelTankName, string filePath)
        {
            InFileExtension = inFileExtension;
            FuelTankName = fuelTankName;
            FilePath = filePath;
        }
    }
}
