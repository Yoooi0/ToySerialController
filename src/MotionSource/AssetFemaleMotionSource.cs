using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;
using DebugUtils;

namespace ToySerialController.MotionSource
{
    public class AssetFemaleMotionSource : AbstractFemaleMotionSource
    {
        private Atom _assetAtom;
        private Component _assetComponent;

        private float _assetLength;
        private float _assetRadius;
        private Vector3 _assetPosition;
        private Vector3 _assetUp, _assetRight, _assetForward;

        private JSONStorableStringChooser AssetChooser, ComponentChooser, UpDirectionChooser;
        private JSONStorableFloat PositionOffsetSlider, LengthScaleSlider;

        private SuperController Controller => SuperController.singleton;

        public override Vector3 ReferencePosition => _assetPosition;
        public override Vector3 ReferenceUp => _assetUp;
        public override Vector3 ReferenceRight => _assetRight;
        public override Vector3 ReferenceForward => _assetForward;
        public override float ReferenceLength => _assetLength;
        public override float ReferenceRadius => _assetRadius;
        public override Vector3 ReferencePlaneNormal => _assetUp;

        public override void CreateUI(IUIBuilder builder)
        {
            AssetChooser = builder.CreateScrollablePopup("MotionSource:Asset", "Select Asset", null, null, AssetChooserCallback);
            ComponentChooser = builder.CreateScrollablePopup("MotionSource:Component", "Select Component", null, null, ComponentChooserCallback);
            UpDirectionChooser = builder.CreateScrollablePopup("MotionSource:UpDirection", "Select Up Direction", new List<string> { "+Up", "+Right", "+Forward", "-Up", "-Right", "-Forward" }, "+Up", null);
            PositionOffsetSlider = builder.CreateSlider("MotionSource:PositionOffset", "Position Offset", 0, 0, 1, true, true);
            LengthScaleSlider = builder.CreateSlider("MotionSource:LengthScale", "Length Scale", 1, 0, 1, true, true);

            base.CreateUI(builder);

            FindAssets();
        }

        public override void DestroyUI(IUIBuilder builder)
        {
            base.DestroyUI(builder);
            builder.Destroy(AssetChooser);
            builder.Destroy(ComponentChooser);
            builder.Destroy(UpDirectionChooser);
            builder.Destroy(PositionOffsetSlider);
            builder.Destroy(LengthScaleSlider);
        }

        public override void StoreConfig(JSONNode config)
        {
            base.StoreConfig(config);
            config.Store(AssetChooser);
            config.Store(ComponentChooser);
            config.Store(UpDirectionChooser);
            config.Store(PositionOffsetSlider);
            config.Store(LengthScaleSlider);
        }

        public override void RestoreConfig(JSONNode config)
        {
            base.RestoreConfig(config);
            config.Restore(AssetChooser);
            config.Restore(ComponentChooser);
            config.Restore(UpDirectionChooser);
            config.Restore(PositionOffsetSlider);
            config.Restore(LengthScaleSlider);

            FindAssets(AssetChooser.val);
            ComponentChooserCallback(ComponentChooser.val);
        }

        public override bool Update() => UpdateAsset() && base.Update();

