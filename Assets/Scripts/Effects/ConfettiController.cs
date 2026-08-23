using UnityEngine;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.Effects
{
    public class ConfettiController : MonoBehaviour
    {
        public static ConfettiController Instance { get; private set; }

        [SerializeField] private ParticleSystem _confettiParticles;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Burst()
        {
            if (SaveSystem.Data.reduceMotion) return;
            if (_confettiParticles != null)
            {
                _confettiParticles.Play();
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.sfxConfetti);
            }
        }
    }
}
