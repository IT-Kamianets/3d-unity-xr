using System;
using ITKamianets.Engine.Character;
using ITKamianets.Engine.Input;
using UnityEngine;

namespace ITKamianets.Engine.XR
{
    /// <summary>
    /// The VR view of a character: an XR origin with the headset as the camera and the two
    /// controllers as hand anchors, kept on a <see cref="CharacterMotor"/>.
    ///
    /// <list type="bullet">
    /// <item>The rig follows the motor; the motor never follows the rig directly. When the player
    /// steps or leans in the room, <see cref="HeadOffset"/> says how far the head is from the body,
    /// and the controls (e.g. <c>VRControls</c>) walk the body there, so walls still block. A head
    /// that the body can't reach (a wall) is pulled back to <see cref="Settings"/>' leash.</item>
    /// <item>Eye height: the player's real standing height is mapped to the character's eyes at
    /// <see cref="Recenter"/>, so short and tall players see from the same eyes. Crouching for real
    /// lowers the view; a crouched body under a low ceiling keeps the view below it.</item>
    /// <item>Turning (snap or smooth, <see cref="XRComfort"/>) turns the rig around the head; the
    /// character faces where the head looks.</item>
    /// <item>A teleport of the motor (respawn) moves the rig with it and faces the new yaw.</item>
    /// </list>
    ///
    /// The rig lives at the scene root (the body turns, the room doesn't). While it is enabled
    /// the scene's previous main camera is switched off and the rig's camera is the main camera.
    /// Put it on the local player only (an owner-only component in a networked game). Updates before
    /// the controls (they read <see cref="HeadYaw"/> and <see cref="HeadOffset"/>) and follows the
    /// body in LateUpdate.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class XRCharacterRig : MonoBehaviour
    {
        [Serializable]
        public class RigSettings
        {
            [Tooltip("How far the head may be from the body before the view is held back, meters.")]
            public float Leash = 0.3f;

            [Tooltip("A body jump longer than this (respawn, blink) moves the rig with it instead of leashing.")]
            public float TeleportDistance = 1.5f;

            [Tooltip("Seconds the view takes to catch up with the body. A networked body moves at the tick rate (e.g. 60 Hz) while a headset draws at 72-120 Hz; this hides the steps.")]
            public float Smoothing = 0.025f;

            [Tooltip("Draw only the shadow of the character's body for this player, so the view isn't inside the head.")]
            public bool HideBody = true;

            [Tooltip("Show a simple marker at each controller until a game gives the hands a model.")]
            public bool ShowHandMarkers = true;

            public float NearClip = 0.05f;
            public float FarClip = 300f;
        }

        [Tooltip("Empty: the CharacterMotor on this object.")]
        [SerializeField] private CharacterMotor target;

        [SerializeField] private RigSettings settings = new RigSettings();

        public RigSettings Settings => settings;
        public CharacterMotor Target => target;

        /// <summary>The XR origin (tracking space). Its yaw is the rig's turn.</summary>
        public Transform Origin { get; private set; }

        public Camera Camera { get; private set; }

        public Transform Head => Camera != null ? Camera.transform : null;

        /// <summary>Head position minus body position on the ground plane, in world space.</summary>
        public Vector3 HeadOffset { get; private set; }

        /// <summary>Head height above the character's feet, mapped to the character's eyes (meters).</summary>
        public float HeadHeight { get; private set; }

        /// <summary>The head's facing around the up axis, degrees. The character should face this way.</summary>
        public float HeadYaw { get; private set; }

        /// <summary>True once the headset has reported a pose.</summary>
        public bool IsTracking { get; private set; }

        /// <summary>
        /// Set by the controls each frame: true while the body is walking toward the head (the stick
        /// is idle), so the rig counts that movement as catching up, not as the player moving on.
        /// </summary>
        public bool BodyFollowsHead { get; set; }

        /// <summary>0..1, how much the comfort vignette is closed now. Set by the controls from movement.</summary>
        public float Motion { get; set; }

        /// <summary>Raised after a snap turn, with the degrees turned.</summary>
        public event Action<float> Turned;

        private readonly Transform[] _hands = new Transform[2];
        private readonly bool[] _handTracked = new bool[2];
        private GameObject _previousCamera;
        private ComfortVignette _vignette;
        private float _heightOffset;
        private bool _calibrated;
        private bool _turnHeld;
        private Vector3 _lastBody;
        private Vector3 _lastHead;
        private Vector3 _offset;
        private Vector3 _viewBody;
        private Renderer[] _bodyRenderers;
        private UnityEngine.Rendering.ShadowCastingMode[] _bodyShadows;

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<CharacterMotor>();
            }
        }

        private void OnEnable()
        {
            BuildRig();
            HideBody(settings.HideBody);
            Application.onBeforeRender += OnBeforeRender;
        }

        private void OnDisable()
        {
            Application.onBeforeRender -= OnBeforeRender;
            HideBody(false);
            if (Origin != null)
            {
                XRVisuals.SetUITrackingOrigin(null, Origin);
                Destroy(Origin.gameObject);
            }

            Origin = null;
            Camera = null;
            _calibrated = false;
            IsTracking = false;
            if (_previousCamera != null)
            {
                _previousCamera.SetActive(true);
                _previousCamera = null;
            }
        }

        /// <summary>The anchor that follows a controller's aim pose (forward is where it points).</summary>
        public Transform Hand(HandSide hand)
        {
            return _hands[(int)hand];
        }

        public bool IsHandTracked(HandSide hand)
        {
            return _handTracked[(int)hand];
        }

        /// <summary>The ray a controller points along, in world space. False when it isn't tracked.</summary>
        public bool TryGetHandRay(HandSide hand, out Ray ray)
        {
            var anchor = _hands[(int)hand];
            if (anchor == null || !_handTracked[(int)hand])
            {
                ray = default;
                return false;
            }

            ray = new Ray(anchor.position, anchor.forward);
            return true;
        }

        /// <summary>
        /// Puts the head over the body, facing the character's yaw, and maps the player's current
        /// head height to standing eye height. Call when the player stands up straight (a button).
        /// </summary>
        public void Recenter()
        {
            if (Origin == null || target == null || !EngineInput.Default.TryReadHead(out var head))
            {
                return;
            }

            _heightOffset = target.Settings.StandingEyeHeight - head.position.y;
            _calibrated = true;
            FaceYaw(target.Yaw);
            _offset = Vector3.zero;
            _viewBody = target.transform.position;
            PlaceOverBody(_offset);
            _lastBody = target.transform.position;
            _lastHead = Head.position;
        }

        private void BuildRig()
        {
            if (Origin != null)
            {
                return;
            }

            var main = Camera.main;
            if (main != null)
            {
                _previousCamera = main.gameObject;
                _previousCamera.SetActive(false);
            }

            Origin = new GameObject("XR Origin").transform;
            if (target != null)
            {
                Origin.SetPositionAndRotation(target.transform.position, Quaternion.Euler(0f, target.Yaw, 0f));
                _lastBody = target.transform.position;
                _viewBody = _lastBody;
            }

            var cameraObject = new GameObject("XR Camera") { tag = "MainCamera" };
            cameraObject.transform.SetParent(Origin, false);
            Camera = cameraObject.AddComponent<Camera>();
            Camera.nearClipPlane = settings.NearClip;
            Camera.farClipPlane = settings.FarClip;
            cameraObject.AddComponent<AudioListener>();

            for (var i = 0; i < _hands.Length; i++)
            {
                var hand = new GameObject(i == (int)HandSide.Left ? "Left Hand" : "Right Hand").transform;
                hand.SetParent(Origin, false);
                if (settings.ShowHandMarkers)
                {
                    XRVisuals.Marker(hand);
                }

                hand.gameObject.SetActive(false);
                _hands[i] = hand;
            }

            _vignette = ComfortVignette.Create(Camera);
        }

        private void HideBody(bool hide)
        {
            if (hide && target != null)
            {
                _bodyRenderers = target.GetComponentsInChildren<Renderer>();
                _bodyShadows = new UnityEngine.Rendering.ShadowCastingMode[_bodyRenderers.Length];
                for (var i = 0; i < _bodyRenderers.Length; i++)
                {
                    _bodyShadows[i] = _bodyRenderers[i].shadowCastingMode;
                    _bodyRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                }
            }
            else if (!hide && _bodyRenderers != null)
            {
                for (var i = 0; i < _bodyRenderers.Length; i++)
                {
                    if (_bodyRenderers[i] != null)
                    {
                        _bodyRenderers[i].shadowCastingMode = _bodyShadows[i];
                    }
                }

                _bodyRenderers = null;
            }
        }

        private void Update()
        {
            if (Origin == null || target == null)
            {
                return;
            }

            ReadPoses();
            if (!IsTracking)
            {
                return;
            }

            // Menus opened in the match are pointed at from this rig's controllers.
            XRVisuals.SetUITrackingOrigin(Origin);

            if (!_calibrated)
            {
                Recenter();
            }

            Turn();
            UpdateHeadState();
        }

        // After the motor has moved this frame: carry the rig with the body.
        private void LateUpdate()
        {
            if (Origin == null || target == null || !IsTracking)
            {
                return;
            }

            var body = target.transform.position;
            if (Vector3.Distance(body, _lastBody) > settings.TeleportDistance)
            {
                FaceYaw(target.Yaw);
                _offset = Vector3.zero;
                _viewBody = body;
            }
            else
            {
                _viewBody = settings.Smoothing > 0f ? Vector3.Lerp(_viewBody, body, 1f - Mathf.Exp(-Time.deltaTime / settings.Smoothing)) : body;
                // The head moves with the body (stick walking), plus the player's own steps in the
                // room. While the body walks toward the head, that walk closes the gap instead.
                _offset += Flat(Head.position - _lastHead);
                if (BodyFollowsHead)
                {
                    _offset -= Flat(body - _lastBody);
                }

                _offset = Vector3.ClampMagnitude(_offset, settings.Leash);
            }

            PlaceOverBody(_offset);
            _lastBody = body;
            _lastHead = Head.position;
            ReadPoses();
            UpdateHeadState();
            if (_vignette != null)
            {
                _vignette.Strength = XRComfort.Current.Vignette ? Motion * XRComfort.Current.VignetteStrength : 0f;
            }
        }

        private void OnBeforeRender()
        {
            // The latest headset pose just before drawing, so the view lags as little as possible.
            if (Origin != null)
            {
                ReadPoses();
            }
        }

        private void ReadPoses()
        {
            var input = EngineInput.Default;
            if (input.TryReadHead(out var head))
            {
                IsTracking = true;
                Camera.transform.SetLocalPositionAndRotation(head.position, head.rotation);
            }

            for (var i = 0; i < _hands.Length; i++)
            {
                _handTracked[i] = input.TryReadHandAim((HandSide)i, out var pose);
                if (_handTracked[i])
                {
                    _hands[i].SetLocalPositionAndRotation(pose.position, pose.rotation);
                }

                if (_hands[i].gameObject.activeSelf != _handTracked[i])
                {
                    _hands[i].gameObject.SetActive(_handTracked[i]);
                }
            }
        }

        private void Turn()
        {
            var comfort = XRComfort.Current;
            var input = EngineInput.Default;
            var stick = input.PlayerEnabled ? input.Player.Turn.ReadValue<float>() : 0f;

            if (comfort.SnapTurn)
            {
                if (Mathf.Abs(stick) > 0.7f && !_turnHeld)
                {
                    _turnHeld = true;
                    var angle = Mathf.Sign(stick) * comfort.SnapAngle;
                    RotateAroundHead(angle);
                    Turned?.Invoke(angle);
                }
                else if (Mathf.Abs(stick) < 0.3f)
                {
                    _turnHeld = false;
                }
            }
            else if (Mathf.Abs(stick) > 0.15f)
            {
                RotateAroundHead(stick * comfort.SmoothTurnSpeed * Time.deltaTime);
            }
        }

        private void RotateAroundHead(float degrees)
        {
            var pivot = Head.position;
            var turn = Quaternion.Euler(0f, degrees, 0f);
            Origin.SetPositionAndRotation(pivot + turn * (Origin.position - pivot), turn * Origin.rotation);
        }

        // Turns the rig so the head faces the given yaw.
        private void FaceYaw(float yaw)
        {
            var headYaw = Mathf.Atan2(Head.forward.x, Head.forward.z) * Mathf.Rad2Deg;
            RotateAroundHead(Mathf.DeltaAngle(headYaw, yaw));
        }

        // Moves the origin so the head is at body + offset on the ground plane, and at the mapped eye height.
        private void PlaceOverBody(Vector3 offset)
        {
            var body = _viewBody;
            var local = Head.localPosition;
            var headAboveFloor = local.y + _heightOffset;
            var eye = Mathf.Min(headAboveFloor, target.EyeHeight + 0.05f);

            var head = Flat(Head.position);
            var wanted = Flat(body) + Flat(offset);
            var position = Origin.position + (wanted - head);
            position.y = body.y + eye - local.y;
            Origin.position = position;
        }

        private void UpdateHeadState()
        {
            HeadOffset = Vector3.ClampMagnitude(_offset + Flat(Head.position - _lastHead), settings.Leash);
            HeadHeight = Head.localPosition.y + _heightOffset;
            var forward = Head.forward;
            if (Mathf.Abs(forward.y) > 0.95f)
            {
                forward = -Head.up * Mathf.Sign(forward.y);
            }

            HeadYaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        }

        private static Vector3 Flat(Vector3 v)
        {
            return new Vector3(v.x, 0f, v.z);
        }
    }
}
