using System;
using System.Collections;
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
        private static readonly Color HintBackgroundColor = new Color(0.46f, 0.84f, 0.74f, 1f);
        private static readonly Color HintPulseBackgroundColor = new Color(0.86f, 0.97f, 0.84f, 1f);
        private static readonly Color TextColor = new Color(0.12f, 0.14f, 0.18f, 1f);

        private Image _background;
        private Text _label;
        private Button _button;
        private Action<int> _clickHandler;
        private BoardCell _cell;
        private Coroutine _hintPulseRoutine;
        private bool _isHinted;
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
            _cell = cell;
            _index = cell.Index;
            gameObject.name = $"BoardTile_{cell.Index:00}";
            ApplyVisualState();
        }

        public void SetHinted(bool isHinted)
        {
            _isHinted = isHinted;
            ApplyVisualState();
        }

        public void ReplayHintFeedback()
        {
            if (_cell == null || _cell.IsMatched || !_isHinted)
            {
                return;
            }

            if (_hintPulseRoutine != null)
            {
                StopCoroutine(_hintPulseRoutine);
            }

            _hintPulseRoutine = StartCoroutine(PlayHintPulse());
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

        private void ApplyVisualState()
        {
            if (_cell == null)
            {
                return;
            }

            _label.text = _cell.IsMatched ? string.Empty : _cell.Number.ToString();
            _button.interactable = !_cell.IsMatched;

            if (_cell.IsMatched)
            {
                _background.color = ClearedBackgroundColor;
                return;
            }

            if (_cell.IsSelected)
            {
                _background.color = SelectedBackgroundColor;
                return;
            }

            _background.color = _isHinted ? HintBackgroundColor : NormalBackgroundColor;
        }

        private IEnumerator PlayHintPulse()
        {
            for (int pulse = 0; pulse < 2; pulse++)
            {
                _background.color = HintPulseBackgroundColor;
                yield return new WaitForSeconds(0.12f);
                ApplyVisualState();
                yield return new WaitForSeconds(0.08f);
            }

            _hintPulseRoutine = null;
            ApplyVisualState();
        }
    }
}
