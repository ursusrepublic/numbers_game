using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.Board
{
    [DisallowMultipleComponent]
    public sealed class BoardTileView : MonoBehaviour
    {
        private static readonly Color NormalBackgroundColor = new Color(0.94f, 0.95f, 0.96f, 1f);
        private static readonly Color SelectedBackgroundColor = new Color(1f, 0.84f, 0.32f, 1f);
        private static readonly Color ClearedBackgroundColor = new Color(0.22f, 0.24f, 0.28f, 0.9f);
        private static readonly Color TextColor = new Color(0.12f, 0.14f, 0.18f, 1f);

        private Image _background;
        private Text _label;
        private Button _button;
        private Action<int> _clickHandler;
        private int _index;

        public static BoardTileView Create(Transform parent, Font font, Action<int> clickHandler)
        {
            var tileObject = new GameObject(
                "BoardTile",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(BoardTileView));

            tileObject.transform.SetParent(parent, false);

            var tileImage = tileObject.GetComponent<Image>();
            tileImage.color = NormalBackgroundColor;

            var tileButton = tileObject.GetComponent<Button>();
            tileButton.transition = Selectable.Transition.None;
            tileButton.targetGraphic = tileImage;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(tileObject.transform, false);

            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelObject.GetComponent<Text>();
            label.font = font;
            label.fontStyle = FontStyle.Bold;
            label.fontSize = 32;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = TextColor;
            label.raycastTarget = false;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 18;
            label.resizeTextMaxSize = 42;

            var tileView = tileObject.GetComponent<BoardTileView>();
            tileView.Setup(tileImage, label, tileButton, clickHandler);
            return tileView;
        }

        public void SetCell(BoardCell cell)
        {
            _index = cell.Index;
            gameObject.name = $"BoardTile_{cell.Index:00}";
            _label.text = cell.IsMatched ? string.Empty : cell.Number.ToString();
            _button.interactable = !cell.IsMatched;

            if (cell.IsMatched)
            {
                _background.color = ClearedBackgroundColor;
                return;
            }

            _background.color = cell.IsSelected ? SelectedBackgroundColor : NormalBackgroundColor;
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClicked);
            }
        }

        private void Setup(Image background, Text label, Button button, Action<int> clickHandler)
        {
            _background = background;
            _label = label;
            _button = button;
            _clickHandler = clickHandler;

            _button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            _clickHandler?.Invoke(_index);
        }
    }
}
