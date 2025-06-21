using System;
using TMPro;
using UnityEngine;

public class TopCanvas : Page
{
    [SerializeField] private TextMeshProUGUI _moneyAmount, _diamondAmount;


    private void OnEnable()
    {
        GeneralData.MoneyAmountChanged += OnMoneyAmountChanged;
        GeneralData.DiamondAmountChanged += OnDiamondAmountChanged;
        OnMoneyAmountChanged();
        OnDiamondAmountChanged();
    }

    private void OnDisable()
    {
        GeneralData.MoneyAmountChanged -= OnMoneyAmountChanged;
        GeneralData.DiamondAmountChanged -= OnDiamondAmountChanged;
        CancelInvoke();
    }

    public override void Close(PageParams pageData = null, Action onCompleted = null)
    {
        gameObject.SetActive(false);
    }

    public override void Open(PageParams pageData = null, Action onCompleted = null)
    {
        gameObject.SetActive(true);
    }

    private void OnMoneyAmountChanged()
    {
        _moneyAmount.text = $"{SaveSystem.Inst.GeneralData.Money:0.##}";
    }

    private void OnDiamondAmountChanged()
    {
        _diamondAmount.text = $"{SaveSystem.Inst.GeneralData.Diamonds:0.##}";
    }
}
