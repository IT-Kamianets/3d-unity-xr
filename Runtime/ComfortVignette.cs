using UnityEngine;
using UnityEngine.UI;

namespace ITKamianets.Engine.XR
{
    /// <summary>
    /// Darkens the edges of the view (a tunnel) while the player moves with the stick, which
    /// reduces motion sickness. A uGUI image on a camera-space canvas, so it needs no extra shader
    /// in builds. <see cref="XRCharacterRig"/> creates one and sets <see cref="Strength"/>.
    /// </summary>
    public class ComfortVignette : MonoBehaviour
    {
        [Tooltip("How quickly it follows Strength, per second.")]
        [SerializeField] private float speed = 6f;

        /// <summary>0 = open, 1 = closed to a small window.</summary>
        public float Strength { get; set; }

        private RawImage _image;
        private float _current;

        private static Texture2D _texture;

        public static ComfortVignette Create(Camera camera)
        {
            var go = new GameObject("Comfort Vignette", typeof(Canvas));
            go.transform.SetParent(camera.transform, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = camera.nearClipPlane + 0.01f;
            canvas.sortingOrder = short.MaxValue;

            var imageObject = new GameObject("Tunnel", typeof(RectTransform), typeof(RawImage));
            imageObject.transform.SetParent(go.transform, false);
            var rect = (RectTransform)imageObject.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var vignette = go.AddComponent<ComfortVignette>();
            vignette._image = imageObject.GetComponent<RawImage>();
            vignette._image.texture = Texture;
            vignette._image.raycastTarget = false;
            vignette.Apply(0f);
            return vignette;
        }

        private void Update()
        {
            _current = Mathf.MoveTowards(_current, Mathf.Clamp01(Strength), speed * Time.deltaTime);
            Apply(_current);
        }

        // Scaling the image up opens the tunnel: at 0 its dark ring is off screen.
        private void Apply(float strength)
        {
            if (_image == null)
            {
                return;
            }

            _image.enabled = strength > 0.01f;
            var scale = Mathf.Lerp(2.2f, 1f, strength);
            _image.rectTransform.localScale = new Vector3(scale, scale, 1f);
        }

        // Transparent middle, black ring.
        private static Texture2D Texture
        {
            get
            {
                if (_texture != null)
                {
                    return _texture;
                }

                const int size = 128;
                _texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Comfort Vignette" };
                var pixels = new Color32[size * size];
                for (var y = 0; y < size; y++)
                {
                    for (var x = 0; x < size; x++)
                    {
                        var dx = (x + 0.5f) / size * 2f - 1f;
                        var dy = (y + 0.5f) / size * 2f - 1f;
                        var d = Mathf.Sqrt(dx * dx + dy * dy);
                        var alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 0.85f, d));
                        pixels[y * size + x] = new Color32(0, 0, 0, (byte)(alpha * 255));
                    }
                }

                _texture.SetPixels32(pixels);
                _texture.Apply();
                return _texture;
            }
        }
    }
}
