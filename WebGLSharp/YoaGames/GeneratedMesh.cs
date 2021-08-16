using System.Collections.Generic;
using System.Numerics;
using static WebGLSharp.Geometry;

namespace WebGLSharp.YoaGames
{
    public class GeneratedMesh
    {
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Geometry.Face> faces = new List<Geometry.Face>();

        public void AddVertex(float px, float py, float pz, float nx, float ny, float nz, float u, float v)
        {
            positions.Add(new Vector3(0.03f * px, 0.03f * py, 0.03f * pz));
            normals.Add(new Vector3(nx, ny, nz));
            uvs.Add(new Vector2(u, v));
        }

        public void AddVertex(double d, double e, double f, double g, double h, double i, double j, double k)
        {
            AddVertex((float)d, (float)e, (float)f, (float)g, (float)h, (float)i, (float)j, (float)k);
        }

        public void AddTriangle(int i, int j, int k)
        {
            var verticies = new List<Vertex>();
            verticies.Add(new Vertex(positions[i], normals[i], uvs[i]));
            verticies.Add(new Vertex(positions[k], normals[k], uvs[k]));
            verticies.Add(new Vertex(positions[j], normals[j], uvs[j]));

            faces.Add(new Geometry.Face(verticies));
        }

        public Geometry ToGeometry() => new Geometry(faces);
        /*
        public void scalePositions(float scaleValue)
        {
            for (int i = 0; i < vertices.Count; i += 8)
            {
                float px = vertices[i);
                float py = vertices[i + 1);
                float pz = vertices[i + 2);

                vertices[i] = scaleValue * px;
                vertices[i + 1] = scaleValue * py;
                vertices[i + 2] = scaleValue * pz;
            }
        }

        public GeneratedMesh reverseIndexOrientation()
        {
            for (int i = 0; i < indices.Count; i += 3)
            {
                // int i1 = indices[i);
                int i2 = indices[i + 1];
                int i3 = indices[i + 2];

                indices[i + 1] = i3;
                indices[i + 2] = i2;
            }
            return this;
        }

        // TODO on actual mesh for dynamic?
        // scale position is src pos from func
        // scale value is displacement scale
        /*
        public void addDisplacement(MapPosition2Value func, float scalePosition, float scaleValue)
        {
            for (int i = 0; i < vertices.Count; i += 8)
            {
                float px = vertices[i];
                float py = vertices[i + 1];
                float pz = vertices[i + 2];
                float nx = vertices[i + 3];
                float ny = vertices[i + 4];
                float nz = vertices[i + 5];

                double value = func.getValue(px * scalePosition, py * scalePosition, pz * scalePosition);
                value *= scaleValue;
                vertices[i]= (float)(px - nx * value);
                vertices[i + 1]= (float)(py - ny * value);
                vertices[i + 2]= (float)(pz - nz * value);
            }
        }
        *
        public void merge(GeneratedMesh other)
        {
            // see howmany vertices we have now
            int offset = getVertexCount();

            for (int i = 0; i < other.vertices.Count; i++)
                vertices.Add(other.vertices[i]);

            for (int i = 0; i < other.indices.Count; i++)
                indices.Add(other.indices[i] + offset);
        }*/
    }
}
