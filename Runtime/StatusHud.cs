using TMPro;
using UnityEngine;

namespace ITKamianets.Engine.XR
{
    /// <summary>
    /// World-space status text. Generic display primitive -- has no idea what it's reporting on.
    /// With <c>followView</c> on, it eases to a spot in front of the camera instead of being fixed
    /// to the head, which is uncomfortable in VR.
    /// </summary>
    public class StatusHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        [Header("Follow the view")]
        [SerializeField] private bool followView;

        [Tooltip("Empty: the main camera.")]
        [SerializeField] private Transform view;

        [SerializeField] private float distance = 1.5f;

        [Tooltip("Meters below eye height.")]
        [SerializeField] private float drop = 0.2f;

        [Tooltip("How quickly it catches up with the view. Lower is calmer.")]
        [SerializeField] private float followSpeed = 3f;

        public string Status => text != null ? text.text : null;

        public bool FollowView
        {
            get => followView;
            set => followView = value;
        }

        public void SetStatus(string message)
        {
            if (text != null)
            {
                text.text = message;
            }
        }

        /// <summary>Puts the display in front of the view at once, e.g. when it first appears.</summary>
        public void SnapToView()
        {
            if (TryGetTarget(out var position, out var rotation))
            {
                transform.SetPositionAndRotation(position, rotation);
            }
        }

        private void LateUpdate()
        {
            if (!followView || !TryGetTarget(out var position, out var rotation))
            {
                return;
            }

            var t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, position, t), Quaternion.Slerp(transform.rotation, rotation, t));
        }

        // Level in front of the view: it follows turning, not head tilt.
        private bool TryGetTarget(out Vector3 position, out Quaternion rotation)
        {
            var source = view != null ? view : Camera.main != null ? Camera.main.transform : null;
            if (source == null)
            {
                position = default;
                rotation = default;
                return false;
            }

            var forward = Vector3.ProjectOnPlane(source.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.ProjectOnPlane(source.up, Vector3.up);
            }

            forward.Normalize();
            position = source.position + forward * distance + Vector3.down * drop;
            rotation = Quaternion.LookRotation(forward, Vector3.up);
            return true;
        }
    }
}
