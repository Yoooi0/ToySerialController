using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ToySerialController.Utils
{
    public static class ComponentCache
    {
        private static Dictionary<Component, Dictionary<string, Component>> _components = new Dictionary<Component, Dictionary<string, Component>>();
        private static Dictionary<Atom, Dictionary<string, Rigidbody>> _rigidbodies = new Dictionary<Atom, Dictionary<string, Rigidbody>>();

        public static T GetComponent<T>(Component parent, string name) where T : Component
        {
            Dictionary<string, Component> dictionary;
            if(_components.TryGetValue(parent, out dictionary))
            {
                Component component;
                if (dictionary.TryGetValue(name, out component))
                    return (T) component;
            }

            var result = parent.GetComponentsInChildren<T>()?.FirstOrDefault(c => c.name == name);
            if (result == null)
                return null;

            if (dictionary == null)
                _components.Add(parent, new Dictionary<string, Component>());

            _components[parent][name] = result;
            return result;
        }

        public static Rigidbody GetRigidbody(Atom parent, string name)
        {
            Dictionary<string, Rigidbody> dictionary;
            if (_rigidbodies.TryGetValue(parent, out dictionary))
            {
                Rigidbody rigidbody;
                if (dictionary.TryGetValue(name, out rigidbody))
                    return rigidbody;
            }

            var result = parent.rigidbodies?.FirstOrDefault(b => b.name == name);
            if (result == null)
                return null;

            if (dictionary == null)
                _rigidbodies.Add(parent, new Dictionary<string, Rigidbody>());

            _rigidbodies[parent][name] = result;
            return result;
        }

        public static void Clear()
        {
            _components.Clear();
            _rigidbodies.Clear();
        }
    }
}
