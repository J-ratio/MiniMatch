using UnityEngine;
using DG.Tweening;
using System;

public class CardFlipAnimation : MonoBehaviour
{
    public GameObject front;
    public GameObject back;
    public float flipDuration = 0.3f;

    public void FlipToFront(Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2))
            .AppendCallback(() => { front.SetActive(true); back.SetActive(false); })
            .Append(transform.DORotate(new Vector3(0, 0, 0), flipDuration / 2))
            .OnComplete(() => { if (onComplete != null) onComplete(); });
    }

    public void FlipToBack(Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2))
            .AppendCallback(() => { front.SetActive(false); back.SetActive(true); })
            .Append(transform.DORotate(new Vector3(0, 0, 0), flipDuration / 2))
            .OnComplete(() => { if (onComplete != null) onComplete(); });
    }
}

