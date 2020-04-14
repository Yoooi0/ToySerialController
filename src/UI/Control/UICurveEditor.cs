using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using ToySerialController.UI.Control;
using ToySerialController.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ToySerialController.UI
{
    public class UICurveEditor : JSONStorableParam
    {
        public readonly UIDynamic container;
        public readonly GameObject gameObject;

        private Color _pointColor = new Color(0.427f, 0.035f, 0.517f);
        private Color _selectedPointColor = new Color(0.682f, 0.211f, 0.788f);
        private Color _handleLineColor = new Color(0, 0, 0);
        private Color _handleLineColorFree = new Color(0.427f, 0.035f, 0.517f);
        private Color _inHandleColor = new Color(0, 0, 0);
        private Color _inHandleColorWeighted = new Color(0.427f, 0.035f, 0.517f);
        private Color _outHandleColor = new Color(0, 0, 0);
        private Color _outHandleColorWeighted = new Color(0.427f, 0.035f, 0.517f);
        private Color _lineColor = new Color(0.9f, 0.9f, 0.9f);
        private Color _backgroundColor = new Color(0.721f, 0.682f, 0.741f);

        private UILine _line;
        private AnimationCurve _curve;
        private UICurveEditorPoint _selectedPoint;
        private List<Keyframe> _defaultKeyframes;

        private List<UICurveEditorPoint> _points = new List<UICurveEditorPoint>();
        private int _evaluateCount = 200;

        public int evaluateCount
        {
            get { return _evaluateCount; }
            set { _evaluateCount = value; UpdateCurve(); }
        }

        public AnimationCurve curve
        {
            get { return _curve; }
            set { _curve = value; SetPointsFromKeyframes(value.keys.ToList()); }
        }

        public UICurveEditor(IUIBuilder builder, UIDynamic container, string name, float width, float height)
        {
            this.container = container;
            this.name = name;

            gameObject = new GameObject();
            gameObject.transform.SetParent(container.transform, false);

            var buttonHeight = 25;
            var mask = gameObject.AddComponent<RectMask2D>();
            mask.rectTransform.anchoredPosition = new Vector2(0, buttonHeight / 2);
            mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height - buttonHeight);

            var input = gameObject.AddComponent<UIInputBehaviour>();
            input.OnInput += OnInput;

            var backgroundContent = new GameObject();
            var canvasContent = new GameObject();

            backgroundContent.transform.SetParent(gameObject.transform, false);
            canvasContent.transform.SetParent(gameObject.transform, false);

            var backgroundImage = backgroundContent.AddComponent<Image>();
            backgroundImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            backgroundImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height - buttonHeight);
            backgroundImage.color = _backgroundColor;

            _line = canvasContent.AddComponent<UILine>();
            _line.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _line.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height - buttonHeight);
            _line.color = _lineColor;
            _line.lineThickness = 4;

            var mouseClick = canvasContent.AddComponent<UIMouseClickBehaviour>();
            mouseClick.OnClick += OnCanvasClick;

            var buttonGroup = new UIHorizontalGroup(container, 510, buttonHeight, new Vector2(0, 0), 5, idx => builder.CreateButtonEx());
            buttonGroup.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -(height - buttonHeight) / 2);
            var buttons = buttonGroup.items.Select(o => o.GetComponent<UIDynamicButton>()).ToList();

            foreach(var b in buttons)
            {
                b.buttonText.fontSize = 18;
                b.buttonColor = Color.white;
            }

            buttons[0].label = "Mode";
            buttons[1].label = "In Mode";
            buttons[2].label = "Out Mode";
            buttons[3].label = "Default";
            buttons[4].label = "Linear";

            buttons[0].button.onClick.AddListener(OnHandleModeButtonClick);
            buttons[1].button.onClick.AddListener(OnInHandleModeButtonClick);
            buttons[2].button.onClick.AddListener(OnOutHandleModeButtonClick);
            buttons[3].button.onClick.AddListener(SetValToDefault);
            buttons[4].button.onClick.AddListener(OnSetLinearButtonClick);

            _curve = new AnimationCurve();
            _defaultKeyframes = new List<Keyframe> { new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0) };
            SetValToDefault();
        }

        private UICurveEditorPoint CreatePoint(Vector2 position)
        {
            var pointObject = new GameObject();
            pointObject.transform.SetParent(_line.transform, false);

            var point = pointObject.AddComponent<UICurveEditorPoint>();
            point.draggingRect = _line.rectTransform;
            point.color = _pointColor;
            point.inHandleColor = _inHandleColor;
            point.outHandleColor = _outHandleColor;
            point.lineColor = _handleLineColor;

            point.OnDragBegin += OnPointBeginDrag;
            point.OnDragging += OnPointDragging;
            point.OnClick += OnPointClick;
            point.rectTransform.anchoredPosition = position;

            _points.Add(point);
            return point;
        }

        private void DestroyPoint(UICurveEditorPoint point)
        {
            _points.Remove(point);
            GameObject.Destroy(point.gameObject);
            UpdateCurve();
        }

        public void SetPointsFromKeyframes(List<Keyframe> keyframes)
        {
            var sizeDelta = _line.rectTransform.sizeDelta;

            while (_points.Count > keyframes.Count)
                DestroyPoint(_points.First());
            while (_points.Count < keyframes.Count)
                CreatePoint(new Vector2());

            for (var i = 0; i < keyframes.Count; i++)
            {
                var point = _points[i];
                var key = keyframes[i];
                point.rectTransform.anchoredPosition = new Vector2(key.time, key.value) * sizeDelta;

                if (key.inTangent != key.outTangent)
                    point.handleMode = 1;

                if (((int)key.weightedMode & 1) > 0) point.inHandleMode = 1;
                if (((int)key.weightedMode & 2) > 0) point.outHandleMode = 1;

                var outHandleNormal = (MathUtils.VectorFromAngle(Mathf.Atan(key.outTangent)) * sizeDelta).normalized;
                if(point.outHandleMode == 1 && i < keyframes.Count - 1)
                {
                    var x = key.outWeight * (keyframes[i + 1].time - key.time) * sizeDelta.x;
                    var y = x * (outHandleNormal.y / outHandleNormal.x);
                    var length = Mathf.Sqrt(x * x + y * y);
                    point.outHandlePosition = outHandleNormal * length;
                }
                else
                {
                    point.outHandlePosition = outHandleNormal * point.outHandleLength;
                }

                var inHandleNormal = -(MathUtils.VectorFromAngle(Mathf.Atan(key.inTangent)) * sizeDelta).normalized;
                if (point.inHandleMode == 1 && i > 0)
                {
                    var x = key.inWeight * (key.time - keyframes[i - 1].time) * sizeDelta.x;
                    var y = x * (inHandleNormal.y / inHandleNormal.x);
                    var length = Mathf.Sqrt(x * x + y * y);
                    point.inHandlePosition = inHandleNormal * length;
                }
                else
                {
                    point.inHandlePosition = inHandleNormal * point.inHandleLength;
                }
            }

            UpdateCurve();
        }

        private void SetSelectedPoint(UICurveEditorPoint point)
        {
            if (_selectedPoint != null)
            {
                _selectedPoint.color = _pointColor;
                _selectedPoint.showHandles = false;
                _selectedPoint = null;
            }

            if (point != null)
            {
                point.color = _selectedPointColor;
                point.showHandles = true;
                point.SetVerticesDirty();

                _selectedPoint = point;
            }
        }

        private void SetHandleMode(int mode)
        {
            if (_selectedPoint == null)
                return;

            _selectedPoint.handleMode = mode;
            _selectedPoint.lineColor = mode == 0 ? _handleLineColor : _handleLineColorFree;
            UpdateCurve();
        }

        private void SetOutHandleMode(int mode)
        {
            if (_selectedPoint == null)
                return;

            _selectedPoint.outHandleMode = mode;
            _selectedPoint.outHandleColor = mode == 0 ? _outHandleColor : _outHandleColorWeighted;
            UpdateCurve();
        }

        private void SetInHandleMode(int mode)
        {
            if (_selectedPoint == null)
                return;

            _selectedPoint.inHandleMode = mode;
            _selectedPoint.inHandleColor = mode == 0 ? _inHandleColor : _inHandleColorWeighted;
            UpdateCurve();
        }

        private void UpdateCurve()
        {
            var sizeDelta = _line.rectTransform.sizeDelta;

            _points.Sort(new UICurveEditorPointComparer());
            while (_curve.keys.Length > _points.Count)
                _curve.RemoveKey(0);

            for (var i = 0; i < _points.Count; i++)
            {
                var point = _points[i];

                var position = point.rectTransform.anchoredPosition / sizeDelta;

                var key = new Keyframe(position.x, position.y);
                key.weightedMode = (WeightedMode)(point.inHandleMode | (point.outHandleMode << 1));

                var outPosition = point.outHandlePosition / sizeDelta;
                var inPosition = point.inHandlePosition / sizeDelta;

                if (Math.Abs(inPosition.x) < 0.0001f)
                {
                    key.inTangent = Mathf.Infinity;
                    key.inWeight = 0f;
                }
                else
                {
                    key.inTangent = inPosition.y / inPosition.x;

                    var prev = i > 0 ? _points[i - 1] : null;
                    if (prev != null)
                    {
                        var prevPosition = prev.rectTransform.anchoredPosition / sizeDelta;
                        var dx = position.x - prevPosition.x;
                        key.inWeight = Mathf.Clamp(Mathf.Abs(inPosition.x / dx), 0f, 1f);
                    }
                }

                if (Math.Abs(outPosition.x) < 0.0001f)
                {
                    key.outTangent = Mathf.Infinity;
                    key.outWeight = 0f;
                }
                else
                {
                    key.outTangent = outPosition.y / outPosition.x;

                    var next = i < _points.Count - 1 ? _points[i + 1] : null;
                    if (next != null)
                    {
                        var nextPosition = next.rectTransform.anchoredPosition / sizeDelta;
                        var dx = nextPosition.x - position.x;
                        key.outWeight = Mathf.Clamp(Mathf.Abs(outPosition.x / dx), 0f, 1f);
                    }
                }

                if (i >= _curve.keys.Length)
                    _curve.AddKey(key);
                else
                    _curve.MoveKey(i, key);
            }

            var result = new List<Vector2>();
            for(var i = 0; i < _evaluateCount; i++)
            {
                var t = (float)i / (_evaluateCount - 1);
                var value = _curve.Evaluate(t);
                result.Add(new Vector2(t * sizeDelta.x, value * sizeDelta.y));
            }

            _line.points = result;
        }

        private void OnInput(object sender, InputEventArgs e)
        {
            if (_selectedPoint != null)
            {
                if (e.Pressed)
                {
                    if (e.Key == KeyCode.Delete)
                    {
                        DestroyPoint(_selectedPoint);
                        SetSelectedPoint(null);
                    }
                    else if(e.Key == KeyCode.Z)
                        SetInHandleMode(1 - _selectedPoint.inHandleMode);
                    else if (e.Key == KeyCode.X)
                        SetOutHandleMode(1 - _selectedPoint.outHandleMode);
                    else if (e.Key == KeyCode.C)
                        SetHandleMode(1 - _selectedPoint.handleMode);
                }
            }
        }

        private void OnCanvasClick(object sender, PointerEventArgs e)
        {
            if(e.Data.clickCount == 2)
            {
                Vector2 localPosition;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_line.rectTransform, e.Data.position, e.Data.pressEventCamera, out localPosition))
                    return;

                CreatePoint(localPosition + _line.rectTransform.sizeDelta / 2);
                UpdateCurve();
                SetSelectedPoint(null);
            }

            if (IsClickOutsidePoint(_selectedPoint, e.Data))
                SetSelectedPoint(null);
        }

        private void OnPointBeginDrag(object sender, UICurveEditorPoint.EventArgs e)
        {
            var p = sender as UICurveEditorPoint;
            if (_selectedPoint != p)
                SetSelectedPoint(p);
        }

        private void OnPointDragging(object sender, UICurveEditorPoint.EventArgs e) => UpdateCurve();

        private void OnPointClick(object sender, UICurveEditorPoint.EventArgs e)
        {
            var point = sender as UICurveEditorPoint;
            if (!e.Data.dragging) {
                if (e.IsPointEvent)
                    SetSelectedPoint(point);
                else if(!e.IsInHandleEvent && !e.IsOutHandleEvent)
                {
                    if(IsClickOutsidePoint(point, e.Data))
                        SetSelectedPoint(null);
                }
            }
        }

        private void OnHandleModeButtonClick()
        {
            if (_selectedPoint != null)
                SetHandleMode(1 - _selectedPoint.handleMode);
        }

        private void OnOutHandleModeButtonClick()
        {
            if (_selectedPoint != null)
                SetOutHandleMode(1 - _selectedPoint.outHandleMode);
        }

        private void OnInHandleModeButtonClick()
        {
            if (_selectedPoint != null)
                SetInHandleMode(1 - _selectedPoint.inHandleMode);
        }

        private void OnSetLinearButtonClick()
        {
            if (_selectedPoint == null)
                return;

            var idx = _points.IndexOf(_selectedPoint);
            var key = _curve.keys[idx];

            if (idx > 0)
            {
                var prev = _curve.keys[idx - 1];
                prev.outTangent = key.inTangent = (key.value - prev.value) / (key.time - prev.time);
                _curve.MoveKey(idx - 1, prev);
            }

            if (idx < _curve.keys.Length - 1)
            {
                var next = _curve.keys[idx + 1];
                next.inTangent = key.outTangent = (next.value - key.value) / (next.time - key.time);
                _curve.MoveKey(idx + 1, next);
            }

            _curve.MoveKey(idx, key);
            SetPointsFromKeyframes(_curve.keys.ToList());
        }

        private bool IsClickOutsidePoint(UICurveEditorPoint point, PointerEventData eventData)
        {
            if (point == null)
                return false;

            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_line.rectTransform, eventData.pressPosition, eventData.pressEventCamera, out localPoint))
            {
                var p = localPoint + _line.rectTransform.sizeDelta / 2;
                var c = point.rectTransform.anchoredPosition;
                var a = c + point.inHandlePosition;
                var b = c + point.outHandlePosition;

                if (MathUtils.DistanceToLine(p, c, a) > 20 && MathUtils.DistanceToLine(p, c, b) > 20)
                    return true;
            }

            return false;
        }
        public override bool StoreJSON(JSONClass jc, bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
		{
			bool flag = NeedsStore(jc, includePhysical, includeAppearance) || forceStore;
			if (flag)
			{
                for(var i = 0; i < _curve.keys.Length; i++)
                {
                    var k = _curve.keys[i];
                    jc[name][i] = $"{k.time}, {k.value}, {k.inTangent}, {k.outTangent}, {k.inWeight}, {k.outWeight}, {(int)k.weightedMode}";
                }
			}

			return flag;
		}

		public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
		{
			if (!NeedsRestore(jc, restorePhysical, restoreAppearance))
				return;

            if (jc[name] != null)
			{
                var keyframes = new List<Keyframe>();
                var array = jc[name].AsArray;

                for(var i = 0; i < array.Count; i++)
                {
                    var values = jc[name][i].Value.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).Select(s => float.Parse(s)).ToArray();
                    var key = new Keyframe(values[0], values[1], values[2], values[3], values[4], values[5]);
                    key.weightedMode = (WeightedMode)((int)(values[6]));
                    keyframes.Add(key);
                }

                SetPointsFromKeyframes(keyframes);
            }
			else if (setMissingToDefault)
			{
                SetValToDefault();
			}
		}

		public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
		    => RestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);

        public override void SetDefaultFromCurrent() => _defaultKeyframes = _curve.keys.ToList();
        public override void SetValToDefault() => SetPointsFromKeyframes(_defaultKeyframes);

        public float Evaluate(float t) => _curve.Evaluate(t);

        private class UICurveEditorPointComparer : IComparer<UICurveEditorPoint>
        {
            public int Compare(UICurveEditorPoint x, UICurveEditorPoint y)
                => Comparer<float>.Default.Compare(x.rectTransform.anchoredPosition.x, y.rectTransform.anchoredPosition.x);
        }
    }
}