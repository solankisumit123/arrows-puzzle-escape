using UnityEngine;
using UnityEngine.EventSystems;
using ArrowsPuzzle.Engine;

namespace ArrowsPuzzle.Gameplay
{
    public class InputHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private GridView _gridView;

        private BlockView _activeBlockView;
        private Vector2 _dragStartPos;
        private Vector2 _blockOriginalPos;
        private (int min, int max) _moveRange;
        private bool _isDragging;

        public void OnPointerDown(PointerEventData eventData)
        {
            var hitView = GetBlockUnderPointer(eventData);
            if (hitView != null)
            {
                _activeBlockView = hitView;
                _activeBlockView.OnPressed();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_activeBlockView != null && !_isDragging)
            {
                _activeBlockView.OnReleased();
                _activeBlockView = null;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_activeBlockView == null)
            {
                _activeBlockView = GetBlockUnderPointer(eventData);
            }

            if (_activeBlockView == null || GameManager.Instance.IsPaused || GameManager.Instance.IsLevelComplete)
            {
                return;
            }

            var block = _activeBlockView.BlockData;
            if (block.IsLocked || block.HasExited)
            {
                _activeBlockView = null;
                return;
            }

            _isDragging = true;
            _dragStartPos = eventData.position;
            var rt = _activeBlockView.GetComponent<RectTransform>();
            _blockOriginalPos = rt.anchoredPosition;
            _moveRange = GameManager.Instance.Engine.GetValidMoveRange(block.Id);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _activeBlockView == null) return;

            var block = _activeBlockView.BlockData;
            Vector2 totalDelta = eventData.position - _dragStartPos;
            float cs = _gridView.CellSize;
            var rt = _activeBlockView.GetComponent<RectTransform>();

            if (block.Orientation == BlockOrientation.Horizontal)
            {
                float clampedDeltaX = Mathf.Clamp(totalDelta.x, _moveRange.min * cs, _moveRange.max * cs);
                rt.anchoredPosition = new Vector2(_blockOriginalPos.x + clampedDeltaX, _blockOriginalPos.y);
            }
            else
            {
                // In UI canvas, downward dragging gives negative screen Y, but grid positive row offset moves down (-Y)
                float rawOffset = -totalDelta.y / cs;
                float clampedOffset = Mathf.Clamp(rawOffset, _moveRange.min, _moveRange.max);
                rt.anchoredPosition = new Vector2(_blockOriginalPos.x, _blockOriginalPos.y - (clampedOffset * cs));
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging || _activeBlockView == null) return;

            var block = _activeBlockView.BlockData;
            float cs = _gridView.CellSize;
            var rt = _activeBlockView.GetComponent<RectTransform>();
            Vector2 displacement = rt.anchoredPosition - _blockOriginalPos;

            int targetOffset = 0;
            if (block.Orientation == BlockOrientation.Horizontal)
            {
                targetOffset = Mathf.RoundToInt(displacement.x / cs);
            }
            else
            {
                targetOffset = Mathf.RoundToInt(-displacement.y / cs);
            }

            // Clamp to allowed range
            targetOffset = Mathf.Clamp(targetOffset, _moveRange.min, _moveRange.max);

            if (targetOffset != 0)
            {
                bool success = GameManager.Instance.TryMoveBlock(block.Id, targetOffset);
                if (success)
                {
                    if (MoveValidator.WouldExitGate(block, targetOffset, GameManager.Instance.Engine.Grid, new System.Collections.Generic.List<Gate>(GameManager.Instance.Engine.Gates)))
                    {
                        _activeBlockView.AnimateExit(targetOffset, block.Orientation == BlockOrientation.Horizontal, cs);
                    }
                    else
                    {
                        _activeBlockView.AnimateSlide(targetOffset, block.Orientation == BlockOrientation.Horizontal, cs);
                    }
                }
                else
                {
                    // Snap back
                    rt.anchoredPosition = _blockOriginalPos;
                }
            }
            else
            {
                rt.anchoredPosition = _blockOriginalPos;
            }

            _activeBlockView.OnReleased();
            _activeBlockView = null;
            _isDragging = false;
        }

        private BlockView GetBlockUnderPointer(PointerEventData eventData)
        {
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            foreach (var r in results)
            {
                var bv = r.gameObject.GetComponentInParent<BlockView>();
                if (bv != null) return bv;
            }
            return null;
        }
    }
}
