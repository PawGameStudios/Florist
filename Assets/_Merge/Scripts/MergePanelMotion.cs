using System;
using DG.Tweening;
using UnityEngine;

namespace Florist.Merge
{
    // Owned by the scene/prefab controller; no hierarchy lookup or runtime components.
    public sealed class MergePanelMotion
    {
        private readonly Transform target;
        private readonly CanvasGroup group;
        private readonly Vector3 scale;
        private Tween tween;

        public MergePanelMotion(Transform target, CanvasGroup group)
        {
            this.target = target;
            this.group = group;
            scale = target.localScale;
        }

        public void Show()
        {
            Reset();
            target.localScale = scale * .97f;
            var sequence = DOTween.Sequence().SetUpdate(true);
            sequence.Append(target.DOScale(scale, .14f).SetEase(Ease.OutQuad));
            if (group != null)
            {
                group.alpha = 0f;
                sequence.Join(group.DOFade(1f, .14f));
            }
            tween = sequence;
        }

        public void Hide(Action completed)
        {
            tween?.Kill();
            if (group != null) group.interactable = false;
            var sequence = DOTween.Sequence().SetUpdate(true);
            sequence.Append(target.DOScale(scale * .97f, .14f).SetEase(Ease.InQuad));
            if (group != null) sequence.Join(group.DOFade(0f, .14f));
            sequence.OnComplete(() => { Reset(); completed?.Invoke(); });
            tween = sequence;
        }

        public void Reset()
        {
            tween?.Kill();
            tween = null;
            if (target != null) target.localScale = scale;
            if (group != null)
            {
                group.alpha = 1f;
                group.interactable = true;
            }
        }
    }
}
