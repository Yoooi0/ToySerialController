using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;
using DebugUtils;
using ToySerialController.Config;

namespace ToySerialController.MotionSource
{
    public abstract class AbstractAssetBase : IUIProvider, IConfigProvider
    {
        private Atom _assetAtom;
        private Component _assetComponent;

        private JSONStorableStringChooser AssetChooser, ComponentChooser, UpDirectionChooser;
        private JSONStorableFloat UpOffsetSlider, RightOffsetSlider, ForwardOffsetSlider;

        private SuperController Controller => SuperController.singleton;

        protected Vector3 Extents { get; private set; }

        public Vector3 Position { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 Forward { get; private set; }

        public virtual void CreateUI(IUIBuilder builder)
        {
            AssetChooser = builder.CreateScrollablePopup("MotionSource:Asset", "Select Asset", null, null, AssetChooserCallback);
            ComponentChooser = builder.CreateScrollablePopup("MotionSource:Component", "Select Component", null, null, ComponentChooserCallback);
            UpDirectionChooser = builder.CreateScrollablePopup("MotionSource:UpDirection", "Select Up Direction", new List<string> { "+Up", "+Right", "+Forward", "-Up", "-Right", "-Forward" }, "+Up", null);
            UpOffsetSlider = builder.CreateSlider("MotionSource:UpOffset", "Up Offset (%)", -0.5f, -1, 1, true, true, valueFormat: "P0");
            RightOffsetSlider = builder.CreateSlider("MotionSource:RightOffset", "Right Offset (%)", 0, -1, 1, true, true, valueFormat: "P0");
            ForwardOffsetSlider = builder.CreateSlider("MotionSource:ForwardOffset", "Forward Offset (%)", 0, -1, 1, true, true, valueFormat: "P0");

            FindAssets();
        }

        public virtual void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(AssetChooser);
            builder.Destroy(ComponentChooser);
            builder.Destroy(UpDirectionChooser);
            builder.Destroy(UpOffsetSlider);
            builder.Destroy(RightOffsetSlider);
            builder.Destroy(ForwardOffsetSlider);
        }

        public virtual void StoreConfig(JSONNode config)
        {
            config.Store(AssetChooser);
            config.Store(ComponentChooser);
            config.Store(UpDirectionChooser);
            config.Store(UpOffsetSlider);
            config.Store(RightOffsetSlider);
            config.Store(ForwardOffsetSlider);
        }

        public virtual void RestoreConfig(JSONNode config)
        {
            config.Restore(AssetChooser);
            config.Restore(ComponentChooser);
            config.Restore(UpDirectionChooser);
            config.Restore(UpOffsetSlider);
            config.Restore(RightOffsetSlider);
            config.Restore(ForwardOffsetSlider);

            FindAssets(AssetChooser.val);
            ComponentChooserCallback(ComponentChooser.val);
        }

        public virtual bool Update()
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
            if (UpDirectionChooser.val == "-Up")
            {
                Up = -transform.up;
                Right = -transform.right;
                Forward = -transform.forward;
            }
            else
            {
                if (UpDirectionChooser.val == "+Up") Up = transform.up;
                else if (UpDirectionChooser.val == "+Right") Up = transform.right;
                else if (UpDirectionChooser.val == "+Forward") Up = transform.forward;
                else if (UpDirectionChooser.val == "-Right") Up = -transform.right;
                else if (UpDirectionChooser.val == "-Forward") Up = -transform.forward;

                var upRotation = Quaternion.FromToRotation(transform.up, Up);
                Right = upRotation * transform.right;
                Forward = upRotation * transform.forward;
            }

            Extents = transform.rotation * bounds.extents;

            var offsetUp = Vector3.Project(Extents, Up) * UpOffsetSlider.val * 2;
            var offsetRight = Vector3.Project(Extents, Right) * RightOffsetSlider.val * 2;
            var offsetForward = Vector3.Project(Extents, Forward) * ForwardOffsetSlider.val * 2;
            Position = transform.position + transform.rotation * bounds.center + offsetUp + offsetRight + offsetForward;

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

        private void AssetChooserCallback(string s)
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

        private void ComponentChooserCallback(string s)
        {
            _assetComponent = (Component)_assetAtom?.GetComponentByName<MeshFilter>(s)
                           ?? (_assetAtom?.GetComponentByName<SkinnedMeshRenderer>(s));

            ComponentChooser.valNoCallback = _assetComponent == null ? "None" : s;
        }

        public void Refresh() => FindAssets(AssetChooser.val);
    }
}
