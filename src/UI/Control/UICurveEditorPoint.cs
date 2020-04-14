using System;
using ToySerialController.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ToySerialController.UI.Control
{
    public class UICurveEditorPoint : MaskableGraphic, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public event EventHandler<EventArgs> OnClick;
        public event EventHandler<EventArgs> OnDragBegin;
        public event EventHandler<EventArgs> OnDragEnd;
        public event EventHandler<EventArgs> OnDragging;

        private float _pointRadius = 10;
        private float _handleRadius = 7;
        private float _outHandleLength = 50;
        private float _inHandleLength = 50;
        private bool _isDraggingPoint = false;
        private bool _isDraggingOutHandle = false;
        private bool _isDraggingInHandle = false;
        private bool _showHandles = false;

        private int _handleMode = 0;    // 0 = both, 1 = free
        private int _inHandleMode = 0;  // 0 = constant, 1 = weighted
        private int _outHandleMode = 0; // 0 = constant, 1 = weighted

        private Color _lineColor = new Color(0.5f, 0.5f, 0.5f);
        private Color _inHandleColor = new Color(0, 0, 0);
        private Color _outHandleColor = new Color(0, 0, 0);

        private Vector2 _outHandlePosition = Vector2.right * 50;
        private Vector2 _inHandlePosition = Vector2.left * 50;

        public RectTransform draggingRect;

        public float pointRadius
        {
            get { return _pointRadius; }
            set { _pointRadius = value; SetVerticesDirty(); }
        }

        public float handlePointRadius
        {
            get { return _handleRadius; }
            set { _handleRadius = value; SetVerticesDirty(); }
        }

        public bool showHandles
        {
            get { return _showHandles; }
            set { _showHandles = value; SetVerticesDirty(); }
        }

        public int handleMode
        {
            get { return _handleMode; }
            set { _handleMode = value; SetOutHandlePosition(_outHandlePosition); SetVerticesDirty(); }
        }

        public int inHandleMode
        {
            get { return _inHandleMode; }
            set { _inHandleMode = value; SetInHandlePosition(_inHandlePosition); SetVerticesDirty(); }
        }

        public int outHandleMode
        {
            get { return _outHandleMode; }
            set { _outHandleMode = value; SetOutHandlePosition(_outHandlePosition); SetVerticesDirty(); }
        }

        public float outHandleLength
        {
            get { return _outHandleLength; }
            set { _outHandleLength = value; SetOutHandlePosition(_outHandlePosition); SetVerticesDirty(); }
        }

        public float inHandleLength
        {
            get { return _inHandleLength; }
            set { _inHandleLength = value; SetOutHandlePosition(_inHandlePosition); SetVerticesDirty(); }
        }

        public Vector2 outHandlePosition
        {
            get { return _outHandlePosition; }
            set { SetOutHandlePosition(value); SetVerticesDirty(); }
        }

        public Vector2 inHandlePosition
        {
            get { return _inHandlePosition; }
            set { SetInHandlePosition(value); SetVerticesDirty(); }
        }

        public Color lineColor
        {
            get { return _lineColor; }
            set { _lineColor = value; SetMaterialDirty(); }
        }

        public Color inHandleColor
        {
            get { return _inHandleColor; }
            set { _inHandleColor = value; SetMaterialDirty(); }
        }

        public Color outHandleColor
        {
            get { return _outHandleColor; }
            set { _outHandleColor = value; SetMaterialDirty(); }
        }

        protected void Awake()
        {
            if (draggingRect == null)
                draggingRect = transform.parent.GetComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2();
            rectTransform.anchorMax = new Vector2();
        }

        protected UIVertex[] CreateVbo(Vector2[] vertices, Color color)
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

            if (_showHandles)
            {
                DrawLine(vh, Vector2.zero, _outHandlePosition, 4, _lineColor);
                DrawLine(vh, Vector2.zero, _inHandlePosition, 4, _lineColor);
            }

            DrawDot(vh, Vector2.zero, _pointRadius, color);

            if (_showHandles)
            {
                DrawDot(vh, _outHandlePosition, _handleRadius, _outHandleColor);
                DrawDot(vh, _inHandlePosition, _handleRadius, _inHandleColor);
            }

            var size = showHandles ? (Math.Max(_outHandlePosition.magnitude, _inHandlePosition.magnitude) + _handleRadius) * 2 : _pointRadius * 2;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
        }

        private void DrawDot(VertexHelper vh, Vector2 position, float radius, Color color)
        {
            var segments = 10;
            var prev = position;
            for (var i = 0; i < segments + 1; i++)
            {
                var rad = Mathf.Deg2Rad * (i * (360f / segments));
                var pos0 = prev;
                var pos1 = position + new Vector2(radius * Mathf.Cos(rad), radius * Mathf.Sin(rad));
                prev = pos1;
                vh.AddUIVertexQuad(CreateVbo(new[] { pos0, pos1, position, position }, color));
            }
        }

        private void DrawLine(VertexHelper vh, Vector2 from, Vector2 to, float thickness, Color color)
        {
            var prev = from;
            var cur = to;
            var angle = Mathf.Atan2(cur.y - prev.y, cur.x - prev.x) * 180f / Mathf.PI;

            var v1 = prev + new Vector2(0, -thickness / 2);
            var v2 = prev + new Vector2(0, +thickness / 2);
            var v3 = cur + new Vector2(0, +thickness / 2);
            var v4 = cur + new Vector2(0, -thickness / 2);

            v1 = MathUtils.RotatePointAroundPivot(v1, prev, angle);
            v2 = MathUtils.RotatePointAroundPivot(v2, prev, angle);
            v3 = MathUtils.RotatePointAroundPivot(v3, cur, angle);
            v4 = MathUtils.RotatePointAroundPivot(v4, cur, angle);

            vh.AddUIVertexQuad(CreateVbo(new[] { v1, v2, v3, v4 }, color));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.pressPosition, eventData.pressEventCamera, out localPoint))
            {
                if (Vector2.Distance(Vector2.zero, localPoint) <= _pointRadius)
                {
                    _isDraggingPoint = true;
                    OnDragBegin?.Invoke(this, new EventArgs(eventData, isPointEvent: true));
                    SetDraggedPosition(eventData);
                }
                else if (Vector2.Distance(localPoint, _outHandlePosition) <= _handleRadius)
                {
                    _isDraggingOutHandle = true;
                    OnDragBegin?.Invoke(this, new EventArgs(eventData, isOutHandleEvent: true));
                    SetDraggedAngle(eventData);

                }
                else if (Vector2.Distance(localPoint, _inHandlePosition) <= _handleRadius)
                {
                    _isDraggingInHandle = true;
                    OnDragBegin?.Invoke(this, new EventArgs(eventData, isInHandleEvent: true));
                    SetDraggedAngle(eventData);
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDraggingPoint)
            {
                SetDraggedPosition(eventData);
                OnDragging?.Invoke(this, new EventArgs(eventData, isPointEvent: true));
            }
            else if (_isDraggingOutHandle)
            {
                SetDraggedAngle(eventData);
                OnDragging?.Invoke(this, new EventArgs(eventData, isOutHandleEvent: true));
            }
            else if (_isDraggingInHandle)
            {
                SetDraggedAngle(eventData);
                OnDragging?.Invoke(this, new EventArgs(eventData, isInHandleEvent: true));
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDraggingPoint)
                OnDragEnd?.Invoke(this, new EventArgs(eventData, isPointEvent: true));
            else if (_isDraggingOutHandle)
                OnDragEnd?.Invoke(this, new EventArgs(eventData, isOutHandleEvent: true));
            else if (_isDraggingInHandle)
                OnDragEnd?.Invoke(this, new EventArgs(eventData, isInHandleEvent: true));

            _isDraggingPoint = false;
            _isDraggingOutHandle = false;
            _isDraggingInHandle = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.pressPosition, eventData.pressEventCamera, out localPoint))
            {
                if (Vector2.Distance(Vector2.zero, localPoint) <= _pointRadius)
                    OnClick?.Invoke(this, new EventArgs(eventData, isPointEvent: true));
                else if (Vector2.Distance(localPoint, _outHandlePosition) <= _handleRadius)
                    OnClick?.Invoke(this, new EventArgs(eventData, isOutHandleEvent: true));
                else if (Vector2.Distance(localPoint, _inHandlePosition) <= _handleRadius)
                    OnClick?.Invoke(this, new EventArgs(eventData, isInHandleEvent: true));
                else
                    OnClick?.Invoke(this, new EventArgs(eventData));
            }

        }

        private void SetOutHandlePosition(Vector2 position)
        {
            if (position.x < 0)
                _outHandlePosition = Vector2.up * _outHandleLength;
            else
                _outHandlePosition = position.normalized * (_outHandleMode == 1 ? position.magnitude : _outHandleLength);

            if (_handleMode == 0)
            {
                if (_inHandleMode == 0 || position.x < 0)
                    _inHandlePosition = -_outHandlePosition.normalized * _inHandleLength;
                else
                    _inHandlePosition = -_outHandlePosition.normalized * _inHandlePosition.magnitude;
            }
        }

        private void SetInHandlePosition(Vector2 position)
        {
            if (position.x > 0)
                _inHandlePosition = Vector2.down * _inHandleLength;
            else
                _inHandlePosition = position.normalized * (_inHandleMode == 1 ? position.magnitude : _inHandleLength);

            if (_handleMode == 0)
            {
                if (_outHandleMode == 0 || position.x > 0)
                    _outHandlePosition = -_inHandlePosition.normalized * _outHandleLength;
                else
                    _outHandlePosition = -_inHandlePosition.normalized * _outHandlePosition.magnitude;
            }
        }

        private void SetDraggedPosition(PointerEventData eventData)
        {
            Vector3 worldPoint;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingRect, eventData.position, eventData.pressEventCamera, out worldPoint))
            {
                rectTransform.position = worldPoint;
                rectTransform.rotation = draggingRect.rotation;
            }

            var min = draggingRect.rect.min;
            var max = draggingRect.rect.max;
            var pos = rectTransform.localPosition;
            pos.x = Mathf.Clamp(pos.x, min.x, max.x);
            pos.y = Mathf.Clamp(pos.y, min.y, max.y);
            rectTransform.localPosition = pos;
        }

        private void SetDraggedAngle(PointerEventData eventData)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(draggingRect, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                var position = (localPoint + draggingRect.sizeDelta / 2) - rectTransform.anchoredPosition;
                if (_isDraggingOutHandle) SetOutHandlePosition(position);
                if (_isDraggingInHandle) SetInHandlePosition(position);

                SetVerticesDirty();
            }
        }

        public class EventArgs : System.EventArgs
        {
            public PointerEventData Data { get; private set; }
            public Vector2? LocalPosition { get; private set; }
            public bool IsPointEvent { get; private set ;}
            public bool IsOutHandleEvent  { get; private set; }
            public bool IsInHandleEvent { get; private set; }

            public EventArgs(PointerEventData data, bool isPointEvent = false, bool isOutHandleEvent = false, bool isInHandleEvent = false)
            {
                Data = data;
                IsPointEvent = isPointEvent;
                IsInHandleEvent = isInHandleEvent;
                IsOutHandleEvent = isOutHandleEvent;
            }
        }
    }
}
