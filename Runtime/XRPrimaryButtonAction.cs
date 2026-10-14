using ITKamianets.Engine.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ITKamianets.Engine.XR
{
    /// <summary>
    /// Fires OnPressed once per press of an <see cref="EngineInput"/> action, by default
    /// <c>Player/Jump</c>, which is the right controller's primary button (A) in the XR scheme.
    /// Generic input primitive -- has no idea what the button does. Reads the action, never the
    /// device, so the same component also answers to Space or a gamepad's A when testing on PC.
    /// </summary>
    public class XRPrimaryButtonAction : MonoBehaviour
    {
        [Tooltip("\"Map/Action\" in EngineInput, e.g. Player/Jump (A), Player/Primary (trigger), UI/Pause (menu).")]
        [SerializeField] private string action = "Player/Jump";

        public UnityEvent OnPressed;

        private InputAction _action;

        public string Action
        {
            get => action;
            set
            {
                action = value;
                _action = null;
            }
        }

        private void Update()
        {
            _action ??= EngineInput.Default.Asset.FindAction(action);
            if (_action != null && _action.WasPressedThisFrame())
            {
                OnPressed?.Invoke();
            }
        }
    }
}
