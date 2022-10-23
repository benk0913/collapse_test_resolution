using System;
using DG.Tweening;
using UnityEngine;

namespace Collapse.Blocks
{
    /**
     * Bomb specific behavior
     */
    public class Bomb : NormalBlock
    {

        public const float DELAY_MODIFIER = 0.2f;

        [SerializeField]
        private Transform Sprite;

        [SerializeField]
        private Vector3 ShakeStrength;

        [SerializeField]
        private int ShakeVibrato;

        [SerializeField]
        private float ShakeDuration;

        private Vector3 origin;

        public bool Ignited = false;

        private void Awake()
        {
            origin = Sprite.localPosition;
        }

        protected override void OnMouseUp()
        {
            if (Ignited) return;
            IgniteBomb();
        }

        /**
         * Convenience for shake animation with callback in the end
         */
        public void Shake(Action onComplete = null)
        {
            Sprite.DOKill();
            Sprite.localPosition = origin;
            Sprite.DOShakePosition(ShakeDuration, ShakeStrength, ShakeVibrato, fadeOut: false).onComplete += () =>
            {
                onComplete?.Invoke();
            };
        }

        public void IgniteBomb()
        {
            Ignited = true;
            Shake(() => BoardManager.Instance.TriggerBomb(this));
        }
    }
}