        private bool UpdateAsset()
        {
            if (_assetAtom == null || _assetComponent == null || !_assetAtom.on)
                return false;

            if (DebugDraw.Enabled)
            {
                foreach (var c in _assetAtom.GetComponentsInChildren<MeshFilter>())
                    DebugDraw.DrawLocalBox(c.mesh.bounds, c.transform.position, c.transform.rotation, c == _assetComponent ? Color.green : Color.white);

                foreach (var c in _assetAtom.GetComponentsInChildren<SkinnedMeshRenderer>())
                    DebugDraw.DrawLocalBox(c.sharedMesh.bounds, c.transform.position, c.transform.rotation, c == _assetComponent ? Color.green : Color.white);
            }

            Bounds bounds;
            if (_assetComponent is MeshFilter)
                bounds = (_assetComponent as MeshFilter).mesh.bounds;
            else if (_assetComponent is SkinnedMeshRenderer)
                bounds = (_assetComponent as SkinnedMeshRenderer).sharedMesh.bounds;
            else
                return false;

            var transform = _assetComponent.transform;
            var newUp = transform.up;
            if (UpDirectionChooser.val == "+Up") newUp = transform.up;
            else if (UpDirectionChooser.val == "+Right") newUp = transform.right;
            else if (UpDirectionChooser.val == "+Forward") newUp = transform.forward;
            else if (UpDirectionChooser.val == "-Up") newUp = -transform.up;
            else if (UpDirectionChooser.val == "-Right") newUp = -transform.right;
            else if (UpDirectionChooser.val == "-Forward") newUp = -transform.forward;
            
            var obbCenter = transform.position + transform.rotation * (bounds.max + bounds.min) / 2;
            var projectedExtents = Vector3.Project(transform.rotation * bounds.extents, newUp);
            var projectedDiff = Vector3.Project(obbCenter - transform.position, newUp);

            var endPoint = transform.position + projectedDiff + projectedExtents * Mathf.Sign(Vector3.Dot(projectedExtents, newUp));
            var origin = Vector3.Lerp(transform.position, endPoint, PositionOffsetSlider.val);

            _assetLength = LengthScaleSlider.val * Vector3.Distance(origin, endPoint);

            var upRotation = Quaternion.FromToRotation(transform.up, newUp);
            _assetPosition = origin;
            _assetUp = upRotation * transform.up;
            _assetRight = upRotation * transform.right;
            _assetForward = upRotation * transform.forward;

            _assetRadius = Vector3.Project(transform.rotation * bounds.extents, _assetRight).magnitude;

            DebugDraw.DrawTransform(ReferencePosition, ReferenceUp, ReferenceRight, ReferenceForward, 0.15f);
            DebugDraw.DrawRay(ReferencePosition, ReferenceUp, ReferenceLength, Color.white);
            DebugDraw.DrawLine(ReferencePosition, TargetPosition, Color.yellow);

            return true;
        }

        private void FindAssets(string defaultUid = null)
        {
            var assetUids = Controller.GetAtoms()
                .Where(a => a.type == "CustomUnityAsset")
                .Select(a => a.uid)
                .ToList();

            if (!assetUids.Contains(defaultUid))
                defaultUid = assetUids.FirstOrDefault(uid => uid == _assetAtom?.uid) ?? assetUids.FirstOrDefault() ?? "None";

            assetUids.Insert(0, "None");

            AssetChooser.choices = assetUids;
            AssetChooserCallback(defaultUid);
        }

        protected void AssetChooserCallback(string s)
        {
            _assetAtom = Controller.GetAtomByUid(s);

            if(_assetAtom == null)
            {
                AssetChooser.valNoCallback = "None";
                ComponentChooser.choices = new List<string> { "None" };
                ComponentChooserCallback("None");
                return;
            }

            AssetChooser.valNoCallback = s;

            var meshFilters = _assetAtom.GetComponentsInChildren<MeshFilter>().Select(c => c.name) ?? Enumerable.Empty<string>();
            var skinnedMeshRenderers = _assetAtom.GetComponentsInChildren<SkinnedMeshRenderer>().Select(c => c.name) ?? Enumerable.Empty<string>();
            var componentNames = meshFilters.Concat(skinnedMeshRenderers).ToList();

            var defaultComponent = componentNames.FirstOrDefault() ?? "None";
            componentNames.Insert(0, "None");

            ComponentChooser.choices = componentNames;
            ComponentChooserCallback(defaultComponent);
        }

        protected void ComponentChooserCallback(string s)
        {
            _assetComponent = (Component)_assetAtom?.GetComponentByName<MeshFilter>(s)
                           ?? (Component)_assetAtom?.GetComponentByName<SkinnedMeshRenderer>(s);

            ComponentChooser.valNoCallback = _assetComponent == null ? "None" : s;
        }

        protected override void RefreshButtonCallback()
        {
            base.RefreshButtonCallback();
            FindAssets(AssetChooser.val);
        }
    }
}
