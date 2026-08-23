using UnityEngine;
using UnityEngine.UI;
using ArrowsPuzzle.Engine;

namespace ArrowsPuzzle.Gameplay
{
    [RequireComponent(typeof(RectTransform))]
    public class GateView : MonoBehaviour
    {
        public Gate GateData { get; private set; }

        [SerializeField] private Image _gateFrameImage;
        [SerializeField] private Image _gateGlowImage;
        [SerializeField] private Image _indicatorArrowImage;

        private RectTransform _rt;

        private static readonly Color[] GateColors = new Color[]
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

        private void Awake() => _rt = GetComponent<RectTransform>();

        public void Initialize(Gate gate, float cellSize, int gridSize)
        {
            GateData = gate;
            SetColor(gate.Color);
            PositionGate(gate, cellSize, gridSize);
        }

        private void SetColor(string colorName)
        {
            int idx = System.Array.IndexOf(ColorNames, colorName.ToLower());
            Color c = idx >= 0 ? GateColors[idx] : Color.white;
            if (_gateFrameImage != null) _gateFrameImage.color = c;
            if (_gateGlowImage != null) _gateGlowImage.color = new Color(c.r, c.g, c.b, 0.45f);
        }

        private void PositionGate(Gate gate, float cellSize, int gridSize)
        {
            float gateThickness = 12f;
            float totalGridSpan = cellSize * gridSize;

            switch (gate.Side)
            {
                case GateSide.Left:
                    _rt.sizeDelta = new Vector2(gateThickness, cellSize);
                    _rt.anchoredPosition = new Vector2(-gateThickness, -gate.Index * cellSize);
                    if (_indicatorArrowImage != null) _indicatorArrowImage.transform.localRotation = Quaternion.Euler(0, 0, 180);
                    break;
                case GateSide.Right:
                    _rt.sizeDelta = new Vector2(gateThickness, cellSize);
                    _rt.anchoredPosition = new Vector2(totalGridSpan, -gate.Index * cellSize);
                    if (_indicatorArrowImage != null) _indicatorArrowImage.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    break;
                case GateSide.Top:
                    _rt.sizeDelta = new Vector2(cellSize, gateThickness);
                    _rt.anchoredPosition = new Vector2(gate.Index * cellSize, gateThickness);
                    if (_indicatorArrowImage != null) _indicatorArrowImage.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    break;
                case GateSide.Bottom:
                    _rt.sizeDelta = new Vector2(cellSize, gateThickness);
                    _rt.anchoredPosition = new Vector2(gate.Index * cellSize, -totalGridSpan);
                    if (_indicatorArrowImage != null) _indicatorArrowImage.transform.localRotation = Quaternion.Euler(0, 0, -90);
                    break;
            }
        }
    }
}
