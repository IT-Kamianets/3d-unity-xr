using UnityEngine.XR;

namespace ITKamianets.Engine.XR
{
    /// <summary>Whether the game runs in a headset, and which one. False on PC without a headset.</summary>
    public static class XRStatus
    {
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
