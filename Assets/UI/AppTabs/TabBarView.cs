using System;
using System.Collections.Generic;
using Game.UI.Layout;
using Game.UI.Styling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.AppTabs
{
    [DisallowMultipleComponent]
    public sealed class TabBarView : MonoBehaviour
    {
        private const float ButtonWidth = 190f;
        private const float MinButtonWidth = 120f;
        private const float MaxButtonWidth = 240f;
        private const float ButtonHeight = 104f;
        private const float MinButtonHeight = 84f;
        private const float MaxButtonHeight = 120f;
        private const float IconSize = 64f;
        private const float MinIconSize = 50f;
        private const float MaxIconSize = 68f;
        private const float LabelFontSize = 42f;
        private const float MinLabelFontSize = 36f;
        private const float MaxLabelFontSize = 44f;
        private const float RowSpacing = 4f;
        private const float MinRowSpacing = 2f;
        private const float MaxRowSpacing = 6f;
        private const float BarTopPadding = 8f;
        private const float MinBarTopPadding = 4f;
        private const float MaxBarTopPadding = 12f;

        private sealed class TabButtonElements
        {
            public AppTabId Id;
            public Button Button;
            public Image Icon;
            public TMP_Text Label;
            public LayoutElement Layout;
            public RectTransform IconRect;
        }

        private readonly List<TabButtonElements> _buttons = new List<TabButtonElements>();

        private RectTransform _rootRect;
        private RectTransform _buttonRowRect;
        private HorizontalLayoutGroup _buttonRowLayout;
        private TMP_FontAsset _regularFont;
        private AppTabId _selectedTab = AppTabId.Main;
        private Vector2 _lastRootSize = new Vector2(-1f, -1f);

        public event Action<AppTabId> TabSelected;

        public static TabBarView Create(
            Transform parent,
            TMP_FontAsset regularFont,
            Texture2D mainIconTexture,
            Texture2D dailyIconTexture,
            Texture2D journeyIconTexture,
            Texture2D meIconTexture)
        {
            var rootObject = new GameObject(
                "TabBarView",
                typeof(RectTransform),
                typeof(Image),
                typeof(TabBarView));

            rootObject.transform.SetParent(parent, false);

            var rootRect = (RectTransform)rootObject.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var rootImage = rootObject.GetComponent<Image>();
            rootImage.color = GamePalette.TabBarBackground;

            var borderObject = new GameObject("TopBorder", typeof(RectTransform), typeof(Image));
            borderObject.transform.SetParent(rootObject.transform, false);

            var borderRect = (RectTransform)borderObject.transform;
            borderRect.anchorMin = new Vector2(0f, 1f);
            borderRect.anchorMax = new Vector2(1f, 1f);
            borderRect.pivot = new Vector2(0.5f, 1f);
            borderRect.sizeDelta = new Vector2(0f, 1f);
            borderRect.anchoredPosition = Vector2.zero;

            var borderImage = borderObject.GetComponent<Image>();
            borderImage.color = GamePalette.TabBarBorder;

            var buttonRowObject = new GameObject(
                "ButtonRow",
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup));

            buttonRowObject.transform.SetParent(rootObject.transform, false);

            var buttonRowRect = (RectTransform)buttonRowObject.transform;
            buttonRowRect.anchorMin = Vector2.zero;
            buttonRowRect.anchorMax = Vector2.one;
            buttonRowRect.offsetMin = Vector2.zero;
            buttonRowRect.offsetMax = Vector2.zero;

            var buttonRowLayout = buttonRowObject.GetComponent<HorizontalLayoutGroup>();
            buttonRowLayout.childAlignment = TextAnchor.MiddleCenter;
            buttonRowLayout.spacing = 0f;
            buttonRowLayout.padding = new RectOffset(0, 0, 0, 0);
            buttonRowLayout.childControlWidth = false;
            buttonRowLayout.childControlHeight = false;
            buttonRowLayout.childForceExpandWidth = false;
            buttonRowLayout.childForceExpandHeight = false;

            var view = rootObject.GetComponent<TabBarView>();
            view.Setup(rootRect, buttonRowRect, buttonRowLayout, regularFont);

            view.AddButton(buttonRowObject.transform, AppTabId.Main, "Main", mainIconTexture);
            view.AddButton(buttonRowObject.transform, AppTabId.Daily, "Daily", dailyIconTexture);
            view.AddButton(buttonRowObject.transform, AppTabId.Journey, "Journey", journeyIconTexture);
            view.AddButton(buttonRowObject.transform, AppTabId.Me, "Me", meIconTexture);

            view.SetSelectedTab(AppTabId.Main);
            return view;
        }

        public void SetSelectedTab(AppTabId selectedTab)
        {
            _selectedTab = selectedTab;

            foreach (TabButtonElements button in _buttons)
            {
                bool isSelected = button.Id == selectedTab;
                if (button.Icon != null)
                {
                    button.Icon.color = isSelected
                        ? GamePalette.TabBarActive
                        : GamePalette.TabBarInactive;
                }

                if (button.Label != null)
                {
                    button.Label.color = isSelected
                        ? GamePalette.TabBarActive
                        : GamePalette.TabBarInactive;
                }
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

        private void Setup(
            RectTransform rootRect,
            RectTransform buttonRowRect,
            HorizontalLayoutGroup buttonRowLayout,
            TMP_FontAsset regularFont)
        {
            _rootRect = rootRect;
            _buttonRowRect = buttonRowRect;
            _buttonRowLayout = buttonRowLayout;
            _regularFont = regularFont != null ? regularFont : TMP_Settings.defaultFontAsset;
        }

        private void AddButton(Transform parent, AppTabId tabId, string labelText, Texture2D iconTexture)
        {
            var buttonObject = new GameObject(
                $"{tabId}TabButton",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement),
                typeof(VerticalLayoutGroup));

            buttonObject.transform.SetParent(parent, false);

            var buttonRect = (RectTransform)buttonObject.transform;
            buttonRect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

            var buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = Color.clear;

            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = buttonImage;

            var layout = buttonObject.GetComponent<LayoutElement>();
            layout.preferredWidth = ButtonWidth;
            layout.preferredHeight = ButtonHeight;
            layout.minWidth = ButtonWidth;
            layout.minHeight = ButtonHeight;

            var verticalLayout = buttonObject.GetComponent<VerticalLayoutGroup>();
            verticalLayout.childAlignment = TextAnchor.MiddleCenter;
            verticalLayout.spacing = RowSpacing;
            verticalLayout.padding = new RectOffset(0, 0, (int)BarTopPadding, 0);
            verticalLayout.childControlWidth = false;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = false;
            verticalLayout.childForceExpandHeight = false;

            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconObject.transform.SetParent(buttonObject.transform, false);

            var iconLayout = iconObject.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = IconSize;
            iconLayout.preferredHeight = IconSize;
            iconLayout.minWidth = IconSize;
            iconLayout.minHeight = IconSize;

            var iconRect = (RectTransform)iconObject.transform;
            iconRect.sizeDelta = new Vector2(IconSize, IconSize);

            var iconImage = iconObject.GetComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            iconImage.sprite = CreateTextureSprite(iconTexture);
            iconImage.enabled = iconImage.sprite != null;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            labelObject.transform.SetParent(buttonObject.transform, false);

            var labelLayout = labelObject.GetComponent<LayoutElement>();
            labelLayout.preferredWidth = ButtonWidth;
            labelLayout.preferredHeight = 28f;

            TMP_Text label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = _regularFont;
            label.fontSize = LabelFontSize;
            label.text = labelText;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;

            button.onClick.AddListener(() => HandleButtonClicked(tabId));

            _buttons.Add(new TabButtonElements
            {
                Id = tabId,
                Button = button,
                Icon = iconImage,
                Label = label,
                Layout = layout,
                IconRect = iconRect
            });
        }

        private void HandleButtonClicked(AppTabId tabId)
        {
            if (_selectedTab == tabId)
            {
                return;
            }

            TabSelected?.Invoke(tabId);
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
            float buttonHeight = MobileLayout.ClampScaled(ButtonHeight, MinButtonHeight, MaxButtonHeight, scale);
            float maxButtonWidth = rootRect.width / Mathf.Max(1, _buttons.Count);
            float buttonWidth = Mathf.Min(
                MobileLayout.ClampScaled(ButtonWidth, MinButtonWidth, MaxButtonWidth, scale),
                maxButtonWidth);
            float iconSize = MobileLayout.ClampScaled(IconSize, MinIconSize, MaxIconSize, scale);
            float labelFontSize = MobileLayout.ClampScaled(LabelFontSize, MinLabelFontSize, MaxLabelFontSize, scale);
            float rowSpacing = MobileLayout.ClampScaled(RowSpacing, MinRowSpacing, MaxRowSpacing, scale);
            int topPadding = Mathf.RoundToInt(MobileLayout.ClampScaled(BarTopPadding, MinBarTopPadding, MaxBarTopPadding, scale));

            if (_buttonRowLayout != null)
            {
                _buttonRowLayout.spacing = 0f;
            }

            foreach (TabButtonElements button in _buttons)
            {
                if (button.Layout != null)
                {
                    button.Layout.preferredWidth = buttonWidth;
                    button.Layout.preferredHeight = buttonHeight;
                    button.Layout.minWidth = buttonWidth;
                    button.Layout.minHeight = buttonHeight;
                }

                if (button.IconRect != null)
                {
                    button.IconRect.sizeDelta = new Vector2(iconSize, iconSize);
                }

                if (button.Label != null)
                {
                    button.Label.fontSize = labelFontSize;
                }

                if (button.Button != null)
                {
                    VerticalLayoutGroup verticalLayout = button.Button.GetComponent<VerticalLayoutGroup>();
                    if (verticalLayout != null)
                    {
                        verticalLayout.spacing = rowSpacing;
                        verticalLayout.padding = new RectOffset(0, 0, topPadding, 0);
                    }
                }
            }
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
    }
}
