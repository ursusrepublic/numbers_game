using UnityEngine;

namespace Game.UI.Layout
{
    [DisallowMultipleComponent]
    public sealed class SafeAreaView : MonoBehaviour
    {
        private RectTransform _targetRect;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        private ScreenOrientation _lastOrientation;

        public static SafeAreaView Create(Transform parent, string name)
        {
            var safeAreaObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(SafeAreaView));

            safeAreaObject.transform.SetParent(parent, false);

            var rectTransform = (RectTransform)safeAreaObject.transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var safeAreaView = safeAreaObject.GetComponent<SafeAreaView>();
            safeAreaView.Setup(rectTransform);
            return safeAreaView;
        }

        private void Awake()
        {
            if (_targetRect == null)
            {
                _targetRect = transform as RectTransform;
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

        public void Setup(RectTransform targetRect)
        {
            _targetRect = targetRect;
            ApplySafeArea(force: true);
        }

        private void ApplySafeArea(bool force)
        {
            if (_targetRect == null)
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

            Vector2 anchorMin = new Vector2(
                safeArea.xMin / width,
                safeArea.yMin / height);

            Vector2 anchorMax = new Vector2(
                safeArea.xMax / width,
                safeArea.yMax / height);

            _targetRect.anchorMin = anchorMin;
            _targetRect.anchorMax = anchorMax;
            _targetRect.offsetMin = Vector2.zero;
            _targetRect.offsetMax = Vector2.zero;
        }
    }
}
