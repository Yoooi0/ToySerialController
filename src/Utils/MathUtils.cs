using UnityEngine;

namespace ToySerialController.Utils
{
    public static class MathUtils
    {
        public static Vector2 VectorFromAngle(float angle)
            => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        public static float DistanceToLine(Vector2 p, Vector2 a, Vector2 b)
        {
            var diff = b - a;
            var dist = diff.magnitude;
            if (dist < 0.00001f)
                return Vector2.Distance(p, a);

            var t = Mathf.Clamp(Vector2.Dot(p - a, diff.normalized), 0, dist);
            return Vector2.Distance(p, a + diff.normalized * t);
        }

        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
            => Quaternion.Euler(angles) * (point - pivot) + pivot;
        public static Vector2 RotatePointAroundPivot(Vector2 point, Vector2 pivot, float angle)
            => RotatePointAroundPivot(point, pivot, angle * Vector3.forward);
    }
}
