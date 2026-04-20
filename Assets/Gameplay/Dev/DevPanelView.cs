using System;
using Game.UI.Styling;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.Dev
{
    [DisallowMultipleComponent]
    public sealed class DevPanelView : MonoBehaviour
    {
        public event Action ShowPairsClicked;
        public event Action SolveOnePairClicked;

        private Text _infoLabel;
        private Button _showPairsButton;
        private Button _solveOnePairButton;

        public static DevPanelView Create(Transform parent)
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

            Text titleLabel = CreateTextElement(panelObject.transform, "TitleLabel");
            ConfigureRect((RectTransform)titleLabel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(280f, 48f), new Vector2(0f, -26f));
            titleLabel.text = "Developer Mode";

            Text infoLabel = CreateTextElement(panelObject.transform, "InfoLabel");
            ConfigureRect((RectTransform)infoLabel.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            ((RectTransform)infoLabel.transform).offsetMin = new Vector2(20f, 120f);
            ((RectTransform)infoLabel.transform).offsetMax = new Vector2(-20f, -88f);

            (Button showPairsButton, Text showPairsLabel) = CreateButton(panelObject.transform, "ShowPairsButton", "Show All Pairs");
            ConfigureRect((RectTransform)showPairsButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(280f, 56f), new Vector2(0f, 94f));

            (Button solveOnePairButton, Text solveOnePairLabel) = CreateButton(panelObject.transform, "SolveOnePairButton", "Solve One Pair");
            ConfigureRect((RectTransform)solveOnePairButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(280f, 56f), new Vector2(0f, 26f));

            var devPanelView = panelObject.GetComponent<DevPanelView>();
            devPanelView.Setup(titleLabel, infoLabel, showPairsButton, showPairsLabel, solveOnePairButton, solveOnePairLabel);
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
            Text titleLabel,
            Text infoLabel,
            Button showPairsButton,
            Text showPairsLabel,
            Button solveOnePairButton,
            Text solveOnePairLabel)
        {
            _infoLabel = infoLabel;
            _showPairsButton = showPairsButton;
            _solveOnePairButton = solveOnePairButton;

            ConfigureLabel(titleLabel, 34, TextAnchor.MiddleCenter, GamePalette.PrimaryText);
            ConfigureLabel(_infoLabel, 24, TextAnchor.UpperLeft, GamePalette.DeveloperPanelInfo);
            ConfigureLabel(showPairsLabel, 26, TextAnchor.MiddleCenter, GamePalette.PrimaryText);
            ConfigureLabel(solveOnePairLabel, 26, TextAnchor.MiddleCenter, GamePalette.PrimaryText);

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

        private static Text CreateTextElement(Transform parent, string name)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            return textObject.GetComponent<Text>();
        }

        private static (Button Button, Text Label) CreateButton(Transform parent, string name, string labelText)
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

            Text label = CreateTextElement(buttonObject.transform, "Label");
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

        private static void ConfigureLabel(Text label, int fontSize, TextAnchor alignment, Color color)
        {
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }
}
