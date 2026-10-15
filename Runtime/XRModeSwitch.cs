using UnityEngine;

namespace ITKamianets.Engine.XR
{
    /// <summary>
    /// Picks the VR or the flat-screen set of components of one object: <see cref="vrOnly"/> (e.g.
    /// the XR rig and VR controls) while <see cref="XRStatus.IsActive"/>, <see cref="flatOnly"/>
    /// (e.g. a camera-relative controller) otherwise, and switches when a headset starts or stops.
    /// Disabled, it turns both sets off, so as an owner-only component of a networked player it
    /// leaves other players' copies without controls. Keep the listed components disabled in the
    /// prefab; this enables the right ones.
    /// </summary>
    public class XRModeSwitch : MonoBehaviour
    {
        [SerializeField] private Behaviour[] vrOnly = new Behaviour[0];
        [SerializeField] private Behaviour[] flatOnly = new Behaviour[0];

        private bool? _vr;

        /// <summary>True when the VR set is on.</summary>
        public bool IsVR => _vr == true;

        private void OnEnable()
        {
            Apply(XRStatus.IsActive);
        }

        private void OnDisable()
        {
            _vr = null;
            SetAll(vrOnly, false);
            SetAll(flatOnly, false);
        }

        private void Update()
        {
            var vr = XRStatus.IsActive;
            if (vr != _vr)
            {
                Apply(vr);
            }
        }

        private void Apply(bool vr)
        {
            _vr = vr;

            // Off first, so the camera the flat set owned is free before the rig takes it (and back).
            SetAll(vr ? flatOnly : vrOnly, false);
            SetAll(vr ? vrOnly : flatOnly, true);
        }

        private static void SetAll(Behaviour[] behaviours, bool enabled)
        {
            foreach (var behaviour in behaviours)
            {
                if (behaviour != null)
                {
                    behaviour.enabled = enabled;
                }
            }
        }
    }
}
