using UnityEngine;
using DG.Tweening;

public class RotateSelf : MonoBehaviour
{
    [SerializeField] private Vector3 _rotationAmount = new(0, 0, 360);
    [SerializeField] private float _duration = 2f;
    [SerializeField] private bool _loop = true;
    private Tweener _rotationTween;

    private void OnEnable()
    {
        RotateObject();
    }

    private void OnDisable()
    {
        _rotationTween?.Kill();
    }

    private void RotateObject()
    {
        _rotationTween = transform.DORotate(_rotationAmount, _duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(_loop ? -1 : 0, LoopType.Restart);
    }
}