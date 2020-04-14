using System;
using UnityEngine;

namespace ToySerialController.Utils
{
    public static class DebugDraw
    {
        private static readonly LineDrawer _lineDrawer = new LineDrawer();
        private static readonly Material _material = new Material(Shader.Find("Sprites/Default"));

        public static bool Enabled { get; set; }

        public static void Clear() => _lineDrawer?.Clear();
        public static void Draw() => _lineDrawer?.Draw(_material);

        public static void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            if (!Enabled) return;
            _lineDrawer.PushLine(start, end, color);
        }
        public static void DrawRay(Vector3 position, Vector3 normal, float distance, Color color) => DrawLine(position, position + normal * distance, color);

        public static void DrawPoint(Vector3 position, Color color, float size = 0.003f)
        {
            var scale = Mathf.Clamp(Vector3.Distance(position, Camera.main.transform.position), 0, 5);
            DrawCircle(position, Camera.main.transform.forward, color, size * (1 + scale), 10);
        }

        public static void DrawTransform(Vector3 position, Vector3 up, Vector3 right, Vector3 forward, float size = 1)
        {
            DrawLine(position, position + forward * size, Color.blue);
            DrawLine(position, position + right * size, Color.red);
            DrawLine(position, position + up * size, Color.green);
        }

        public static void DrawTransform(Transform transform, Vector3 position, float size = 1) => DrawTransform(position, transform.up, transform.right, transform.forward, size);
        public static void DrawTransform(Transform transform, float size = 1) => DrawTransform(transform, transform.position, size);

        public static void DrawBox(Vector3 position, Quaternion rotation, Vector3 extents, Color color)
        {
            var p0 = position + rotation * new Vector3(-extents.x, -extents.y, -extents.z);
            var p1 = position + rotation * new Vector3(extents.x, extents.y, extents.z);
            var p3 = position + rotation * new Vector3(-extents.x, -extents.y, extents.z);
            var p4 = position + rotation * new Vector3(-extents.x, extents.y, -extents.z);
            var p5 = position + rotation * new Vector3(extents.x, -extents.y, -extents.z);
            var p6 = position + rotation * new Vector3(-extents.x, extents.y, extents.z);
            var p7 = position + rotation * new Vector3(extents.x, -extents.y, extents.z);
            var p8 = position + rotation * new Vector3(extents.x, extents.y, -extents.z);

            DrawLine(p6, p1, color);
            DrawLine(p1, p8, color);
            DrawLine(p8, p4, color);
            DrawLine(p4, p6, color);
            DrawLine(p3, p7, color);
            DrawLine(p7, p5, color);
            DrawLine(p5, p0, color);
            DrawLine(p0, p3, color);
            DrawLine(p6, p3, color);
            DrawLine(p1, p7, color);
            DrawLine(p8, p5, color);
            DrawLine(p4, p0, color);
        }

        public static void DrawBox(Vector3 position, Vector3 extents, Color color) => DrawBox(position, Quaternion.identity, extents, color);
        public static void DrawBox(Bounds bounds, Color color) => DrawBox((bounds.min + bounds.max) / 2, (bounds.max - bounds.min) / 2, color);
        public static void DrawBox(Bounds bounds, Vector3 position, Quaternion rotation, Color color) => DrawBox(position + rotation * (bounds.max + bounds.min) / 2, rotation, (bounds.max - bounds.min) / 2, color);

        public static void DrawEllipse(Vector3 position, Quaternion rotation, Color color, float radiusX, float radiusZ, int segments = 20)
        {
            if (segments <= 2)
                return;

            for (int i = 0, j = 1; j < segments + 1; i = j++)
            {
                var xi = Mathf.Sin(Mathf.Deg2Rad * i * (360f / segments)) * radiusX;
                var zi = Mathf.Cos(Mathf.Deg2Rad * i * (360f / segments)) * radiusZ;
                var xj = Mathf.Sin(Mathf.Deg2Rad * j * (360f / segments)) * radiusX;
                var zj = Mathf.Cos(Mathf.Deg2Rad * j * (360f / segments)) * radiusZ;

                DrawLine(position + rotation * new Vector3(xi, 0, zi), position + rotation * new Vector3(xj, 0, zj), color);
            }
        }

        public static void DrawEllipse(Vector3 position, Vector3 normal, Color color, float radiusX, float radiusZ, int segments = 20) => DrawEllipse(position, Quaternion.FromToRotation(Vector3.up, normal), color, radiusX, radiusZ, segments);

        public static void DrawCircle(Vector3 position, Vector3 normal, Color color, float radius, int segments = 20) => DrawEllipse(position, normal, color, radius, radius, segments);
        public static void DrawCircle(Vector3 position, Quaternion rotation, Color color, float radius, int segments = 20) => DrawEllipse(position, rotation, color, radius, radius, segments);

        public static void DrawRectangle(Vector3 position, Vector3 normal, Color color, float width, float height) => DrawEllipse(position, normal, color, width, height, 4);
        public static void DrawRectangle(Vector3 position, Quaternion rotation, Color color, float width, float height) => DrawEllipse(position, rotation, color, width, height, 4);

        public static void DrawSquare(Vector3 position, Vector3 normal, Color color, float size) => DrawEllipse(position, normal, color, size, size, 4);
        public static void DrawSquare(Vector3 position, Quaternion rotation, Color color, float size) => DrawEllipse(position, rotation, color, size, size, 4);
    }
}