using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Florist.Merge
{
    public sealed class MergeSceneExit : MonoBehaviour
    {
        public static event Action ReturnRequested;
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private Button _returnButton;
        [SerializeField] private GameObject _mergeInput;
        [SerializeField] private TextMeshProUGUI _moneyText;

        private void OnEnable()
        {
            _returnButton.onClick.AddListener(ReturnToFlorist);
            _returnButton.interactable = ReturnRequested != null;
            GeneralData.MoneyAmountChanged += RefreshMoney;
            RefreshMoney();
        }

        private void OnDisable()
        {
            _returnButton.onClick.RemoveListener(ReturnToFlorist);
            GeneralData.MoneyAmountChanged -= RefreshMoney;
        }

        private void RefreshMoney()
        {
            if (_moneyText != null)
                _moneyText.text = SaveSystem.Inst != null
                    ? $"Altın: {SaveSystem.Inst.GeneralData.Money:0.##}"
                    : "";
        }

        private void ReturnToFlorist()
        {
            if (ReturnRequested == null) return;
            _returnButton.interactable = false;
            _mergeInput.SetActive(false);
            _gameManager.PrepareForExit();
            ReturnRequested.Invoke();
        }
    }
}
