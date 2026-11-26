using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>Helpers for composing PLACEHOLDER geometry from primitives (replaced in PHASE 09).</summary>
    internal static class ModelBuilder
    {
        public static GameObject Prim(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            Vector3 localEuler = default,
            bool keepCollider = false)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localEulerAngles = localEuler;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().sharedMaterial = material;

            if (!keepCollider)
            {
                Collider collider = go.GetComponent<Collider>();
                if (collider != null) Object.DestroyImmediate(collider);
            }
            return go;
        }

        public static Transform Group(string name, Transform parent, Vector3 position = default, Vector3 euler = default)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localEulerAngles = euler;
            return go.transform;
        }
    }
}
