using UnityEngine;
using DG.Tweening;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.Effects
{
    public static class BlockAnimator
    {
        public static void PlaySquashStretch(Transform target, bool isHorizontal)
        {
            if (SaveSystem.Data.reduceMotion) return;
            Vector3 squash = isHorizontal ? new Vector3(0.85f, 1.15f, 1f) : new Vector3(1.15f, 0.85f, 1f);
            target.DOScale(squash, 0.08f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                target.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutBack);
            });
        }

        public static void PlayImpactThud(Transform target, Vector2 impactDir)
        {
            if (SaveSystem.Data.reduceMotion) return;
            target.DOPunchPosition(impactDir * 8f, 0.15f, 10, 1);
        }
    }
}
