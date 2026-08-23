using UnityEngine;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.Effects
{
    public class ParticleThemeManager : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] _themeBackgroundParticles; // 10 chapter themes

        public void SetTheme(int chapterId)
        {
            if (SaveSystem.Data.reduceMotion)
            {
                foreach (var ps in _themeBackgroundParticles) if (ps != null) ps.Stop();
                return;
            }

            for (int i = 0; i < _themeBackgroundParticles.Length; i++)
            {
                if (_themeBackgroundParticles[i] != null)
                {
                    if (i == chapterId - 1) _themeBackgroundParticles[i].Play();
                    else _themeBackgroundParticles[i].Stop();
                }
            }
        }
    }
}
