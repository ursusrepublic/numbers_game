using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Layout
{
    public static class MobileLayout
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        public const float MatchWidthOrHeight = 0.7f;

        public static void ConfigureCanvasScaler(CanvasScaler scaler)
        {
            if (scaler == null)
            {
                return;
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = MatchWidthOrHeight;
        }

        public static float GetScale(float width, float height)
        {
            float safeWidth = Mathf.Max(1f, width);
            float safeHeight = Mathf.Max(1f, height);
            float widthScale = safeWidth / ReferenceResolution.x;
            float heightScale = safeHeight / ReferenceResolution.y;
            return Mathf.Lerp(widthScale, heightScale, MatchWidthOrHeight);
        }

        public static float GetScale(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return 1f;
            }

            Rect rect = rectTransform.rect;
            return GetScale(rect.width, rect.height);
        }

        public static float ClampScaled(float baseValue, float minValue, float maxValue, float scale)
        {
            return Mathf.Clamp(baseValue * scale, minValue, maxValue);
        }
    }
}
