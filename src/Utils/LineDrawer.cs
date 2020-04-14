using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ToySerialController.Utils
{
    public class LineDrawer
    {
        private readonly Mesh _mesh;
        private readonly List<Vector3> _vertices;
        private readonly List<Color> _colors;

        public LineDrawer()
        {
            _mesh = new Mesh();
            _vertices = new List<Vector3>();
            _colors = new List<Color>();
        }

        public void PushLine(Vector3 start, Vector3 stop, Color color)
        {
            _vertices.Add(start);
            _vertices.Add(stop);
            _colors.Add(color);
            _colors.Add(color);
        }

        public void Draw(Material material, int layer = 0)
        {
            var idx = Enumerable.Range(0, _vertices.Count);

            _mesh.vertices = _vertices.ToArray();
            _mesh.colors = _colors.ToArray();
            _mesh.uv = idx.Select(_ => Vector2.zero).ToArray();
            _mesh.normals = idx.Select(_ => Vector3.zero).ToArray();
            _mesh.SetIndices(idx.ToArray(), MeshTopology.Lines, 0);
            _mesh.RecalculateBounds();

            Graphics.DrawMesh(_mesh, Matrix4x4.identity, material, layer);
        }

        public void Clear()
        {
            _vertices.Clear();
            _colors.Clear();
        }
    }
}