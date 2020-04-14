using Leap.Unity;
using System;
using UnityEngine;

namespace ToySerialController.UI
{
    public class UIInputBehaviour : MonoBehaviour
    {
        public event EventHandler<InputEventArgs> OnInput;

        protected void Update()
        {
            foreach (var v in Enum<KeyCode>.values)
            {
                if (Input.GetKeyUp(v))
                    OnInput?.Invoke(this, new InputEventArgs(v, false));
                if (Input.GetKeyDown(v))
                    OnInput?.Invoke(this, new InputEventArgs(v, true));
            }
        }
    }
}
