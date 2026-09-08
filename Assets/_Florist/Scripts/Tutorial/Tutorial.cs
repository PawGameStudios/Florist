using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public enum PopupPos
    {
        Top, Bottom
    }

    public enum PointDirection
    {
        Right, Left
    }

    public enum ClickableState
    {
        None, PopupBg, Background, HighlightArea, All
    }

    public enum ObjectActivationOptions
    {
        PopUp, Hand, Hand2, Bg
    }

    [Header("General")]
    [SerializeField] private GameObject _tutorialParent;
    [SerializeField] private GameObject _bg;
    [SerializeField] private Button _bgButton;
    [SerializeField] private Button _skipButton;
    [SerializeField] private Image _bgImage;

    [Header("Popup")]
    [SerializeField] private GameObject _popup;
    [SerializeField] private CanvasGroup _popupCanvasGroup;
    [SerializeField] private Transform _popupTopRef;
    [SerializeField] private Transform _popupBottomRef;
    [SerializeField] private RectTransform _popupRect;
    [SerializeField] private Button _popupBgButton;
    [SerializeField] private TextMeshProUGUI _popupExplanation;

    [Header("Highlight")]
    [SerializeField] private Image _highlightImage;
    [SerializeField] private Image _highlightImage2;
    [SerializeField] private Image _highlightBg;
    [SerializeField] private Button _highlightButton;
    [SerializeField] private Button _highlightButton2;

    [Header("Hand")]
    [SerializeField] private Transform _hand;
    [SerializeField] private Transform _hand2;
    [SerializeField] private Animator _handAnimator;
    [SerializeField] private Animator _handAnimator2;

    private Action _clickCallback;
    private Action _timeoutCallback, _delayedCallback;
    private Action _handAnimCallback;
    private string _handAnimToPlay = "Idle";
    private string _hand2AnimToPlay = "Idle";
    private float _delay = 0, _activationDelay = 0;
    private ObjectActivationOptions[] _objectActivationOptionsArray;
    private Sequence _handTweenSequence = null;
    private Tween _handTween = null;
    private Tween _bgTween = null;
    private Sequence _popupSequence = null;
    private Transform _handCoordinateSpace;
    private Vector3 _handLocalPosition;

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        CancelInvoke();
        _handTween?.Kill();
        _handTweenSequence?.Kill();
        _bgTween?.Kill();
        _popupSequence?.Kill();
    }

    public Tutorial Init()
    {
        _tutorialParent.SetActive(true);
        SetDefaultValues();
        return this;
    }

    public Tutorial SetSkipTutorial(bool enable, Action onButtonCallback = null)
    {
        _skipButton.gameObject.SetActive(enable);
        _skipButton.onClick.RemoveAllListeners();
        _skipButton.onClick.AddListener(() =>
        {
            onButtonCallback?.Invoke();
        });
        return this;
    }

    public Tutorial SetExplanation(string explanation)
    {
        _popupExplanation.text = explanation;
        return this;
    }

    public Tutorial PointTo(Vector3 targetPos, PointDirection direction = PointDirection.Left)
    {
        if (direction == PointDirection.Left)
        {
            _hand.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            _hand.localScale = new Vector3(-1, 1, 1);
        }

        _handTween?.Kill();
        _handAnimToPlay = "Click";
        _hand.transform.position = targetPos;
        if (_handCoordinateSpace != null)
            _handLocalPosition = _handCoordinateSpace.InverseTransformPoint(targetPos);
        return this;
    }

    public Tutorial SetHandCoordinateSpace(Transform coordinateSpace)
    {
        _handCoordinateSpace = coordinateSpace;
        return this;
    }

    public void RefreshHandPosition()
    {
        if (_handCoordinateSpace != null)
            _hand.position = _handCoordinateSpace.TransformPoint(_handLocalPosition);
    }

    public Tutorial SetHandCallback(Action callback)
    {
        _handAnimCallback = callback;
        return this;
    }

    public Tutorial SwipeBetween(Vector3 swipeStartPos, Vector3 swipeTargetPos, float duration = 2, bool loop = true, int loopCount = -1)
    {
        _handAnimToPlay = "Idle";
        _handTween?.Kill();
        _hand.transform.position = swipeStartPos;

        if (_handCoordinateSpace != null)
        {
            _handLocalPosition = _handCoordinateSpace.InverseTransformPoint(swipeStartPos);
            Vector3 localTarget = _handCoordinateSpace.InverseTransformPoint(swipeTargetPos);
            _handTween = DOTween.To(() => _handLocalPosition, position =>
            {
                _handLocalPosition = position;
                RefreshHandPosition();
            }, localTarget, duration);
        }
        else
        {
            _handTween = _hand.DOMove(swipeTargetPos, duration);
        }
        if (loop)
            _handTween.SetLoops(loopCount, LoopType.Restart);
        return this;
    }

    public Tutorial SetPopupPosition(PopupPos pos)
    {
        switch (pos)
        {
            case PopupPos.Top:
                _popupRect.position = _popupTopRef.position;
                break;
            case PopupPos.Bottom:
                _popupRect.position = _popupBottomRef.position;
                break;
        }
        return this;
    }

    public Tutorial SetObjectActivation(params ObjectActivationOptions[] options)
    {
        Debug.Log($"#tutorial# SetObjectActivation, options: {string.Join(", ", options)}");
        _objectActivationOptionsArray = options;
        return this;
    }

    public Tutorial SetClickableState(params ClickableState[] states)
    {
        _bgButton.interactable = false;
        _popupBgButton.interactable = false;
        _highlightButton.interactable = false;
        for (int i = 0; i < states.Length; i++)
        {
            switch (states[i])
            {
                case ClickableState.All:
                    _bgButton.interactable = true;
                    _popupBgButton.interactable = true;
                    _highlightButton.interactable = true;
                    return this;
                case ClickableState.PopupBg:
                    _popupBgButton.interactable = true;
                    break;
                case ClickableState.Background:
                    _bgButton.interactable = true;
                    break;
                case ClickableState.HighlightArea:
                    _highlightButton.interactable = true;
                    break;
            }
        }
        return this;
    }

    public Tutorial SetClickCallback(Action clickCallback)
    {
        _clickCallback = clickCallback;
        return this;
    }

    public Tutorial SetTimeout(float timeout, Action callback = null)
    {
        Invoke(nameof(FinishTutorialStep), timeout);
        _timeoutCallback = callback;
        return this;
    }

    public Tutorial SetDelayedCallback(float delay, Action callback)
    {
        _delayedCallback = callback;
        Invoke(nameof(CallDelayedAction), delay);
        return this;
    }

    public Tutorial SetDelay(float delay)
    {
        _delay = delay;
        return this;
    }

    public Tutorial SetActivationDelay(float delay)
    {
        _activationDelay = delay;
        return this;
    }

    public Tutorial CloseHand()
    {
        _hand.gameObject.SetActive(false);
        return this;
    }

    public Tutorial Highlight(Sprite sprite, Transform transform)
    {
        _highlightImage.sprite = sprite;
        var source = (RectTransform)transform;
        MatchHighlightRect(_highlightImage.rectTransform, source);
        if (_highlightButton.targetGraphic is Image buttonImage)
            buttonImage.sprite = sprite;
        MatchHighlightRect((RectTransform)_highlightButton.transform, source);

        _highlightButton.gameObject.SetActive(true);
        _highlightImage.gameObject.SetActive(true);
        // _highlightBg.gameObject.SetActive(true);
        return this;
    }

    private static void MatchHighlightRect(RectTransform target, RectTransform source)
    {
        // The card and tutorial can have different parent scales, anchors and pivots.
        var corners = new Vector3[4];
        source.GetWorldCorners(corners);
        var bounds = new Bounds(target.parent.InverseTransformPoint(corners[0]), Vector3.zero);
        for (int i = 1; i < corners.Length; i++)
            bounds.Encapsulate(target.parent.InverseTransformPoint(corners[i]));

        target.anchorMin = target.anchorMax = new Vector2(.5f, .5f);
        target.pivot = new Vector2(.5f, .5f);
        target.localScale = Vector3.one;
        target.localRotation = Quaternion.identity;
        target.sizeDelta = new Vector2(bounds.size.x, bounds.size.y);
        target.localPosition = bounds.center;
    }

    public Tutorial Highlight2(Sprite sprite, Transform transform)
    {
        _highlightImage2.sprite = sprite;
        _highlightImage2.transform.position = transform.position;
        _highlightImage2.transform.localScale = transform.localScale;
        _highlightImage2.GetComponent<RectTransform>().sizeDelta = transform.GetComponent<RectTransform>().sizeDelta;

        _highlightButton2.GetComponent<Image>().sprite = sprite;
        _highlightButton2.transform.position = transform.position;
        _highlightButton2.transform.localScale = transform.localScale;
        _highlightButton2.GetComponent<RectTransform>().sizeDelta = transform.GetComponent<RectTransform>().sizeDelta;

        _highlightButton2.gameObject.SetActive(true);
        _highlightImage2.gameObject.SetActive(true);
        return this;
    }

    public void StartTutorial()
    {
        Invoke(nameof(SetObjectActivation2), _activationDelay);
    }

    public void FinishTutorial()
    {
        SetDefaultValues(() =>
        {
            _tutorialParent.SetActive(false);
        });
    }

    public void CloseTutorial()
    {
        _tutorialParent.SetActive(false);
    }

    public void ButtonClick()
    {
        Invoke(nameof(FireCallback), _delay);
    }

    public void OnHandAnimClicked()
    {
        _handAnimCallback?.Invoke();
    }

    public void StopHandAnim()
    {
        _handAnimator.Play("Idle");
        _handTween?.Kill();
        _handTweenSequence?.Kill();
        _hand.gameObject.SetActive(false);
    }

    public Tutorial ResumeHandAnim()
    {
        _hand.gameObject.SetActive(true);
        _handAnimator.Play(_handAnimToPlay);
        return this;
    }

    public void FinishTutorialStep()
    {
        CancelInvoke(nameof(FinishTutorialStep));

        _timeoutCallback?.Invoke();
        SetDefaultValues(() =>
        {
            _tutorialParent.SetActive(false);
        });
    }

    public void SetDefaultValues(Action onComplete = null)
    {
        CancelInvoke();
        _handCoordinateSpace = null;
        Debug.Log($"#tutorial# SetDefaultValues");
        _activationDelay = 0;
        _delay = 0;
        _bgTween?.Kill();
        _bgTween = _bgImage.DOFade(0f, .1f).OnComplete(() =>
        {
            _bg.SetActive(false);
        });
        _handTween?.Kill();
        _popupSequence?.Kill();
        _popupSequence = DOTween.Sequence();
        _popupSequence.Append(_popup.transform.DOScale(0, .1f))
                        .Join(_popupCanvasGroup.DOFade(0, .1f))
                        .AppendCallback(() =>
                        {
                            _popup.SetActive(false);
                            onComplete?.Invoke();
                        });
        _hand.gameObject.SetActive(false);
        _hand2.gameObject.SetActive(false);
        _highlightBg.gameObject.SetActive(false);
        _highlightImage.gameObject.SetActive(false);
        _highlightImage2.gameObject.SetActive(false);
        _highlightButton.gameObject.SetActive(false);
        _highlightButton2.gameObject.SetActive(false);
        _clickCallback = null;
        _timeoutCallback = null;
        _handAnimCallback = null;
        _objectActivationOptionsArray = new ObjectActivationOptions[] { };
        _timeoutCallback = null;
        _popupRect.position = _popupBottomRef.position;
        _hand.localScale = new Vector3(1, 1, 1);
    }

    private void SetObjectActivation2()
    {
        Debug.Log($"#tutorial# SetObjectActivation2");
        _tutorialParent.SetActive(true);
        for (int i = 0; i < _objectActivationOptionsArray.Length; i++)
        {
            switch (_objectActivationOptionsArray[i])
            {
                case ObjectActivationOptions.PopUp:
                    _popup.SetActive(true);
                    _popupSequence?.Kill();
                    _popupSequence = DOTween.Sequence();
                    _popupSequence.Append(_popup.transform.DOScale(0, 0f))
                                    .Append(_popupCanvasGroup.DOFade(0, 0))
                                    .Append(_popup.transform.DOScale(1, .3f))
                                    .Join(_popupCanvasGroup.DOFade(1, .3f));
                    break;
                case ObjectActivationOptions.Bg:
                    _bg.SetActive(true);
                    _bgTween?.Kill();
                    _bgImage.color = new Color(0, 0, 0, 0);
                    _bgTween = _bgImage.DOFade(.58f, .1f);
                    break;
                case ObjectActivationOptions.Hand:
                    _hand.gameObject.SetActive(true);
                    CancelInvoke(nameof(PlayHandAnim));
                    Invoke(nameof(PlayHandAnim), .1f);
                    break;
                case ObjectActivationOptions.Hand2:
                    _hand.gameObject.SetActive(true);
                    _hand2.gameObject.SetActive(true);
                    CancelInvoke(nameof(PlayHandAnim));
                    Invoke(nameof(PlayHandAnim), .1f);
                    break;
            }
        }
    }

    private void PlayHandAnim()
    {
        if (_handAnimator.gameObject.activeInHierarchy)
            _handAnimator.Play(_handAnimToPlay);
        if (_handAnimator2.gameObject.activeInHierarchy)
            _handAnimator2.Play(_hand2AnimToPlay);
    }

    private void FireCallback()
    {
        _clickCallback?.Invoke();
    }

    private void CallDelayedAction()
    {
        _delayedCallback?.Invoke();
    }
}
