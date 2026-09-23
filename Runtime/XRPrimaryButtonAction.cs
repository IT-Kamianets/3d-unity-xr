using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace ITKamianets.Engine.XR
{
    /// <summary>
    /// Fires OnPressed once per press of the primary button (e.g. A/X) on the given
    /// controller. Generic input primitive -- has no idea what the button does.
    /// </summary>
    public class XRPrimaryButtonAction : MonoBehaviour
    {
        [SerializeField] private XRNode controllerNode = XRNode.RightHand;
        public UnityEvent OnPressed;

        private bool _wasPressed;

        private void Update()
        {
            var device = InputDevices.GetDeviceAtXRNode(controllerNode);
            if (!device.isValid)
            {
                return;
            }

            if (!device.TryGetFeatureValue(CommonUsages.primaryButton, out var isPressed))
            {
                return;
            }

            if (isPressed && !_wasPressed)
            {
                OnPressed?.Invoke();
            }

            _wasPressed = isPressed;
        }
    }
}
