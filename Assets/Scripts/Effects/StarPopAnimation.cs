using UnityEngine;
using DG.Tweening;
using ArrowsPuzzle.Systems;

namespace ArrowsPuzzle.Effects
{
    public static class StarPopAnimation
    {
        public static void Pop(Transform starTransform, float delay = 0f)
        {
            if (SaveSystem.Data.reduceMotion)
            {
                starTransform.localScale = Vector3.one;
                return;
            }

            starTransform.localScale = Vector3.zero;
            starTransform.DOScale(1.3f, 0.25f)
                .SetDelay(delay)
                .SetEase(Ease.OutBack)
                .OnComplete(() => starTransform.DOScale(1f, 0.1f));
        }
    }
}
