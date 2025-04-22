using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HappinessMeter : MonoBehaviour
{
    public float HappinessValue => _happinessValue;
    [SerializeField] private Image _emojiImage;
    [SerializeField] private TextMeshProUGUI _happinessText;
    [SerializeField] private List<Sprite> _emojiSprites;
    private int _totalTicksForCustomer, _currentTick = 0;
    private float _happinessValue = 100f;

    private void OnEnable()
    {
        int currentday = SaveSystem.Inst.GeneralData.CurrentDayIndex;
        int customerCount = Configs.LevelConfig.Days[currentday].Events.Count;
        float totalTickCount = Configs.LevelConfig.DayTimeInfo.DayDuration * 60 * 10f;
        float ticksPerCustomer = totalTickCount / customerCount;
        _totalTicksForCustomer = (int)(ticksPerCustomer * 10);
        _happinessValue = 100;
    }

    private void OnDisable()
    {
        Timer.TimeTickMiliseconds -= TimeTickHandler;
    }

    public void ResetHappinessMeter()
    {
        _currentTick = 0;
        _happinessValue = 100;
        _happinessText.text = $"{_happinessValue}%";
        SetEmojiText();
    }

    public void StartNewHappinessCountdown()
    {
        _currentTick = 0;
        _happinessValue = 100;
        Timer.TimeTickMiliseconds -= TimeTickHandler;
        Timer.TimeTickMiliseconds += TimeTickHandler;
    }

    public void StopHappinessCountdown()
    {
        Timer.TimeTickMiliseconds -= TimeTickHandler;
    }

    public void ChangeHappinessAfterOrderReceived(float happinessValue)
    {
        _happinessValue += happinessValue;
        _happinessText.text = $"{_happinessValue}%";
        SetEmojiText();
    }

    private void TimeTickHandler()
    {
        _currentTick++;
        _happinessValue = 100 - (int)((float)_currentTick / _totalTicksForCustomer * 100);
        _happinessText.text = $"{_happinessValue}%";
        SetEmojiText();
    }

    private void SetEmojiText()
    {
        int emojiCount = _emojiSprites.Count;
        int divider = 100 / emojiCount;
        int emojiIndex = Mathf.Clamp((int)(_happinessValue / divider), 0, emojiCount - 1);
        _emojiImage.sprite = _emojiSprites[emojiIndex];
    }
}
