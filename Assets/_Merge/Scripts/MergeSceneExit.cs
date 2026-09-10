using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Florist.Merge
{
    public sealed class MergeSceneExit : MonoBehaviour
    {
        public static event Action ReturnRequested;
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private Button _returnButton;
        [SerializeField] private GameObject _mergeInput;
        [SerializeField] private TextMeshProUGUI _moneyText;
        [SerializeField] private string _standaloneFloristScenePath = "Assets/_Florist/Scenes/1_GameScene.unity";
        private bool _returning;

        private void OnEnable()
        {
            _returnButton.onClick.AddListener(ReturnToFlorist);
            _returning = false;
            _returnButton.interactable = ReturnRequested != null || Application.CanStreamedLevelBeLoaded(_standaloneFloristScenePath);
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
                    ? $"<sprite=0> {SaveSystem.Inst.GeneralData.Money:0.##}"
                    : "";
        }

        private void ReturnToFlorist()
        {
            if (_returning) return;
            if (ReturnRequested == null && !Application.CanStreamedLevelBeLoaded(_standaloneFloristScenePath)) return;
            _returning = true;
            _returnButton.interactable = false;
            if (ReturnRequested != null)
            {
                _gameManager.PrepareForExit();
                _mergeInput.SetActive(false);
                ReturnRequested.Invoke();
                return;
            }
            // Direct Merge entry has no suspended Florist session to restore.
            try
            {
                var load = SceneManager.LoadSceneAsync(_standaloneFloristScenePath, LoadSceneMode.Single);
                if (load != null)
                {
                    _gameManager.PrepareForExit();
                    _mergeInput.SetActive(false);
                    return;
                }
            }
            catch (Exception exception) { Debug.LogException(exception, this); }
            _returning = false;
            _returnButton.interactable = true;
        }
    }
}
