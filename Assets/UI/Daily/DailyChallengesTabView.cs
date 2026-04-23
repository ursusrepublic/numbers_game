using System;
using System.Collections.Generic;
using Game.App.Daily;
using Game.UI.Layout;
using Game.UI.Styling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Daily
{
    [DisallowMultipleComponent]
    public sealed class DailyChallengesTabView : MonoBehaviour
    {
        private static Sprite _circleSprite;

        private const float HeaderHeight = 300f;
        private const float MinHeaderHeight = 220f;
        private const float MaxHeaderHeight = 340f;
        private const float PlayButtonHeight = 96f;
        private const float MinPlayButtonHeight = 76f;
        private const float MaxPlayButtonHeight = 112f;
        private const float PlayButtonBottomPadding = 28f;
        private const float MinPlayButtonBottomPadding = 20f;
        private const float MaxPlayButtonBottomPadding = 36f;
        private const float ContentSidePadding = 28f;
        private const float MinContentSidePadding = 16f;
        private const float MaxContentSidePadding = 36f;
        private const float HeaderTitleFontSize = 32f;
        private const float MinHeaderTitleFontSize = 24f;
        private const float MaxHeaderTitleFontSize = 34f;
        private const float MonthLabelFontSize = 34f;
        private const float MinMonthLabelFontSize = 24f;
        private const float MaxMonthLabelFontSize = 36f;
        private const float CounterFontSize = 28f;
        private const float MinCounterFontSize = 18f;
        private const float MaxCounterFontSize = 30f;
        private const float WeekdayFontSize = 18f;
        private const float MinWeekdayFontSize = 14f;
        private const float MaxWeekdayFontSize = 20f;
        private const float DayFontSize = 26f;
        private const float MinDayFontSize = 18f;
        private const float MaxDayFontSize = 28f;

        private readonly List<DailyCalendarDayView> _dayViews = new List<DailyCalendarDayView>();
        private readonly List<TMP_Text> _weekdayLabels = new List<TMP_Text>();

        private RectTransform _rootRect;
        private RectTransform _headerRect;
        private RectTransform _calendarRect;
        private RectTransform _daysGridRect;
        private RectTransform _playButtonRect;
        private GridLayoutGroup _daysGrid;
        private Button _previousMonthButton;
        private Button _nextMonthButton;
        private Button _playButton;
        private Image _headerIconImage;
        private TMP_Text _titleLabel;
        private TMP_Text _monthLabel;
        private TMP_Text _countLabel;
        private TMP_Text _playButtonLabel;
        private DailyChallengeDateKey _selectedDate;
        private Vector2 _lastRootSize = new Vector2(-1f, -1f);

        public event Action PreviousMonthClicked;
        public event Action NextMonthClicked;
        public event Action<DailyChallengeDateKey> DaySelected;
        public event Action<DailyChallengeDateKey> PlayClicked;

        public static DailyChallengesTabView Create(
            Transform parent,
            TMP_FontAsset regularFont,
            TMP_FontAsset boldFont,
            Texture2D headerIconTexture)
        {
            var rootObject = new GameObject(
                "DailyChallengesTabView",
                typeof(RectTransform),
                typeof(DailyChallengesTabView));

            rootObject.transform.SetParent(parent, false);

            var rootRect = (RectTransform)rootObject.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var headerObject = new GameObject("HeaderArea", typeof(RectTransform));
            headerObject.transform.SetParent(rootObject.transform, false);
            var headerRect = (RectTransform)headerObject.transform;
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(0f, HeaderHeight);
            headerRect.anchoredPosition = Vector2.zero;

            TMP_Text titleLabel = CreateText(headerObject.transform, "TitleLabel", "Daily Challenges");
            ConfigureRect((RectTransform)titleLabel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(520f, 48f), new Vector2(0f, -24f));

            var headerIconObject = new GameObject("HeaderIcon", typeof(RectTransform), typeof(Image));
            headerIconObject.transform.SetParent(headerObject.transform, false);
            var headerIconRect = (RectTransform)headerIconObject.transform;
            headerIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            headerIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            headerIconRect.pivot = new Vector2(0.5f, 0.5f);
            headerIconRect.sizeDelta = new Vector2(120f, 120f);
            headerIconRect.anchoredPosition = new Vector2(0f, -8f);

            var headerIconImage = headerIconObject.GetComponent<Image>();
            headerIconImage.sprite = CreateTextureSprite(headerIconTexture);
            headerIconImage.color = GamePalette.DailyHeaderIcon;
            headerIconImage.preserveAspect = true;

            (Button previousMonthButton, TMP_Text previousLabel) = CreateArrowButton(headerObject.transform, "PreviousMonthButton", "‹");
            ConfigureRect((RectTransform)previousMonthButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(60f, 44f), new Vector2(0f, 4f));

            TMP_Text monthLabel = CreateText(headerObject.transform, "MonthLabel", "October 2025");
            ConfigureRect((RectTransform)monthLabel.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(320f, 44f), new Vector2(72f, 4f));

            (Button nextMonthButton, TMP_Text nextLabel) = CreateArrowButton(headerObject.transform, "NextMonthButton", "›");
            ConfigureRect((RectTransform)nextMonthButton.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(60f, 44f), new Vector2(0f, 4f));

            var countIconObject = new GameObject("CompletedIcon", typeof(RectTransform), typeof(Image));
            countIconObject.transform.SetParent(headerObject.transform, false);
            var countIconRect = (RectTransform)countIconObject.transform;
            countIconRect.anchorMin = new Vector2(1f, 0f);
            countIconRect.anchorMax = new Vector2(1f, 0f);
            countIconRect.pivot = new Vector2(1f, 0f);
            countIconRect.sizeDelta = new Vector2(20f, 20f);
            countIconRect.anchoredPosition = new Vector2(-132f, 16f);

            var countIconImage = countIconObject.GetComponent<Image>();
            countIconImage.sprite = GetCircleSprite();
            countIconImage.color = GamePalette.DailyCompletedRing;

            TMP_Text countLabel = CreateText(headerObject.transform, "CountLabel", "0/31");
            ConfigureRect((RectTransform)countLabel.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(110f, 44f), new Vector2(0f, 4f));

            var calendarObject = new GameObject("CalendarArea", typeof(RectTransform));
            calendarObject.transform.SetParent(rootObject.transform, false);
            var calendarRect = (RectTransform)calendarObject.transform;
            calendarRect.anchorMin = new Vector2(0f, 0f);
            calendarRect.anchorMax = new Vector2(1f, 1f);
            calendarRect.offsetMin = new Vector2(0f, PlayButtonBottomPadding + PlayButtonHeight + 20f);
            calendarRect.offsetMax = new Vector2(0f, -HeaderHeight);

            var weekdaysObject = new GameObject(
                "WeekdaysRow",
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup));
            weekdaysObject.transform.SetParent(calendarObject.transform, false);
            var weekdaysRect = (RectTransform)weekdaysObject.transform;
            weekdaysRect.anchorMin = new Vector2(0f, 1f);
            weekdaysRect.anchorMax = new Vector2(1f, 1f);
            weekdaysRect.pivot = new Vector2(0.5f, 1f);
            weekdaysRect.sizeDelta = new Vector2(0f, 32f);
            weekdaysRect.anchoredPosition = Vector2.zero;

            var weekdaysLayout = weekdaysObject.GetComponent<HorizontalLayoutGroup>();
            weekdaysLayout.childAlignment = TextAnchor.MiddleCenter;
            weekdaysLayout.childControlWidth = false;
            weekdaysLayout.childControlHeight = false;
            weekdaysLayout.childForceExpandWidth = false;
            weekdaysLayout.childForceExpandHeight = false;
            weekdaysLayout.spacing = 0f;

            string[] weekdayTitles = { "S", "M", "T", "W", "T", "F", "S" };
            var weekdayLabels = new List<TMP_Text>();
            foreach (string weekdayTitle in weekdayTitles)
            {
                TMP_Text weekdayLabel = CreateText(weekdaysObject.transform, $"{weekdayTitle}Label", weekdayTitle);
                var layout = weekdayLabel.gameObject.AddComponent<LayoutElement>();
                layout.preferredWidth = 60f;
                layout.preferredHeight = 24f;
                weekdayLabels.Add(weekdayLabel);
            }

            var daysGridObject = new GameObject(
                "DaysGrid",
                typeof(RectTransform),
                typeof(GridLayoutGroup));
            daysGridObject.transform.SetParent(calendarObject.transform, false);
            var daysGridRect = (RectTransform)daysGridObject.transform;
            daysGridRect.anchorMin = new Vector2(0f, 0f);
            daysGridRect.anchorMax = new Vector2(1f, 1f);
            daysGridRect.offsetMin = Vector2.zero;
            daysGridRect.offsetMax = new Vector2(0f, -40f);

            var daysGrid = daysGridObject.GetComponent<GridLayoutGroup>();
            daysGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            daysGrid.constraintCount = 7;
            daysGrid.cellSize = new Vector2(74f, 74f);
            daysGrid.spacing = new Vector2(0f, 0f);
            daysGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            daysGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            daysGrid.childAlignment = TextAnchor.UpperCenter;
            daysGrid.padding = new RectOffset(0, 0, 0, 0);

            var dayViews = new List<DailyCalendarDayView>();
            for (int index = 0; index < 42; index++)
            {
                DailyCalendarDayView dayView = DailyCalendarDayView.Create(daysGridObject.transform, regularFont);
                dayViews.Add(dayView);
            }

            (Button playButton, TMP_Text playButtonLabel) = CreatePlayButton(rootObject.transform, "PlayButton", "Play");
            var playButtonRect = (RectTransform)playButton.transform;
            playButtonRect.anchorMin = new Vector2(0.5f, 0f);
            playButtonRect.anchorMax = new Vector2(0.5f, 0f);
            playButtonRect.pivot = new Vector2(0.5f, 0f);
            playButtonRect.sizeDelta = new Vector2(860f, PlayButtonHeight);
            playButtonRect.anchoredPosition = new Vector2(0f, PlayButtonBottomPadding);

            var view = rootObject.GetComponent<DailyChallengesTabView>();
            view.Setup(
                rootRect,
                headerRect,
                calendarRect,
                daysGridRect,
                playButtonRect,
                daysGrid,
                previousMonthButton,
                nextMonthButton,
                playButton,
                headerIconImage,
                titleLabel,
                monthLabel,
                countLabel,
                playButtonLabel,
                previousLabel,
                nextLabel,
                weekdayLabels,
                dayViews,
                regularFont,
                boldFont);
            return view;
        }

        public void SetMonthState(DailyCalendarMonthState state)
        {
            _selectedDate = state.SelectedDate;

            if (_monthLabel != null)
            {
                _monthLabel.text = state.MonthLabel;
            }

            if (_countLabel != null)
            {
                _countLabel.text = $"{state.CompletedDays}/{state.TotalDays}";
            }

            if (_previousMonthButton != null)
            {
                _previousMonthButton.gameObject.SetActive(state.CanGoPreviousMonth);
            }

            if (_nextMonthButton != null)
            {
                _nextMonthButton.gameObject.SetActive(state.CanGoNextMonth);
            }

            if (_playButtonLabel != null)
            {
                _playButtonLabel.text = state.SelectedDayState != null ? state.SelectedDayState.ActionLabel : "Play";
            }

            if (_playButton != null)
            {
                bool interactable = state.SelectedDayState != null && state.SelectedDayState.ActionEnabled;
                _playButton.interactable = interactable;
                var image = _playButton.GetComponent<Image>();
                if (image != null)
                {
                    image.color = interactable
                        ? GamePalette.PrimaryButton
                        : GamePalette.ActionButtonSurfaceDisabled;
                }
            }

            for (int index = 0; index < _dayViews.Count; index++)
            {
                DailyCalendarDayState dayState = state.Days != null && index < state.Days.Length
                    ? state.Days[index]
                    : new DailyCalendarDayState { IsEmpty = true };
                _dayViews[index].SetState(dayState);
            }
        }

        private void OnEnable()
        {
            ApplyResponsiveLayout(force: true);
        }

        private void LateUpdate()
        {
            ApplyResponsiveLayout(force: false);
        }

        private void OnDestroy()
        {
            if (_previousMonthButton != null)
            {
                _previousMonthButton.onClick.RemoveListener(HandlePreviousMonthClicked);
            }

            if (_nextMonthButton != null)
            {
                _nextMonthButton.onClick.RemoveListener(HandleNextMonthClicked);
            }

            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(HandlePlayClicked);
            }

            foreach (DailyCalendarDayView dayView in _dayViews)
            {
                if (dayView != null)
                {
                    dayView.Clicked -= HandleDayClicked;
                }
            }
        }

        private void Setup(
            RectTransform rootRect,
            RectTransform headerRect,
            RectTransform calendarRect,
            RectTransform daysGridRect,
            RectTransform playButtonRect,
            GridLayoutGroup daysGrid,
            Button previousMonthButton,
            Button nextMonthButton,
            Button playButton,
            Image headerIconImage,
            TMP_Text titleLabel,
            TMP_Text monthLabel,
            TMP_Text countLabel,
            TMP_Text playButtonLabel,
            TMP_Text previousLabel,
            TMP_Text nextLabel,
            List<TMP_Text> weekdayLabels,
            List<DailyCalendarDayView> dayViews,
            TMP_FontAsset regularFont,
            TMP_FontAsset boldFont)
        {
            TMP_FontAsset effectiveRegularFont = regularFont != null
                ? regularFont
                : boldFont != null
                    ? boldFont
                    : TMP_Settings.defaultFontAsset;
            TMP_FontAsset effectiveBoldFont = boldFont != null ? boldFont : effectiveRegularFont;

            _rootRect = rootRect;
            _headerRect = headerRect;
            _calendarRect = calendarRect;
            _daysGridRect = daysGridRect;
            _playButtonRect = playButtonRect;
            _daysGrid = daysGrid;
            _previousMonthButton = previousMonthButton;
            _nextMonthButton = nextMonthButton;
            _playButton = playButton;
            _headerIconImage = headerIconImage;
            _titleLabel = titleLabel;
            _monthLabel = monthLabel;
            _countLabel = countLabel;
            _playButtonLabel = playButtonLabel;
            _weekdayLabels.AddRange(weekdayLabels);
            _dayViews.AddRange(dayViews);

            ConfigureLabel(_titleLabel, effectiveBoldFont, HeaderTitleFontSize, GamePalette.DailyHeaderText, TextAlignmentOptions.Center);
            ConfigureLabel(_monthLabel, effectiveBoldFont, MonthLabelFontSize, GamePalette.DailyHeaderText, TextAlignmentOptions.Left);
            ConfigureLabel(_countLabel, effectiveBoldFont, CounterFontSize, GamePalette.DailyHeaderText, TextAlignmentOptions.Right);
            ConfigureLabel(_playButtonLabel, effectiveBoldFont, 30f, GamePalette.PrimaryText, TextAlignmentOptions.Center);
            ConfigureLabel(previousLabel, effectiveBoldFont, 36f, GamePalette.DailyHeaderText, TextAlignmentOptions.Center);
            ConfigureLabel(nextLabel, effectiveBoldFont, 36f, GamePalette.DailyHeaderText, TextAlignmentOptions.Center);

            foreach (TMP_Text weekdayLabel in _weekdayLabels)
            {
                ConfigureLabel(weekdayLabel, effectiveRegularFont, WeekdayFontSize, GamePalette.DailyWeekdayText, TextAlignmentOptions.Center);
            }

            foreach (DailyCalendarDayView dayView in _dayViews)
            {
                dayView.Clicked += HandleDayClicked;
                dayView.SetMetrics(74f, DayFontSize);
            }

            _previousMonthButton.onClick.AddListener(HandlePreviousMonthClicked);
            _nextMonthButton.onClick.AddListener(HandleNextMonthClicked);
            _playButton.onClick.AddListener(HandlePlayClicked);

            ApplyResponsiveLayout(force: true);
        }

        private void HandlePreviousMonthClicked()
        {
            PreviousMonthClicked?.Invoke();
        }

        private void HandleNextMonthClicked()
        {
            NextMonthClicked?.Invoke();
        }

        private void HandlePlayClicked()
        {
            PlayClicked?.Invoke(_selectedDate);
        }

        private void HandleDayClicked(DailyChallengeDateKey date)
        {
            DaySelected?.Invoke(date);
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (_rootRect == null)
            {
                return;
            }

            Rect rootRect = _rootRect.rect;
            Vector2 rootSize = new Vector2(rootRect.width, rootRect.height);
            if (rootSize.x <= 0f || rootSize.y <= 0f)
            {
                return;
            }

            if (!force && Vector2.Distance(_lastRootSize, rootSize) < 0.5f)
            {
                return;
            }

            _lastRootSize = rootSize;

            float scale = MobileLayout.GetScale(rootRect.width, rootRect.height);
            float headerHeight = MobileLayout.ClampScaled(HeaderHeight, MinHeaderHeight, MaxHeaderHeight, scale);
            float playButtonHeight = MobileLayout.ClampScaled(PlayButtonHeight, MinPlayButtonHeight, MaxPlayButtonHeight, scale);
            float playButtonBottomPadding = MobileLayout.ClampScaled(PlayButtonBottomPadding, MinPlayButtonBottomPadding, MaxPlayButtonBottomPadding, scale);
            float sidePadding = MobileLayout.ClampScaled(ContentSidePadding, MinContentSidePadding, MaxContentSidePadding, scale);
            float titleFontSize = MobileLayout.ClampScaled(HeaderTitleFontSize, MinHeaderTitleFontSize, MaxHeaderTitleFontSize, scale);
            float monthFontSize = MobileLayout.ClampScaled(MonthLabelFontSize, MinMonthLabelFontSize, MaxMonthLabelFontSize, scale);
            float countFontSize = MobileLayout.ClampScaled(CounterFontSize, MinCounterFontSize, MaxCounterFontSize, scale);
            float weekdayFontSize = MobileLayout.ClampScaled(WeekdayFontSize, MinWeekdayFontSize, MaxWeekdayFontSize, scale);
            float dayFontSize = MobileLayout.ClampScaled(DayFontSize, MinDayFontSize, MaxDayFontSize, scale);

            if (_headerRect != null)
            {
                _headerRect.sizeDelta = new Vector2(0f, headerHeight);
            }

            if (_calendarRect != null)
            {
                _calendarRect.offsetMin = new Vector2(sidePadding, playButtonBottomPadding + playButtonHeight + 20f);
                _calendarRect.offsetMax = new Vector2(-sidePadding, -headerHeight);
            }

            if (_playButtonRect != null)
            {
                _playButtonRect.sizeDelta = new Vector2(Mathf.Max(260f, rootRect.width - (sidePadding * 2f)), playButtonHeight);
                _playButtonRect.anchoredPosition = new Vector2(0f, playButtonBottomPadding);
            }

            if (_titleLabel != null)
            {
                _titleLabel.fontSize = titleFontSize;
            }

            if (_monthLabel != null)
            {
                _monthLabel.fontSize = monthFontSize;
            }

            if (_countLabel != null)
            {
                _countLabel.fontSize = countFontSize;
            }

            if (_playButtonLabel != null)
            {
                _playButtonLabel.fontSize = MobileLayout.ClampScaled(30f, 24f, 34f, scale);
            }

            foreach (TMP_Text weekdayLabel in _weekdayLabels)
            {
                weekdayLabel.fontSize = weekdayFontSize;
            }

            float gridWidth = _daysGridRect != null ? _daysGridRect.rect.width : Mathf.Max(1f, rootRect.width - (sidePadding * 2f));
            float cellSize = Mathf.Floor((gridWidth - 6f) / 7f);
            cellSize = Mathf.Clamp(cellSize, 36f, 82f);

            if (_daysGrid != null)
            {
                _daysGrid.cellSize = new Vector2(cellSize, cellSize);
                _daysGrid.spacing = new Vector2(1f, 6f * scale);
            }

            foreach (DailyCalendarDayView dayView in _dayViews)
            {
                dayView.SetMetrics(cellSize, dayFontSize);
            }

            if (_headerIconImage != null)
            {
                RectTransform headerIconRect = (RectTransform)_headerIconImage.transform;
                float iconSize = MobileLayout.ClampScaled(120f, 84f, 140f, scale);
                headerIconRect.sizeDelta = new Vector2(iconSize, iconSize);
            }
        }

        private static TMP_Text CreateText(Transform parent, string name, string text)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);
            TMP_Text label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.enableAutoSizing = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            return label;
        }

        private static (Button Button, TMP_Text Label) CreateArrowButton(Transform parent, string name, string text)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var image = buttonObject.GetComponent<Image>();
            image.color = Color.clear;

            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = image;

            TMP_Text label = CreateText(buttonObject.transform, "Label", text);
            RectTransform labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            return (button, label);
        }

        private static (Button Button, TMP_Text Label) CreatePlayButton(Transform parent, string name, string text)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var image = buttonObject.GetComponent<Image>();
            image.color = GamePalette.PrimaryButton;
            image.sprite = GetCircleSprite();
            image.type = Image.Type.Simple;

            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = image;

            TMP_Text label = CreateText(buttonObject.transform, "Label", text);
            RectTransform labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            return (button, label);
        }

        private static void ConfigureLabel(TMP_Text label, TMP_FontAsset font, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            label.font = font;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
        }

        private static void ConfigureRect(
            RectTransform rectTransform,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta,
            Vector2 anchoredPosition)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.anchoredPosition = anchoredPosition;
        }

        private static Sprite CreateTextureSprite(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
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
            _circleSprite.name = "DailyTabCircleSprite";
            return _circleSprite;
        }
    }
}
