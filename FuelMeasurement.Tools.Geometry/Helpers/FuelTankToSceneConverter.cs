using Assimp;
using FuelMeasurement.Model.Models.GeometryModels;

namespace FuelMeasurement.Tools.Geometry.Helpers
{
    public static class FuelTankToSceneConverter
    {
        private static readonly string _nodeName = "Root";
        private static readonly string _materialName = "MyMaterial";

        /// <summary>
        /// Метод конвертации геометрии в сцену
        /// </summary>
        /// <param name="meshModel">Геометрия</param>
        /// <returns>Сцена</returns>
        public static Scene ConvertFuelTankGeometryModelToScene(MeshModel meshModel)
        {
            Mesh mesh = new("", PrimitiveType.Triangle);

            Scene scene = new();
            scene.RootNode = new Node(_nodeName);

            for (int i = 0; i < meshModel.Triangles.Count; i++)
            {
                mesh.Vertices.Add(new Vector3D(
                    (meshModel.Triangles[i].A.X),
                    (meshModel.Triangles[i].A.Y),
                    (meshModel.Triangles[i].A.Z))
                    );

                mesh.Vertices.Add(new Vector3D(
                    (meshModel.Triangles[i].B.X),
                    (meshModel.Triangles[i].B.Y),
                    (meshModel.Triangles[i].B.Z))
                    );

                mesh.Vertices.Add(new Vector3D(
                    (meshModel.Triangles[i].C.X),
                    (meshModel.Triangles[i].C.Y),
                    (meshModel.Triangles[i].C.Z))
                    );

                mesh.MaterialIndex = 0;
            }

            foreach (var normal in meshModel.Normales)
            {
                mesh.Normals.Add(normal);
            }

            foreach (var face in meshModel.Faces)
            {
                mesh.Faces.Add(face);
            }

            Material mat = new()
            {
                Name = _materialName,
                //ColorDiffuse = new Color4D(100f, 25f, 75f, 55f),
                //ColorAmbient = new Color4D(100f, 25f, 75f, 55f),
                //ColorEmissive = new Color4D(100f, 25f, 75f, 55f),
            };

            scene.Materials.Add(mat);

            scene.RootNode.MeshIndices.Add(0);
            scene.Meshes.Add(mesh);
            return scene;
        }

        /// <summary>
        /// Метод конвертации сцены в геометрию
        /// </summary>
        /// <param name="scene">Сцена</param>
        /// <returns>Геометрия</returns>
        public static MeshModel ConvertSceneToFuelTankModel(Scene scene)
        {
            MeshModel meshModel = new();

            foreach (var mesh in scene.Meshes)
            {
                if (mesh.Vertices.Count != 0)
                {
                    for (int i = 0; i < mesh.Vertices.Count; i += 3)
                    {
                        int index1 = i + 0;
                        int index2 = i + 1;
                        int index3 = i + 2;

                        Face face = new();
                        face.Indices.Add(index1);
                        face.Indices.Add(index2);
                        face.Indices.Add(index3);
                        meshModel.Faces.Add(face);

                        meshModel.Triangles.Add(new Triangle(
                            new Vector3D(mesh.Vertices[index1].X, mesh.Vertices[index1].Y, mesh.Vertices[index1].Z),
                            new Vector3D(mesh.Vertices[index2].X, mesh.Vertices[index2].Y, mesh.Vertices[index2].Z),
                            new Vector3D(mesh.Vertices[index3].X, mesh.Vertices[index3].Y, mesh.Vertices[index3].Z))
                            );
                    }
                }

                for (int i = 0; i < mesh.Normals.Count; i++)
                {
                    meshModel.Normales.Add(mesh.Normals[i]);
                }
            }

            // Для Алексея - здесь я принудительно формирую меш бака
            meshModel.TankType = Common.Enums.MeshType.FuelTank;

            return meshModel;
        }
    }
}
