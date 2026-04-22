using System;
using Game.UI.Styling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.Dev
{
    [DisallowMultipleComponent]
    public sealed class DevPanelView : MonoBehaviour
    {
        public event Action ShowPairsClicked;
        public event Action SolveOnePairClicked;

        private TMP_Text _infoLabel;
        private Button _showPairsButton;
        private Button _solveOnePairButton;

        public static DevPanelView Create(Transform parent, TMP_FontAsset font)
        {
            var panelObject = new GameObject(
                "DevPanel",
                typeof(RectTransform),
                typeof(Image),
                typeof(DevPanelView));

            panelObject.transform.SetParent(parent, false);

            var panelRect = (RectTransform)panelObject.transform;
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.sizeDelta = new Vector2(340f, 420f);
            panelRect.anchoredPosition = new Vector2(-32f, -260f);

            var panelImage = panelObject.GetComponent<Image>();
            panelImage.color = GamePalette.DeveloperPanelBackground;

            TMP_Text titleLabel = CreateTextElement(panelObject.transform, "TitleLabel");
            ConfigureRect((RectTransform)titleLabel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(280f, 48f), new Vector2(0f, -26f));
            titleLabel.text = "Developer Mode";

            TMP_Text infoLabel = CreateTextElement(panelObject.transform, "InfoLabel");
            ConfigureRect((RectTransform)infoLabel.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            ((RectTransform)infoLabel.transform).offsetMin = new Vector2(20f, 120f);
            ((RectTransform)infoLabel.transform).offsetMax = new Vector2(-20f, -88f);

            (Button showPairsButton, TMP_Text showPairsLabel) = CreateButton(panelObject.transform, "ShowPairsButton", "Show All Pairs");
            ConfigureRect((RectTransform)showPairsButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(280f, 56f), new Vector2(0f, 94f));

            (Button solveOnePairButton, TMP_Text solveOnePairLabel) = CreateButton(panelObject.transform, "SolveOnePairButton", "Solve One Pair");
            ConfigureRect((RectTransform)solveOnePairButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(280f, 56f), new Vector2(0f, 26f));

            var devPanelView = panelObject.GetComponent<DevPanelView>();
            devPanelView.Setup(font, titleLabel, infoLabel, showPairsButton, showPairsLabel, solveOnePairButton, solveOnePairLabel);
            return devPanelView;
        }

        public void SetInfo(string info)
        {
            if (_infoLabel != null)
            {
                _infoLabel.text = info;
            }
        }

        private void OnDestroy()
        {
            if (_showPairsButton != null)
            {
                _showPairsButton.onClick.RemoveListener(HandleShowPairsClicked);
            }

            if (_solveOnePairButton != null)
            {
                _solveOnePairButton.onClick.RemoveListener(HandleSolveOnePairClicked);
            }
        }

        private void Setup(
            TMP_FontAsset font,
            TMP_Text titleLabel,
            TMP_Text infoLabel,
            Button showPairsButton,
            TMP_Text showPairsLabel,
            Button solveOnePairButton,
            TMP_Text solveOnePairLabel)
        {
            _infoLabel = infoLabel;
            _showPairsButton = showPairsButton;
            _solveOnePairButton = solveOnePairButton;

            ConfigureLabel(titleLabel, font, 34, TextAnchor.MiddleCenter, GamePalette.PrimaryText);
            ConfigureLabel(_infoLabel, font, 24, TextAnchor.UpperLeft, GamePalette.DeveloperPanelInfo);
            ConfigureLabel(showPairsLabel, font, 26, TextAnchor.MiddleCenter, GamePalette.PrimaryText);
            ConfigureLabel(solveOnePairLabel, font, 26, TextAnchor.MiddleCenter, GamePalette.PrimaryText);

            _showPairsButton.onClick.AddListener(HandleShowPairsClicked);
            _solveOnePairButton.onClick.AddListener(HandleSolveOnePairClicked);
        }

        private void HandleShowPairsClicked()
        {
            ShowPairsClicked?.Invoke();
        }

        private void HandleSolveOnePairClicked()
        {
            SolveOnePairClicked?.Invoke();
        }

        private static TextMeshProUGUI CreateTextElement(Transform parent, string name)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            return textObject.GetComponent<TextMeshProUGUI>();
        }

        private static (Button Button, TMP_Text Label) CreateButton(Transform parent, string name, string labelText)
        {
            var buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));

            buttonObject.transform.SetParent(parent, false);

            var buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = GamePalette.DeveloperPanelButton;

            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = buttonImage;

            TMP_Text label = CreateTextElement(buttonObject.transform, "Label");
            RectTransform labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label.text = labelText;
            label.raycastTarget = false;

            return (button, label);
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

        private static void ConfigureLabel(TMP_Text label, TMP_FontAsset font, int fontSize, TextAnchor alignment, Color color)
        {
            label.font = font;
            label.fontSize = fontSize;
            label.alignment = ConvertAlignment(alignment);
            label.color = color;
            label.enableAutoSizing = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
        }

        private static TextAlignmentOptions ConvertAlignment(TextAnchor alignment)
        {
            return alignment switch
            {
                TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
                TextAnchor.UpperCenter => TextAlignmentOptions.Top,
                TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
                TextAnchor.MiddleRight => TextAlignmentOptions.Right,
                TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
                _ => TextAlignmentOptions.Center,
            };
        }
    }
}
