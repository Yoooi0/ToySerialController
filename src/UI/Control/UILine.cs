using System.Collections.Generic;
using ToySerialController.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ToySerialController.UI
{
    public class UILine : MaskableGraphic, IDragHandler
    {
        private List<Vector2> _points;
        private Vector2 _margin;

        public float lineThickness = 2;
        public bool relativeSize = false;

        public List<Vector2> points
        {
            get { return _points; }
            set
            {
                _points = value;
                SetVerticesDirty();
            }
        }

        public Vector2 margin
        {
            get { return _margin; }
            set
            {
                _margin = value;
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (_points == null || _points.Count < 2)
                return;

            vh.Clear();
            var sizeX = rectTransform.rect.width;
            var sizeY = rectTransform.rect.height;
            var offsetX = -rectTransform.pivot.x * rectTransform.rect.width;
            var offsetY = -rectTransform.pivot.y * rectTransform.rect.height;

            if (!relativeSize)
            {
                sizeX = 1;
                sizeY = 1;
            }


            sizeX -= _margin.x;
            sizeY -= _margin.y;
            offsetX += _margin.x / 2f;
            offsetY += _margin.y / 2f;

            var prevV1 = Vector2.zero;
            var prevV2 = Vector2.zero;

            for (var i = 1; i < _points.Count; i++)
            {
                var prev = _points[i - 1];
                var cur = _points[i];
                prev = new Vector2(prev.x * sizeX + offsetX, prev.y * sizeY + offsetY);
                cur = new Vector2(cur.x * sizeX + offsetX, cur.y * sizeY + offsetY);

                var angle = Mathf.Atan2(cur.y - prev.y, cur.x - prev.x) * 180f / Mathf.PI;

                var v1 = prev + new Vector2(0, -lineThickness / 2);
                var v2 = prev + new Vector2(0, +lineThickness / 2);
                var v3 = cur + new Vector2(0, +lineThickness / 2);
                var v4 = cur + new Vector2(0, -lineThickness / 2);

                v1 = MathUtils.RotatePointAroundPivot(v1, prev, angle);
                v2 = MathUtils.RotatePointAroundPivot(v2, prev, angle);
                v3 = MathUtils.RotatePointAroundPivot(v3, cur, angle);
                v4 = MathUtils.RotatePointAroundPivot(v4, cur, angle);

                if (i > 1)
                    CreateVbo(new[] { prevV1, prevV2, v1, v2 });

                vh.AddUIVertexQuad(CreateVbo(new[] { v1, v2, v3, v4 }));

                prevV1 = v3;
                prevV2 = v4;
            }
        }

        private UIVertex[] CreateVbo(Vector2[] vertices)
        {
            var VboVertices = new UIVertex[4];
            for (var i = 0; i < vertices.Length; i++)
            {
                var vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = vertices[i];
                VboVertices[i] = vert;
            }
            return VboVertices;
        }

        public void OnDrag(PointerEventData eventData) { }
    }
}