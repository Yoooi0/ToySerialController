using UnityEngine;
using UnityEngine.UI;

namespace ToySerialController.UI
{
    public class UICircle : MaskableGraphic
    {
        private int _segments = 10;
        private float _radius = 10;

        public int segments
        {
            get { return _segments; }
            set
            {
                _segments = value;
                SetVerticesDirty();
            }
        }
        public float radius
        {
            get { return _radius; }
            set
            {
                _radius = value;

                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _radius * 2);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _radius * 2);

                SetVerticesDirty();
            }
        }

        protected void Awake()
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _radius * 2);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _radius * 2);
        }

        protected UIVertex[] CreateVbo(Vector2[] vertices)
        {
            var vbo = new UIVertex[4];
            for (var i = 0; i < vertices.Length; i++)
            {
                var vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = vertices[i];
                vbo[i] = vert;
            }
            return vbo;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var prev = Vector2.zero;
            for (var i = 0; i < segments + 1; i++)
            {
                var rad = Mathf.Deg2Rad * (i * (360f / segments));
                var pos0 = prev;
                var pos1 = new Vector2(radius * Mathf.Cos(rad), radius * Mathf.Sin(rad));
                prev = pos1;
                vh.AddUIVertexQuad(CreateVbo(new[] { pos0, pos1, Vector2.zero, Vector2.zero }));
            }
        }
    }
}