using System;
using ITKamianets.Engine.Core;
using UnityEngine;

namespace ITKamianets.Engine.XR
{
    /// <summary>
    /// The player's comfort choices in VR: how the body turns and whether the view narrows while
    /// moving. Saved to <see cref="EngineStorage"/> (<see cref="Load"/> / <see cref="Save"/>), so a
    /// settings menu changes <see cref="Current"/> and calls Save.
    /// </summary>
    [Serializable]
    public class XRComfort
    {
        private const string Key = "xr.comfort";

        /// <summary>Turn in steps (snap) instead of smoothly. Snap is easier on the stomach.</summary>
        public bool SnapTurn = true;

        [Tooltip("Degrees per snap turn.")]
        public float SnapAngle = 30f;

        [Tooltip("Degrees per second with the stick fully over, for smooth turning.")]
        public float SmoothTurnSpeed = 120f;

        /// <summary>Darken the edges of the view while moving with the stick.</summary>
        public bool Vignette = true;

        [Tooltip("How far the vignette closes at full speed, 0..1.")]
        [Range(0f, 1f)]
        public float VignetteStrength = 0.7f;

        private static XRComfort _current;

        /// <summary>The saved choices, loaded on first use.</summary>
        public static XRComfort Current => _current ??= Load();

        /// <summary>Raised by <see cref="Save"/>.</summary>
        public static event Action Changed;

        public static XRComfort Load()
        {
            return EngineStorage.Current.GetJson(Key, new XRComfort()) ?? new XRComfort();
        }

        public void Save()
        {
            _current = this;
            var storage = EngineStorage.Current;
            storage.SetJson(Key, this);
            storage.Save();
            Changed?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCurrent()
        {
            _current = null;
        }
    }
}
