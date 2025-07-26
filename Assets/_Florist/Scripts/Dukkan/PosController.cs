using System;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PosController : MonoBehaviour
{
    [SerializeField] private Transform _moneyPanel;
    [SerializeField] private Transform _moneyParent;
    [SerializeField] private MoneyObject _moneyPrefab;
    [SerializeField] private TextMeshProUGUI _givenMoneyText;
    [SerializeField] private TextMeshProUGUI _remainingChangeText;
    [SerializeField] private SerializedDictionary<int, Sprite> _billSprites;
    private Tween _tween;
    private Action<int> _onCompleted;
    private int _change = 0;

    private void OnDisable()
    {
        _tween?.Kill();
    }

    public void ResetPos()
    {
        _change = 0;
    }

    public void ReceivePayment(int payment, int price, Action<int> onCompleted)
    {
        onCompleted?.Invoke(500); // Example payment, replace with actual logic
        // SaveSystem.Inst.GeneralData.ChangeMoney(payment);

        // _change = payment - price;
        // _onCompleted = onCompleted;
        // _givenMoneyText.text = $"{payment}";
        // _remainingChangeText.text = $"{_change}";

        // // TODO: set position
        // _tween?.Kill();
        // _tween = _moneyPanel.DOLocalMoveY(-10, .3f);
        // // _tween = transform.DOPunchScale(Vector3.one * 0.15f, .7f, vibrato: 1, elasticity: 1).OnComplete(() => onCompleted?.Invoke());
    }

    public void OnMoneyButtonClicked(int amount)
    {
        _change -= amount;
        _remainingChangeText.text = $"{_change}";

        Instantiate(_moneyPrefab, _moneyParent)
                .SetSprite(_billSprites[amount])
                .SetRandomRotation()
                .SetValue(amount);
    }

    public void OnMoneyObjectClicked(int amount)
    {
        _change += amount;
        _remainingChangeText.text = $"{_change}";
    }

    public void OnConfirmButtonClicked()
    {
        _onCompleted?.Invoke(_change);
    }
}
