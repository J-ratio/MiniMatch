using UnityEngine;
using DG.Tweening;
using System;

namespace MiniMatch.Presentation.Animation
{
    public class CardFlipAnimation : MonoBehaviour
    {
        public GameObject front;
        public GameObject back;
        public float flipDuration = 0.3f;

        public void FlipToFront(Action onComplete = null)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2))
                .AppendCallback(() => { 
                    if (front != null) front.SetActive(true); 
                    if (back != null) back.SetActive(false); 
                })
                .Append(transform.DORotate(new Vector3(0, 0, 0), flipDuration / 2))
                .OnComplete(() => onComplete?.Invoke());
        }

        public void FlipToBack(Action onComplete = null)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2))
                .AppendCallback(() => { 
                    if (front != null) front.SetActive(false); 
                    if (back != null) back.SetActive(true); 
                })
                .Append(transform.DORotate(new Vector3(0, 0, 0), flipDuration / 2))
                .OnComplete(() => onComplete?.Invoke());
        }
        
        private void OnDestroy()
        {
            // Clean up any running tweens
            transform.DOKill();
        }
    }
}

