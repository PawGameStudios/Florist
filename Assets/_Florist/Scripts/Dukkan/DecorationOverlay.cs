using System;
using UnityEngine;
using UnityEngine.UI;

public class DecorationOverlay : MonoBehaviour
{
    [SerializeField] private Button _openButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private GameObject _modeRoot;
    [SerializeField] private GameObject _bottomPanel;

    public void Initialize(Action onOpen, Action onClose, Action onCategoryClose)
    {
        _openButton.onClick.AddListener(() => onOpen.Invoke());
        _closeButton.onClick.AddListener(() => onClose.Invoke());
        _backButton.onClick.AddListener(() => onCategoryClose.Invoke());
        _confirmButton.onClick.AddListener(() => onCategoryClose.Invoke());
        HideMode();
    }

    public void ShowOverview()
    {
        _openButton.gameObject.SetActive(false);
        _modeRoot.SetActive(true);
        _bottomPanel.SetActive(false);
    }

    public void ShowCategory(DecorationManager.DecorationType decorationType)
    {
        _modeRoot.SetActive(true);
        _bottomPanel.SetActive(true);
    }

    public void HideMode()
    {
        _modeRoot.SetActive(false);
        _openButton.gameObject.SetActive(true);
    }
}
