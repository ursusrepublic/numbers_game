using System;
using Game.App.Daily;
using Game.UI.Styling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Daily
{
    [DisallowMultipleComponent]
    public sealed class DailyCalendarDayView : MonoBehaviour
    {
        private static Sprite _circleSprite;
        private static Sprite _ringSprite;

        private Button _button;
        private RectTransform _rootRect;
        private RectTransform _selectedBackgroundRect;
        private RectTransform _progressTrackRect;
        private RectTransform _progressFillRect;
        private RectTransform _todayDotRect;
        private TMP_Text _dayLabel;
        private Image _selectedBackgroundImage;
        private Image _progressTrackImage;
        private Image _progressFillImage;
        private Image _todayDotImage;
        private DailyChallengeDateKey _date;

        public event Action<DailyChallengeDateKey> Clicked;

        public static DailyCalendarDayView Create(Transform parent, TMP_FontAsset font)
        {
            var rootObject = new GameObject(
                "DayView",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(DailyCalendarDayView));

            rootObject.transform.SetParent(parent, false);

            var rootRect = (RectTransform)rootObject.transform;
            rootRect.sizeDelta = new Vector2(74f, 74f);

            var rootImage = rootObject.GetComponent<Image>();
            rootImage.color = Color.clear;

            var button = rootObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = rootImage;

            var progressTrackObject = new GameObject("ProgressTrack", typeof(RectTransform), typeof(Image));
            progressTrackObject.transform.SetParent(rootObject.transform, false);
            var progressTrackRect = (RectTransform)progressTrackObject.transform;
            ConfigureCentered(progressTrackRect, 52f);

            var progressTrackImage = progressTrackObject.GetComponent<Image>();
            progressTrackImage.sprite = GetRingSprite();
            progressTrackImage.color = GamePalette.DailyProgressTrack;
            progressTrackImage.raycastTarget = false;

            var progressFillObject = new GameObject("ProgressFill", typeof(RectTransform), typeof(Image));
            progressFillObject.transform.SetParent(rootObject.transform, false);
            var progressFillRect = (RectTransform)progressFillObject.transform;
            ConfigureCentered(progressFillRect, 52f);

            var progressFillImage = progressFillObject.GetComponent<Image>();
            progressFillImage.sprite = GetRingSprite();
            progressFillImage.type = Image.Type.Filled;
            progressFillImage.fillMethod = Image.FillMethod.Radial360;
            progressFillImage.fillOrigin = (int)Image.Origin360.Top;
            progressFillImage.fillClockwise = true;
            progressFillImage.fillAmount = 0f;
            progressFillImage.color = GamePalette.DailyProgressFill;
            progressFillImage.raycastTarget = false;

            var selectedBackgroundObject = new GameObject("SelectedBackground", typeof(RectTransform), typeof(Image));
            selectedBackgroundObject.transform.SetParent(rootObject.transform, false);
            var selectedBackgroundRect = (RectTransform)selectedBackgroundObject.transform;
            ConfigureCentered(selectedBackgroundRect, 46f);

            var selectedBackgroundImage = selectedBackgroundObject.GetComponent<Image>();
            selectedBackgroundImage.sprite = GetCircleSprite();
            selectedBackgroundImage.color = GamePalette.DailySelectedDayBackground;
            selectedBackgroundImage.raycastTarget = false;

            var dayLabelObject = new GameObject("DayLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            dayLabelObject.transform.SetParent(rootObject.transform, false);
            var dayLabelRect = (RectTransform)dayLabelObject.transform;
            dayLabelRect.anchorMin = Vector2.zero;
            dayLabelRect.anchorMax = Vector2.one;
            dayLabelRect.offsetMin = Vector2.zero;
            dayLabelRect.offsetMax = Vector2.zero;

            var dayLabel = dayLabelObject.GetComponent<TextMeshProUGUI>();
            dayLabel.font = font != null ? font : TMP_Settings.defaultFontAsset;
            dayLabel.fontSize = 26f;
            dayLabel.enableAutoSizing = false;
            dayLabel.alignment = TextAlignmentOptions.Center;
            dayLabel.textWrappingMode = TextWrappingModes.NoWrap;
            dayLabel.overflowMode = TextOverflowModes.Overflow;
            dayLabel.raycastTarget = false;

            var todayDotObject = new GameObject("TodayDot", typeof(RectTransform), typeof(Image));
            todayDotObject.transform.SetParent(rootObject.transform, false);
            var todayDotRect = (RectTransform)todayDotObject.transform;
            todayDotRect.anchorMin = new Vector2(0.5f, 1f);
            todayDotRect.anchorMax = new Vector2(0.5f, 1f);
            todayDotRect.pivot = new Vector2(0.5f, 0.5f);
            todayDotRect.sizeDelta = new Vector2(8f, 8f);
            todayDotRect.anchoredPosition = new Vector2(0f, -10f);

            var todayDotImage = todayDotObject.GetComponent<Image>();
            todayDotImage.sprite = GetCircleSprite();
            todayDotImage.color = GamePalette.DailyTodayDot;
            todayDotImage.raycastTarget = false;

            var view = rootObject.GetComponent<DailyCalendarDayView>();
            view.Setup(
                button,
                rootRect,
                selectedBackgroundRect,
                progressTrackRect,
                progressFillRect,
                todayDotRect,
                dayLabel,
                selectedBackgroundImage,
                progressTrackImage,
                progressFillImage,
                todayDotImage);
            return view;
        }

        public void SetState(DailyCalendarDayState state)
        {
            _date = state.Date;

            bool isVisible = !state.IsEmpty;
            _button.gameObject.SetActive(true);
            _button.interactable = isVisible && state.IsSelectable;
            _dayLabel.gameObject.SetActive(isVisible);
            _selectedBackgroundImage.enabled = isVisible && state.IsSelected;
            _progressTrackImage.enabled = isVisible && (state.HasProgress || state.IsCompleted);
            _progressFillImage.enabled = isVisible && (state.HasProgress || state.IsCompleted);
            _todayDotImage.enabled = isVisible && state.IsToday;

            if (!isVisible)
            {
                _dayLabel.text = string.Empty;
                return;
            }

            _dayLabel.text = state.DayNumber.ToString();

            if (state.IsSelected)
            {
                _dayLabel.color = GamePalette.DailySelectedDayText;
            }
            else if (state.IsFuture)
            {
                _dayLabel.color = GamePalette.DailyFutureDayText;
            }
            else
            {
                _dayLabel.color = GamePalette.DailyDayText;
            }

            if (state.IsCompleted)
            {
                _progressTrackImage.color = GamePalette.DailyCompletedRing;
                _progressFillImage.color = GamePalette.DailyCompletedRing;
                _progressFillImage.fillAmount = 1f;
            }
            else
            {
                _progressTrackImage.color = GamePalette.DailyProgressTrack;
                _progressFillImage.color = state.HasActiveRun
                    ? GamePalette.DailyProgressFill
                    : GamePalette.DailyProgressFill;
                _progressFillImage.fillAmount = Mathf.Clamp01(state.Progress01);
            }
        }

        public void SetMetrics(float cellSize, float fontSize)
        {
            if (_rootRect != null)
            {
                _rootRect.sizeDelta = new Vector2(cellSize, cellSize);
            }

            float ringSize = cellSize * 0.72f;
            float selectedSize = cellSize * 0.62f;
            float dotSize = Mathf.Max(6f, cellSize * 0.10f);

            if (_progressTrackRect != null)
            {
                _progressTrackRect.sizeDelta = new Vector2(ringSize, ringSize);
            }

            if (_progressFillRect != null)
            {
                _progressFillRect.sizeDelta = new Vector2(ringSize, ringSize);
            }

            if (_selectedBackgroundRect != null)
            {
                _selectedBackgroundRect.sizeDelta = new Vector2(selectedSize, selectedSize);
            }

            if (_todayDotRect != null)
            {
                _todayDotRect.sizeDelta = new Vector2(dotSize, dotSize);
                _todayDotRect.anchoredPosition = new Vector2(0f, -(cellSize * 0.14f));
            }

            if (_dayLabel != null)
            {
                _dayLabel.fontSize = fontSize;
            }
        }

        private void Setup(
            Button button,
            RectTransform rootRect,
            RectTransform selectedBackgroundRect,
            RectTransform progressTrackRect,
            RectTransform progressFillRect,
            RectTransform todayDotRect,
            TMP_Text dayLabel,
            Image selectedBackgroundImage,
            Image progressTrackImage,
            Image progressFillImage,
            Image todayDotImage)
        {
            _button = button;
            _rootRect = rootRect;
            _selectedBackgroundRect = selectedBackgroundRect;
            _progressTrackRect = progressTrackRect;
            _progressFillRect = progressFillRect;
            _todayDotRect = todayDotRect;
            _dayLabel = dayLabel;
            _selectedBackgroundImage = selectedBackgroundImage;
            _progressTrackImage = progressTrackImage;
            _progressFillImage = progressFillImage;
            _todayDotImage = todayDotImage;

            _button.onClick.AddListener(HandleClicked);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClicked);
            }
        }

        private void HandleClicked()
        {
            Clicked?.Invoke(_date);
        }

        private static void ConfigureCentered(RectTransform rectTransform, float size)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(size, size);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        private static Sprite GetCircleSprite()
        {
            if (_circleSprite != null)
            {
                return _circleSprite;
            }

            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float radius = (size * 0.5f) - 1.5f;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(radius - distance);
                    pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            _circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _circleSprite;
        }

        private static Sprite GetRingSprite()
        {
            if (_ringSprite != null)
            {
                return _ringSprite;
            }

            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outerRadius = (size * 0.5f) - 1.5f;
            float innerRadius = outerRadius - 10f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = 0f;
                    if (distance <= outerRadius && distance >= innerRadius)
                    {
                        float edge = Mathf.Min(outerRadius - distance, distance - innerRadius);
                        alpha = Mathf.Clamp01(edge + 0.5f);
                    }

                    pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            _ringSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _ringSprite;
        }
    }
}
