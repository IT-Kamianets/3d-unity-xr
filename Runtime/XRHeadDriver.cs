using ITKamianets.Engine.Input;
using UnityEngine;

namespace ITKamianets.Engine.XR
{
    /// <summary>
    /// Lets the headset look around with a scene camera that has no <see cref="XRCharacterRig"/>,
    /// e.g. the camera behind a main menu: while <see cref="XRStatus.IsActive"/>, the camera
    /// turns with the head and moves with it from where it stood, and both controllers show a
    /// pointer ray for world-space menus. Does nothing on flat screens, so the same camera serves
    /// both. Put it on the scene's main camera.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(50)]
    public class XRHeadDriver : MonoBehaviour
    {
        [SerializeField] private float pointerLength = 3f;

        private Transform _origin;
        private readonly Transform[] _hands = new Transform[2];
        private Pose _flatPose;

        private void OnEnable()
        {
            Application.onBeforeRender += Drive;
        }

        private void OnDisable()
        {
            Application.onBeforeRender -= Drive;
            Stop();
        }

        private void LateUpdate()
        {
            Drive();
        }

        private void Drive()
        {
            var input = EngineInput.Default;
            if (!XRStatus.IsActive || !input.TryReadHead(out var head))
            {
                Stop();
                return;
            }

            if (_origin == null)
            {
                Begin(head);
            }

            transform.SetPositionAndRotation(_origin.TransformPoint(head.position), _origin.rotation * head.rotation);
            for (var i = 0; i < _hands.Length; i++)
            {
                var tracked = input.TryReadHandAim((HandSide)i, out var pose);
                _hands[i].gameObject.SetActive(tracked);
                if (tracked)
                {
                    _hands[i].SetLocalPositionAndRotation(pose.position, pose.rotation);
                }
            }
        }

        // Tracking space starts where the camera stands, facing where it faces.
        private void Begin(Pose head)
        {
            _flatPose = new Pose(transform.position, transform.rotation);
            var yaw = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            _origin = new GameObject("XR Menu Origin").transform;
            _origin.SetPositionAndRotation(transform.position - yaw * head.position, yaw);
            for (var i = 0; i < _hands.Length; i++)
            {
                _hands[i] = new GameObject(i == (int)HandSide.Left ? "Left Pointer" : "Right Pointer").transform;
                _hands[i].SetParent(_origin, false);
                XRVisuals.Pointer(_hands[i], pointerLength);
            }

            XRVisuals.SetUITrackingOrigin(_origin);
        }

        // Back to where the camera stood, e.g. when the headset is taken off in the Editor.
        private void Stop()
        {
            if (_origin == null)
            {
                return;
            }

            XRVisuals.SetUITrackingOrigin(null, _origin);
            Destroy(_origin.gameObject);
            _origin = null;
            transform.SetPositionAndRotation(_flatPose.position, _flatPose.rotation);
        }
    }
}
