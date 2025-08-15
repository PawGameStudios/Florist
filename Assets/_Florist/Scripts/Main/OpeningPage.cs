using System;
using UnityEngine;
using DG.Tweening;

public class OpeningPage : Page
{
    [SerializeField] private float _lightAnimPause = 1f;
    [SerializeField] private Vector3 _zoomAmount = new(1.2f, 1.2f, 1.2f);
    [SerializeField] private float _zoomDuration = .5f;
    [SerializeField] private Ease _zoomEase = Ease.OutBack;
    [SerializeField] private float _candleDuration = .5f;
    [SerializeField] private float _ribbonDuration = .5f;
    [SerializeField] private float _balloonMoveX = 50f;
    [SerializeField] private float _balloonMoveY = 250f;
    [SerializeField] private float _balloonMoveDuration = 2.5f;
    [SerializeField] private Ease _balloonMoveEase = Ease.Linear;

    [SerializeField] private Transform _parent;
    [SerializeField] private GameObject _lightParent;
    [SerializeField] private GameObject _playButton;
    [SerializeField] private Animator _ribbonAnimator;
    [SerializeField] private Animator[] _candleAnimators;
    [SerializeField] private Transform[] _balloonTransforms;

    private Sequence _openAnimSequence;

    void OnDisable()
    {
        _openAnimSequence?.Kill();
        CancelInvoke();
    }

    public override void Close(PageParams pageData = null, Action onCompleted = null)
    {
        gameObject.SetActive(false);
    }

    public override void Open(PageParams pageData = null, Action onCompleted = null)
    {
        base.Open(pageData, onCompleted);
        gameObject.SetActive(true);

        InvokeRepeating(nameof(PlayLightAnimation), 0f, _lightAnimPause);
    }

    public void OnPlayButtonClicked()
    {
        _playButton.SetActive(false);
        OpenDukkan();
    }

    private void OpenDukkan()
    {
        _openAnimSequence?.Kill();
        _openAnimSequence = DOTween.Sequence();
        _openAnimSequence.Append(_parent.DOScale(_zoomAmount, _zoomDuration).SetEase(_zoomEase)
                    .OnComplete(() =>
                    {
                        foreach (var candleAnimator in _candleAnimators)
                        {
                            candleAnimator.Play("Out");
                        }
                    }));
        _openAnimSequence.Append(_parent.DOScale(_zoomAmount, _candleDuration).OnComplete(() =>
                    {
                        _ribbonAnimator.Play("Cut");
                    }));
        _openAnimSequence.Append(_parent.DOScale(_zoomAmount, _ribbonDuration));
        foreach (var balloonTransform in _balloonTransforms)
        {
            PlayBalloonAnimation(balloonTransform);
        }
        _openAnimSequence.Append(_parent.DOScale(_zoomAmount, _balloonMoveDuration * 4));
        _openAnimSequence.AppendCallback(OnAnimFinished);
    }

    private void PlayLightAnimation()
    {
        _lightParent.SetActive(!_lightParent.activeSelf);
    }

    private void PlayBalloonAnimation(Transform balloonTransform)
    {
        Sequence ballonSequence = DOTween.Sequence();
        Vector3 originalPosition = balloonTransform.localPosition;
        int randDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        ballonSequence.Append(balloonTransform.DOLocalMove(originalPosition, _zoomDuration + _candleDuration + _ribbonDuration));
        ballonSequence.Append(balloonTransform.DOLocalMove(new Vector3(originalPosition.x + _balloonMoveX * randDirection, originalPosition.y + _balloonMoveY, 0), _balloonMoveDuration).SetEase(_balloonMoveEase));
        ballonSequence.Append(balloonTransform.DOLocalMove(new Vector3(originalPosition.x, originalPosition.y + _balloonMoveY * 2, 0), _balloonMoveDuration).SetEase(_balloonMoveEase));
        ballonSequence.Append(balloonTransform.DOLocalMove(new Vector3(originalPosition.x + _balloonMoveX * randDirection - 1, originalPosition.y + _balloonMoveY * 3, 0), _balloonMoveDuration).SetEase(_balloonMoveEase));
        ballonSequence.Append(balloonTransform.DOLocalMove(new Vector3(originalPosition.x, originalPosition.y + _balloonMoveY * 4, 0), _balloonMoveDuration).SetEase(_balloonMoveEase));
    }

    private void OnAnimFinished()
    {
        References.DukkanPage.Open(onCompleted: () =>
        {
            Close();
        });
    }
}
