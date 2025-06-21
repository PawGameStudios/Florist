using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;

public class EndDayPage : Page
{
    [SerializeField] private GameObject _endDayPanel;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private Image _nightImage;
    [SerializeField] private Image _nightLightImage;
    [SerializeField] private Image _dayImage;
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private TextMeshProUGUI _revenueText;
    [SerializeField] private TextMeshProUGUI _tipText;
    [SerializeField] private TextMeshProUGUI _rentText;
    [SerializeField] private TextMeshProUGUI _refundText;
    [SerializeField] private TextMeshProUGUI _flowerCostText;
    [SerializeField] private TextMeshProUGUI _profitText;
    private Sequence _sequence;

    private void OnDisable()
    {
        _sequence?.Kill();
        CancelInvoke();
    }

    public override void Open(PageParams pageData = null, Action onCompleted = null)
    {
        base.Open(pageData, onCompleted);

        gameObject.SetActive(true);
        _endDayPanel.SetActive(true);

        _fadeImage.gameObject.SetActive(true);
        _fadeImage.color = new Color(0, 0, 0, .95f);
        _nightLightImage.color = new Color(1, 1, 1, 1);
        _nightImage.color = new Color(1, 1, 1, 1);
        _dayImage.color = new Color(1, 1, 1, 1);

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(_fadeImage.DOFade(endValue: 0, duration: .3f).SetEase(Ease.InSine).OnComplete(() =>
        {
            _fadeImage.gameObject.SetActive(false);
            onCompleted?.Invoke();
        }));
    }

    public override void Close(PageParams pageData = null, Action onCompleted = null)
    {
        SaveSystem.Inst.GeneralData.IncreaseDayIndex();
        _endDayPanel.SetActive(false);

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(_nightLightImage.DOFade(endValue: 0, duration: .4f).SetEase(Ease.Linear));
        _sequence.Append(_nightImage.DOFade(endValue: 0, duration: .4f).SetEase(Ease.Linear));
        _sequence.Append(_dayImage.DOFade(endValue: 0, duration: .4f).SetEase(Ease.Linear));
        _sequence.AppendCallback(() =>
        {
            gameObject.SetActive(false);
            onCompleted?.Invoke();
        });
    }

    public void SetData(EarningsInfo earningsInfo)
    {
        _dayText.text = $"{LocalizationManager.GetLocalizedText("day")}: {SaveSystem.Inst.GeneralData.CurrentDayIndex + 1}";
        _revenueText.text = earningsInfo.Earnings.ToString("0.00");
        _tipText.text = earningsInfo.Tip.ToString("0.00");
        _rentText.text = $"-{earningsInfo.Rent:0.00}";
        _refundText.text = $"-{earningsInfo.Refund:0.00}";
        _flowerCostText.text = $"-{earningsInfo.Cost:0.00}";
        _profitText.text = earningsInfo.Profit.ToString("0.00");
    }

    public void OnNextDayButtonClicked()
    {
        HapticsController.PlayButtonHaptic();
        References.MainPage.Open();
        Close(onCompleted: () =>
        {
        });
    }
}
