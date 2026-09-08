using System;
using UnityEngine;
using UnityEngine.UI;

public class DecorationOverlay : MonoBehaviour
{
    [SerializeField] private Button _openButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private GameObject _modeRoot;
    [SerializeField] private GameObject _bottomPanel;
    [SerializeField] private GameObject _decorationButtonsParent;

    public void Initialize(Action onOpen, Action onClose, Action onCategoryClose)
    {
        _openButton.onClick.AddListener(() => onOpen.Invoke());
        _closeButton.onClick.AddListener(() => onClose.Invoke());
        _backButton.onClick.AddListener(() => onCategoryClose.Invoke());
        HideMode();
    }

    public void ShowOverview()
    {
        _openButton.gameObject.SetActive(false);
        _modeRoot.SetActive(true);
        _decorationButtonsParent.SetActive(true);
        _bottomPanel.SetActive(false);
        _backButton.gameObject.SetActive(false);
    }

    public void ShowCategory()
    {
        _modeRoot.SetActive(true);
        _decorationButtonsParent.SetActive(false);
        _bottomPanel.SetActive(true);
        _backButton.gameObject.SetActive(true);
    }

    public void HideMode()
    {
        _modeRoot.SetActive(false);
        _openButton.gameObject.SetActive(true);
    }
}
