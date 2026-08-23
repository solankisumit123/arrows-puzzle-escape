using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ArrowsPuzzle.Engine;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.Gameplay
{
    /// <summary>
    /// Visual representation of a single block on the grid.
    /// Handles rendering, animations, special type visuals, hint glow.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BlockView : MonoBehaviour
    {
        // ── Block Data ───────────────────────────────────────────────────────
        public string BlockId { get; private set; }
        public Block BlockData { get; private set; }

        // ── Visual Components ────────────────────────────────────────────────
        [SerializeField] private Image _bgImage;
        [SerializeField] private Image _borderImage;
        [SerializeField] private Image _iconImage;         // arrow direction icon
        [SerializeField] private Image _overlayImage;      // lock, ice crystal, etc.
        [SerializeField] private Image _colorblindShapeImage; // colorblind mode shape
        [SerializeField] private Image _glowImage;         // hint glow outline
        [SerializeField] private RawImage _photoTileImage; // for custom image puzzle mode
        [SerializeField] private ParticleSystem _exitParticles;

        // ── Skin / Theme ──────────────────────────────────────────────────────
        private static readonly Color[] BlockColors = new Color[]
        {
            new Color(0.92f, 0.25f, 0.25f), // red
            new Color(0.25f, 0.55f, 0.95f), // blue
            new Color(0.20f, 0.80f, 0.40f), // green
            new Color(0.98f, 0.78f, 0.10f), // yellow
            new Color(0.75f, 0.30f, 0.90f), // purple
            new Color(0.98f, 0.55f, 0.10f), // orange
            new Color(0.20f, 0.85f, 0.85f), // cyan
            new Color(0.95f, 0.30f, 0.70f), // pink
        };

        private static readonly string[] ColorNames = { "red","blue","green","yellow","purple","orange","cyan","pink" };

        private RectTransform _rt;
        private Sequence _slideSeq;
        private Tweener _glowTween;
        private bool _isPhotoMode;
        private float _cellSize;

        // ── Init ─────────────────────────────────────────────────────────────

        public void Initialize(Block block, float cellSize, bool photoMode, Texture2D photoTile = null)
        {
            BlockId = block.Id;
            BlockData = block;
            _cellSize = cellSize;
            _isPhotoMode = photoMode;

            SetSize(block, cellSize);
            SetColor(block.Color);
            SetIcon(block);
            SetSpecialOverlay(block);

            if (photoMode && photoTile != null)
                _photoTileImage.texture = photoTile;

            _glowImage.gameObject.SetActive(false);

            if (SaveSystem.Data.colorblindMode)
                EnableColorblindShape(block.Color);
        }

        private void Awake() => _rt = GetComponent<RectTransform>();

        private void SetSize(Block block, float cs)
        {
            float w = block.Orientation == BlockOrientation.Horizontal ? cs * block.Length : cs;
            float h = block.Orientation == BlockOrientation.Vertical   ? cs * block.Length : cs;
            _rt.sizeDelta = new Vector2(w - 4, h - 4); // 4px inset gap
        }

        private void SetColor(string colorName)
        {
            int idx = System.Array.IndexOf(ColorNames, colorName.ToLower());
            Color c = idx >= 0 ? BlockColors[idx] : Color.gray;
            if (_bgImage != null) _bgImage.color = new Color(c.r * 0.85f, c.g * 0.85f, c.b * 0.85f);
            if (_borderImage != null) _borderImage.color = c;
        }

        private void SetIcon(Block block)
        {
            if (_iconImage == null) return;
            // TODO: Assign sprite from Resources/Sprites/Icons/arrow_{H|V}
            // arrow_H = left-right double arrow, arrow_V = up-down double arrow
            // For one-way blocks use directional single arrows
            string iconName = block.Type == BlockType.OneWay
                ? (block.Orientation == BlockOrientation.Horizontal
                    ? (block.OneWayPositive ? "arrow_right" : "arrow_left")
                    : (block.OneWayPositive ? "arrow_down"  : "arrow_up"))
                : (block.Orientation == BlockOrientation.Horizontal ? "arrow_lr" : "arrow_ud");
            _iconImage.sprite = Resources.Load<Sprite>($"Sprites/Icons/{iconName}");
        }

        private void SetSpecialOverlay(Block block)
        {
            if (_overlayImage == null) return;
            string overlayName = block.Type switch
            {
                BlockType.Locked    => "overlay_lock",
                BlockType.Key       => "overlay_key",
                BlockType.Ice       => "overlay_ice",
                BlockType.Teleporter => "overlay_teleport",
                BlockType.Bomb      => "overlay_bomb",
                _                   => null
            };
            _overlayImage.gameObject.SetActive(overlayName != null);
            if (overlayName != null)
                _overlayImage.sprite = Resources.Load<Sprite>($"Sprites/Overlays/{overlayName}");
        }

        private void EnableColorblindShape(string colorName)
        {
            if (_colorblindShapeImage == null) return;
            // Each color has a unique icon shape (circle, triangle, square, diamond, etc.)
            _colorblindShapeImage.gameObject.SetActive(true);
            _colorblindShapeImage.sprite = Resources.Load<Sprite>($"Sprites/Colorblind/shape_{colorName}");
        }

        // ── Positioning ───────────────────────────────────────────────────────

        public void SetGridPosition(int row, int col, float cellSize)
        {
            _rt.anchoredPosition = new Vector2(col * cellSize, -row * cellSize);
        }

        // ── Slide Animation ───────────────────────────────────────────────────

        public void AnimateSlide(int offset, bool isHorizontal, float cellSize, TweenCallback onComplete = null)
        {
            _slideSeq?.Kill();
            Vector2 delta = isHorizontal
                ? new Vector2(offset * cellSize, 0)
                : new Vector2(0, -offset * cellSize);
            Vector2 targetPos = _rt.anchoredPosition + delta;

            // Squash-and-stretch: compress on axis of motion at start
            float squashX = isHorizontal ? 0.92f : 1.05f;
            float squashY = isHorizontal ? 1.05f : 0.92f;

            _slideSeq = DOTween.Sequence();
            if (!SaveSystem.Data.reduceMotion)
            {
                _slideSeq.Append(transform.DOScaleX(squashX, 0.05f));
                _slideSeq.Join(transform.DOScaleY(squashY, 0.05f));
            }
            _slideSeq.Append(_rt.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutCubic));
            if (!SaveSystem.Data.reduceMotion)
            {
                _slideSeq.Append(transform.DOScale(Vector3.one, 0.08f).SetEase(Ease.OutBack));
            }
            if (onComplete != null) _slideSeq.OnComplete(onComplete);
        }

        // ── Exit Animation ────────────────────────────────────────────────────

        public void AnimateExit(int offset, bool isHorizontal, float cellSize)
        {
            Vector2 delta = isHorizontal
                ? new Vector2(offset * cellSize * 2, 0)
                : new Vector2(0, -offset * cellSize * 2);
            Vector2 target = _rt.anchoredPosition + delta;

            _exitParticles?.Play();

            var seq = DOTween.Sequence();
            seq.Append(_rt.DOAnchorPos(target, 0.3f).SetEase(Ease.InCubic));
            seq.Join(transform.DOScale(0f, 0.3f).SetEase(Ease.InBack));
            seq.OnComplete(() => Destroy(gameObject));
        }

        // ── Hint Glow ─────────────────────────────────────────────────────────

        public void ShowHintGlow(bool show, bool isPositiveDirection = true)
        {
            _glowImage.gameObject.SetActive(show);
            _glowTween?.Kill();
            if (show && !SaveSystem.Data.reduceMotion)
            {
                _glowTween = _glowImage.DOFade(0.3f, 0.6f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        // ── Locked visual ─────────────────────────────────────────────────────

        public void UpdateLockedState(bool isLocked)
        {
            _overlayImage?.gameObject.SetActive(isLocked || BlockData.Type == BlockType.Ice);
            if (!isLocked && BlockData.Type == BlockType.Locked)
            {
                // Unlock flash animation
                if (!SaveSystem.Data.reduceMotion)
                    transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 5);
            }
        }

        // ── Press feedback ────────────────────────────────────────────────────

        public void OnPressed()
        {
            if (!SaveSystem.Data.reduceMotion)
                transform.DOScale(0.95f, 0.08f).SetEase(Ease.OutQuad);
        }

        public void OnReleased()
        {
            if (!SaveSystem.Data.reduceMotion)
                transform.DOScale(1f, 0.12f).SetEase(Ease.OutBack);
        }
    }
}
