using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConvoHistory : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> _convoHistoryTexts;
    [SerializeField] private Image _bgImage;
    [SerializeField] private TextMeshProUGUI _speechBubbleTextForLineCount;
    private bool _isOpen = false;
    private const float LINE_HEIGHT = 60f;
    private const float LINE_SPACING = 20f;

    public void SetConvoHistory(List<string> convoHistory)
    {
        if (_isOpen)
        {
            OnCloseButtonClicked();
            return;
        }

        for (int i = 0; i < _convoHistoryTexts.Count; i++)
        {
            if (i < convoHistory.Count)
            {
                _convoHistoryTexts[i].text = convoHistory[i];
                _convoHistoryTexts[i].gameObject.SetActive(true);
            }
            else
            {
                _convoHistoryTexts[i].gameObject.SetActive(false);
            }
        }

        int totalLineCount = convoHistory.Count * 2;
        float height = totalLineCount * LINE_HEIGHT + LINE_SPACING * (convoHistory.Count - 1);
        _bgImage.rectTransform.sizeDelta = new Vector2(_bgImage.rectTransform.sizeDelta.x, height);

        _isOpen = true;
        gameObject.SetActive(true);
    }

    public void OnCloseButtonClicked()
    {
        _isOpen = false;
        gameObject.SetActive(false);
    }
}
