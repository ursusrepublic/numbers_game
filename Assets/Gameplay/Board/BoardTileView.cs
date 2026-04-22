using System;
using System.Collections;
using Game.UI.Styling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.Board
{
    [DisallowMultipleComponent]
    public sealed class BoardTileView : MonoBehaviour
    {
        private Image _background;
        private TMP_Text _label;
        private Button _button;
        private Action<int> _clickHandler;
        private BoardCell _cell;
        private Coroutine _hintPulseRoutine;
        private bool _isHinted;
        private int _index;

        public static BoardTileView Create(Transform parent, TMP_FontAsset font, Action<int> clickHandler)
        {
            var tileObject = new GameObject(
                "BoardTile",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(BoardTileView));

            tileObject.transform.SetParent(parent, false);

            var tileImage = tileObject.GetComponent<Image>();
            tileImage.color = GamePalette.BoardTileNormalBackground;

            var tileButton = tileObject.GetComponent<Button>();
            tileButton.transition = Selectable.Transition.None;
            tileButton.targetGraphic = tileImage;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(tileObject.transform, false);

            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontStyle = FontStyles.Bold;
            label.fontSize = 88;
            label.alignment = TextAlignmentOptions.Center;
            label.color = GamePalette.BoardTileText;
            label.raycastTarget = false;
            label.enableAutoSizing = true;
            label.fontSizeMin = 54;
            label.fontSizeMax = 88;
            label.textWrappingMode = TextWrappingModes.NoWrap;

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

        private void Setup(Image background, TMP_Text label, Button button, Action<int> clickHandler)
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

            _label.text = _cell.Number.ToString();
            _label.color = _cell.IsMatched
                ? GamePalette.InactiveNumberColor
                : GamePalette.BoardTileText;
            _button.interactable = !_cell.IsMatched;

            if (_cell.IsMatched)
            {
                _background.color = Color.clear;
                return;
            }

            if (_cell.IsSelected)
            {
                _background.color = GamePalette.BoardTileSelectedBackground;
                return;
            }

            _background.color = _isHinted
                ? GamePalette.BoardTileHintBackground
                : Color.clear;
        }

        private IEnumerator PlayHintPulse()
        {
            for (int pulse = 0; pulse < 2; pulse++)
            {
                _background.color = GamePalette.BoardTileHintPulseBackground;
                yield return new WaitForSeconds(0.12f);
                ApplyVisualState();
                yield return new WaitForSeconds(0.08f);
            }

            _hintPulseRoutine = null;
            ApplyVisualState();
        }
    }
}
