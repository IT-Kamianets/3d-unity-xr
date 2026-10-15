using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace ITKamianets.Engine.XR
{
    /// <summary>Small shared pieces for the rigs: controller markers, pointer rays, the UI's tracking origin.</summary>
    public static class XRVisuals
    {
        private static Material _lineMaterial;

        /// <summary>A small sphere at a controller, until a game gives the hands a model.</summary>
        public static GameObject Marker(Transform hand, float size = 0.06f)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Marker";
            Object.Destroy(marker.GetComponent<Collider>());
            marker.transform.SetParent(hand, false);
            marker.transform.localScale = Vector3.one * size;
            marker.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return marker;
        }

        /// <summary>A marker and a thin ray along the controller's forward, for pointing at menus.</summary>
        public static LineRenderer Pointer(Transform hand, float length)
        {
            Marker(hand, 0.04f);
            var line = new GameObject("Ray").AddComponent<LineRenderer>();
            line.transform.SetParent(hand, false);
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.forward * length);
            line.startWidth = 0.006f;
            line.endWidth = 0.002f;
            line.startColor = new Color(1f, 1f, 1f, 0.8f);
            line.endColor = new Color(1f, 1f, 1f, 0f);
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.sharedMaterial = LineMaterial;
            return line;
        }

        /// <summary>
        /// Tells the scene's Input System UI module where tracking space is, so controller poses
        /// become world-space rays at world-space canvases. <paramref name="onlyIfCurrent"/>: clear
        /// it only if it is still that origin (another rig may have taken over).
        /// </summary>
        public static void SetUITrackingOrigin(Transform origin, Transform onlyIfCurrent = null)
        {
            var module = EventSystem.current != null ? EventSystem.current.GetComponent<InputSystemUIInputModule>() : null;
            if (module == null)
            {
                return;
            }

            if (onlyIfCurrent != null && module.xrTrackingOrigin != onlyIfCurrent)
            {
                return;
            }

            module.xrTrackingOrigin = origin;
        }

        // Sprites/Default is always in builds (Always Included Shaders) and does vertex colors.
        private static Material LineMaterial => _lineMaterial != null ? _lineMaterial : _lineMaterial = new Material(Shader.Find("Sprites/Default")) { name = "XR Pointer" };
    }
}
