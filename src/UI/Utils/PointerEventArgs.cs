using System;
using UnityEngine.EventSystems;

namespace ToySerialController.UI
{
    public class PointerEventArgs : EventArgs
    {
        public PointerEventData Data { get; private set; }
        public PointerEventArgs(PointerEventData data) { Data = data; }
    }
}
