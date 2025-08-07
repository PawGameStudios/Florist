using System;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

public class Popup : MonoBehaviour
{
    public Transform ButtonTransform => _buttonPositive.transform;
    [SerializeField] private bool _hasPositiveButton;
    [SerializeField] private bool _hasNegativeButton;
    [SerializeField] private bool _hasExplanation;
    [SerializeField] private bool _hasTitle;
    [SerializeField] private Animator _animator;
    [ShowIf(nameof(_hasTitle))][SerializeField] private TextMeshProUGUI _title;
    [ShowIf(nameof(_hasExplanation))][SerializeField] private TextMeshProUGUI _explanation;
    [ShowIf(nameof(_hasPositiveButton))][SerializeField] private TextMeshProUGUI _buttonTextPositive;
    [ShowIf(nameof(_hasNegativeButton))][SerializeField] private TextMeshProUGUI _buttonTextNegative;
    [ShowIf(nameof(_hasPositiveButton))][SerializeField] private Button _buttonPositive;
    [ShowIf(nameof(_hasNegativeButton))][SerializeField] private Button _buttonNegative;

    public Popup SetTitle(string title = "Info")
    {
        _title.text = title;
        return this;
    }

    public Popup SetExplanation(string explanation = "")
    {
        _explanation.text = explanation;
        return this;
    }

    public Popup SetNegativeButtonTexts(string text)
    {
        _buttonTextNegative.text = text;
        return this;
    }

    public Popup SetPositiveButtonTexts(string text)
    {
        _buttonTextPositive.text = text;
        return this;
    }

    public Popup SetPositiveButtonListener(Action callback)
    {
        _buttonPositive.onClick.RemoveAllListeners();
        _buttonPositive.onClick.AddListener(() => { callback?.Invoke(); });
        return this;
    }

    public Popup SetNegaiveButtonListener(Action callback)
    {
        _buttonNegative.onClick.RemoveAllListeners();
        _buttonNegative.onClick.AddListener(() => { callback?.Invoke(); });
        return this;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        _animator.Play("Enter");
    }

    public void Close()
    {
        _animator.Play("Exit");
    }

    public void DisableImmediately()
    {
        OnExitAnimFinished();
    }

    public void OnExitAnimFinished()
    {
        gameObject.SetActive(false);
    }

    public void OnCloseButtonPressed()
    {
        Close();
    }
}
