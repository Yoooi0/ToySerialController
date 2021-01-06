using DebugUtils;
using SimpleJSON;
using System.Collections.Generic;
using ToySerialController.UI;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public class RangeTestMotionSource : IMotionSource
    {
        public Vector3 ReferencePosition { get; private set; }
        public Vector3 ReferenceUp { get; private set; }
        public Vector3 ReferenceRight { get; private set; }
        public Vector3 ReferenceForward { get; private set; }
        public float ReferenceLength { get; private set; }
        public float ReferenceRadius { get; private set; }
        public Vector3 ReferencePlaneNormal { get; private set; }
        public Vector3 TargetPosition { get; private set; }
        public Vector3 TargetUp { get; private set; }
        public Vector3 TargetRight { get; private set; }
        public Vector3 TargetForward { get; private set; }

        private UIGroup _group;
        private JSONStorableFloat SpeedSlider;
        private JSONStorableStringChooser AxisChooser, MotionTypeChooser;
        private float _time;
        private Vector3 _rangeStart, _rangeEnd;
        private Quaternion _referenceRotation;

        public RangeTestMotionSource()
        {
            var camera = Camera.main.transform;

            ReferenceLength = 0.1f;
            ReferenceRadius = 0.05f;
            _rangeStart = camera.position + camera.forward;
            _rangeEnd = camera.position + camera.forward + Vector3.up * ReferenceLength;

            TargetUp = Vector3.up;
            TargetRight = Vector3.right;
            TargetForward = Vector3.forward;
            TargetPosition = _rangeEnd;

            ReferencePosition = _rangeStart;
            ReferencePlaneNormal = Vector3.up;

            _time = 0;
        }

        public void CreateUI(IUIBuilder builder)
        {
            _group = new UIGroup(builder);
            SpeedSlider = _group.CreateSlider("MotionSource:Speed", "Speed", 1, 0, 10, true, true);
            AxisChooser = _group.CreateScrollablePopup("MotionSource:Axis", "Select Axis", new List<string> { "L0", "L1", "L2", "R0", "R1", "R2" }, "L0", AxisChooserCallback);
            MotionTypeChooser = _group.CreateScrollablePopup("MotionSource:Value", "Select Value", new List<string> { "Min", "Center", "Max", "Linear", "Smooth" }, "Min", AxisChooserCallback);

        }
        
        public void DestroyUI(IUIBuilder builder)
        {
            _group.Destroy();
        }

        public void RestoreConfig(JSONNode config) { }
        public void StoreConfig(JSONNode config) { }

        public bool Update()
        {
            var t = GetLerpTime(MotionTypeChooser.val, _time);

            var position = _rangeStart;
            if (AxisChooser.val == "L0") position = _rangeStart + ReferenceUp * Mathf.Lerp(0, ReferenceLength, t);
            if (AxisChooser.val == "L1") position = _rangeStart + ReferenceRight * Mathf.Lerp(-ReferenceLength, ReferenceLength, t);
            if (AxisChooser.val == "L2") position = _rangeStart + ReferenceForward * Mathf.Lerp(-ReferenceLength, ReferenceLength, t);

            ReferencePosition = Vector3.Lerp(ReferencePosition, position, Mathf.Lerp(0, 1, SpeedSlider.val / 10.0f));

            var rotation = Quaternion.identity;
            if (AxisChooser.val == "R0") rotation = Quaternion.Euler(0, Mathf.Lerp(0, 360, t), 0);
            if (AxisChooser.val == "R1") rotation = Quaternion.Euler(Mathf.Lerp(-90, 90, t), 0, 0);
            if (AxisChooser.val == "R2") rotation = Quaternion.Euler(0, 0, Mathf.Lerp(-90, 90, t));

            _referenceRotation = Quaternion.Lerp(_referenceRotation, rotation, Mathf.Lerp(0, 1, SpeedSlider.val / 10.0f));
            ReferenceUp = _referenceRotation * Vector3.up;
            ReferenceRight = _referenceRotation * Vector3.right;
            ReferenceForward = _referenceRotation * Vector3.forward;

            DebugDraw.DrawTransform(ReferencePosition, ReferenceUp, ReferenceRight, ReferenceForward, 0.05f);
            DebugDraw.DrawRay(ReferencePosition, ReferenceUp, ReferenceLength, Color.white);

            _time += SpeedSlider.val / 1000.0f;
            return true;
        }

        public float GetLerpTime(string s, float t)
        {
            if (s == "Min") return 0;
            if (s == "Center") return 0.5f;
            if (s == "Max") return 1;
            if (s == "Linear") return Mathf.Abs(((t % 1) - 0.5f) * 2);
            if (s == "Smooth") return (Mathf.Sin(t) + 1) / 2;
            return 0;
        }

        public void AxisChooserCallback(string s)
        {
            TargetUp = Vector3.up;
            TargetRight = Vector3.right;
            TargetForward = Vector3.forward;

            _time = 0;
        }
    }
}
