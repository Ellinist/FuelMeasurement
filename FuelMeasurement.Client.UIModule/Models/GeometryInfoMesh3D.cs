using FuelMeasurement.Common.Enums;
using HelixToolkit.SharpDX.Core;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace FuelMeasurement.Client.UIModule.Models
{
    public class GeometryInfoMesh3D : MeshGeometry3D
    {
        public GeometryInfoMesh3D(
            MeshGeometry3D meshGeometry3D,
            string idMesh,
            MeshType meshType,
            string id
            )
        {
            Id = id;
            IdMesh = idMesh;
            MeshType = meshType;

            Positions = meshGeometry3D.Positions;
            Indices = meshGeometry3D.TriangleIndices;
            Normals = meshGeometry3D.Normals;
            TextureCoordinates = meshGeometry3D.TextureCoordinates;
            Tangents = meshGeometry3D.Tangents;
            BiTangents = meshGeometry3D.BiTangents;
        }

        public GeometryInfoMesh3D(
            string idMesh,
            MeshType meshType,
            string id
            )
        {
            Id = id;
            IdMesh = idMesh;
            MeshType = meshType;

            Positions = new Vector3Collection();
            Indices = new IntCollection();
            Normals = new Vector3Collection();
            TextureCoordinates = new Vector2Collection();
            Tangents = new Vector3Collection();
            BiTangents = new Vector3Collection();
        }

        public string Id
        { get; }

        public string IdMesh
        { get; set; }

        public MeshType MeshType
        { get; set; }

        public HitTestContext Context { get; set; }
        public Matrix ModelMatrix { get; set; }


        public override bool HitTest(HitTestContext context, Matrix modelMatrix, ref List<HitTestResult> hits, object originalSource)
        {
            Context = context;
            ModelMatrix = modelMatrix;
            return base.HitTest(context, modelMatrix, ref hits, originalSource);
        }

        public bool TryFindHits(Vector3 point, Vector3 vector, out List<HitTestResult> hits)
        {
            hits = new();

            var ray = new Ray(point, vector);
            Context.RayWS = ray;
            HitTest(Context, ModelMatrix, ref hits, this);

            if (hits.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
