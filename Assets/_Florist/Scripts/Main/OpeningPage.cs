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
    [SerializeField] private float _balloonOscillationVariation = 0.3f; // 30% variation in oscillation
    [SerializeField] private float _balloonDurationVariation = 0.2f; // 20% variation in duration

    [SerializeField] private Tutorial _tutorial;
    [SerializeField] private Transform _playButtonTransform;
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

    public override void Open(PageParams pageData = null, Action onCompleted = null)
    {
        base.Open(pageData, onCompleted);

        if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
        {
            _tutorial.Init()
                    .PointTo(_playButtonTransform.position, Tutorial.PointDirection.Right)
                    .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand)
                    .SetClickableState(Tutorial.ClickableState.None)
                    .SetActivationDelay(2f)
                    .StartTutorial();
        }

        InvokeRepeating(nameof(PlayLightAnimation), 0f, _lightAnimPause);
    }

    public void OnPlayButtonClicked()
    {
        _playButton.SetActive(false);
        OpenDukkan();

        if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
        {
            _tutorial.FinishTutorial();
        }
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
        _openAnimSequence.Append(_parent.DOScale(_zoomAmount, _balloonMoveDuration));
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

        // Generate random oscillation amounts for this balloon
        float oscillation1 = _balloonMoveX * (1f + UnityEngine.Random.Range(-_balloonOscillationVariation, _balloonOscillationVariation));
        float oscillation2 = _balloonMoveX * (1f + UnityEngine.Random.Range(-_balloonOscillationVariation, _balloonOscillationVariation));

        float duration = _balloonMoveDuration * (1f + UnityEngine.Random.Range(-_balloonDurationVariation, _balloonDurationVariation)) / 4f;

        // Build the sequence with custom durations and oscillations
        ballonSequence.Append(balloonTransform.DOLocalMove(originalPosition, _zoomDuration + _candleDuration + _ribbonDuration));
        ballonSequence.Append(balloonTransform.DOLocalMove(new Vector3(originalPosition.x + oscillation1 * randDirection, originalPosition.y + _balloonMoveY, 0), duration).SetEase(_balloonMoveEase));
        ballonSequence.Append(balloonTransform.DOLocalMove(new Vector3(originalPosition.x, originalPosition.y + _balloonMoveY * 2, 0), duration).SetEase(_balloonMoveEase));
        ballonSequence.Append(balloonTransform.DOLocalMove(new Vector3(originalPosition.x + oscillation2 * randDirection, originalPosition.y + _balloonMoveY * 3, 0), duration).SetEase(_balloonMoveEase));
        ballonSequence.Append(balloonTransform.DOLocalMove(new Vector3(originalPosition.x, originalPosition.y + _balloonMoveY * 4, 0), duration).SetEase(_balloonMoveEase));
    }

    private void OnAnimFinished()
    {
        References.DukkanPage.Open(onCompleted: () =>
        {
            Close();
        });
    }
}
