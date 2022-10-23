using DG.Tweening;
using UnityEngine;
using System;

namespace Collapse.Blocks
{
    /**
     * Normal Block specific behavior
     */
    public class NormalBlock : Block
    {

        public Action OnTriggerComplete = null;

        public override void Trigger(float delay)
        {
            if (IsTriggered) return;
            IsTriggered = true;

            transform.DOKill();

            transform.DOScale(Vector3.one * 1.5f, delay / 2f).SetEase(Ease.InBounce).onComplete = () =>
            {
                transform.DOScale(Vector3.zero, delay / 2f).SetEase(Ease.OutQuad).onComplete = () =>
                {
                    IsTriggered = false; // Bad design but priority is over avoiding to modify the base class design.
                    base.Trigger(0f);
                    OnTriggerComplete?.Invoke();
                };
            };
        }

    }
}
