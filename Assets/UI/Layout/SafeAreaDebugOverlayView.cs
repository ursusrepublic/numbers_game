using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Layout
{
    [DisallowMultipleComponent]
    public sealed class SafeAreaDebugOverlayView : MonoBehaviour
    {
        private RectTransform _rootRect;
        private Image _topImage;
        private Image _bottomImage;
        private Image _leftImage;
        private Image _rightImage;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        private ScreenOrientation _lastOrientation;

        public static SafeAreaDebugOverlayView Create(Transform parent, string name, Color fillColor)
        {
            var overlayObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(SafeAreaDebugOverlayView));

            overlayObject.transform.SetParent(parent, false);

            var overlayRect = (RectTransform)overlayObject.transform;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var overlayView = overlayObject.GetComponent<SafeAreaDebugOverlayView>();
            overlayView.Setup(overlayRect, fillColor);
            return overlayView;
        }

        private void Awake()
        {
            if (_rootRect == null)
            {
                _rootRect = transform as RectTransform;
            }

            ApplySafeArea(force: true);
        }

        private void OnEnable()
        {
            ApplySafeArea(force: true);
        }

        private void Update()
        {
            ApplySafeArea(force: false);
        }

        public void Setup(RectTransform rootRect, Color fillColor)
        {
            _rootRect = rootRect;
            _topImage = CreateZone("TopZone", fillColor);
            _bottomImage = CreateZone("BottomZone", fillColor);
            _leftImage = CreateZone("LeftZone", fillColor);
            _rightImage = CreateZone("RightZone", fillColor);
            ApplySafeArea(force: true);
        }

        private Image CreateZone(string name, Color fillColor)
        {
            var zoneObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            zoneObject.transform.SetParent(transform, false);

            var zoneImage = zoneObject.GetComponent<Image>();
            zoneImage.color = fillColor;
            zoneImage.raycastTarget = false;
            return zoneImage;
        }

        private void ApplySafeArea(bool force)
        {
            if (_rootRect == null ||
                _topImage == null ||
                _bottomImage == null ||
                _leftImage == null ||
                _rightImage == null)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            ScreenOrientation orientation = Screen.orientation;

            if (!force &&
                safeArea == _lastSafeArea &&
                screenSize == _lastScreenSize &&
                orientation == _lastOrientation)
            {
                return;
            }

            _lastSafeArea = safeArea;
            _lastScreenSize = screenSize;
            _lastOrientation = orientation;

            float width = Mathf.Max(1f, screenSize.x);
            float height = Mathf.Max(1f, screenSize.y);

            float left = safeArea.xMin / width;
            float right = safeArea.xMax / width;
            float bottom = safeArea.yMin / height;
            float top = safeArea.yMax / height;

            SetZone((RectTransform)_topImage.transform, new Vector2(0f, top), new Vector2(1f, 1f));
            SetZone((RectTransform)_bottomImage.transform, new Vector2(0f, 0f), new Vector2(1f, bottom));
            SetZone((RectTransform)_leftImage.transform, new Vector2(0f, bottom), new Vector2(left, top));
            SetZone((RectTransform)_rightImage.transform, new Vector2(right, bottom), new Vector2(1f, top));
        }

        private static void SetZone(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
