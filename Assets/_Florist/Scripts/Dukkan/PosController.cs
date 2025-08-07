using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PosController : MonoBehaviour
{
    [SerializeField] private Animator _posAnimator;
    [SerializeField] private Transform _moneyPanel;
    [SerializeField] private Transform _moneyParent;
    [SerializeField] private MoneyObject _moneyPrefab;
    [SerializeField] private TextMeshProUGUI _posText;
    [SerializeField] private TextMeshProUGUI _paymentText;
    [SerializeField] private TextMeshProUGUI _remainingChangeText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Image _posImage;
    [SerializeField] private Sprite _posOpenSprite;
    [SerializeField] private Sprite _posCloseSprite;
    [SerializeField] private SerializedDictionary<int, Sprite> _billSprites;
    private Action<int> _onCompleted;
    private int _targetChange = 0;
    private int _currentChange = 0;
    private List<GameObject> _moneyObjects = new();

    public void ResetPos()
    {
        _currentChange = 0;
        _targetChange = 0;
    }

    public void ReceivePayment(int payment, int price, Action<int> onCompleted)
    {
        _posAnimator.Play("Open");

        for (int i = 0; i < _moneyObjects.Count; i++)
        {
            Destroy(_moneyObjects[i]);
        }
        _moneyObjects.Clear();

        _currentChange = 0;
        _targetChange = payment - price;
        _onCompleted = onCompleted;
        _paymentText.text = $"{payment}";
        _remainingChangeText.text = $"{_currentChange}";
        _posText.text = $"{price}";
        _priceText.text = $"{price}";

        _confirmButton.interactable = false;

        _posImage.sprite = _posOpenSprite;

        if (!SaveSystem.Inst.SaveData.IsPosTutorialFinished)
        {

        }
    }

    public void OnMoneyButtonClicked(int amount)
    {
        _currentChange += amount;
        CheckChange();

        var obj = Instantiate(_moneyPrefab, _moneyParent)
                .SetSprite(_billSprites[amount])
                .SetRandomRotation()
                .SetValue(amount);

        _moneyObjects.Add(obj.gameObject);
    }

    public void OnMoneyObjectClicked(int amount)
    {
        _currentChange -= amount;
        CheckChange();
    }

    public void OnConfirmButtonClicked()
    {
        _onCompleted?.Invoke(_currentChange);
        _posImage.sprite = _posCloseSprite;
        _posAnimator.Play("Close");
    }

    private void CheckChange()
    {
        _confirmButton.interactable = _currentChange >= _targetChange;
        _remainingChangeText.text = $"{_currentChange}";
    }
}
