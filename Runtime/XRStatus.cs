using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace ITKamianets.Engine.XR
{
    /// <summary>Whether the game runs in a headset, and which one. False on PC without a headset.</summary>
    public static class XRStatus
    {
        /// <summary>
        /// True when the game should use its VR layout: a headset is running, or a simulated one
        /// (the XR Interaction Toolkit's Device Simulator in the Editor) is connected. Game code
        /// picks VR or flat-screen controls, HUD and menus from this.
        /// </summary>
        public static bool IsActive => IsHeadsetActive || InputSystem.GetDevice<XRHMD>() != null;

        public static bool IsHeadsetActive => XRSettings.isDeviceActive;

        /// <summary>The XR plug-in in use (e.g. "OpenXR Display"), or empty.</summary>
        public static string DeviceName => XRSettings.loadedDeviceName;

        /// <summary>The headset's model when known (e.g. "Oculus Quest 3"), or empty.</summary>
        public static string HeadsetModel
        {
            get
            {
                var head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
                return head.isValid ? head.name : string.Empty;
            }
        }
    }
}
