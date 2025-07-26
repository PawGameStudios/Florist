using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class DayTimeManager : MonoBehaviour
{
    public float TotalTimePassed => _totalTimePassed;
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private Image _timeIcon;
    [SerializeField] private List<Sprite> _timeSprites;
    private int _totalMinutesInDay;
    private float _totalTimePassed;
    private float _timeTickInGameTime = 60;

    public void StartDayTimeCountdown(float totalTimePassed)
    {
        _totalMinutesInDay = (Configs.LevelConfig.DayTimeInfo.DayEndTime - Configs.LevelConfig.DayTimeInfo.DayStartTime) * 60;

        _timeTickInGameTime = _totalMinutesInDay / (Configs.LevelConfig.DayTimeInfo.DayDuration * 60 * 10f);
        _totalTimePassed = totalTimePassed;

        int hour = Configs.LevelConfig.DayTimeInfo.DayStartTime + (int)(_totalTimePassed / 60);
        int minutes = (int)(_totalTimePassed % 60);

        SetDay(SaveSystem.Inst.GeneralData.CurrentDayIndex);
        SetTime(hour, minutes);

        Timer.TimeTickMiliseconds -= TimeTickHandler;
        Timer.TimeTickMiliseconds += TimeTickHandler;
    }

    private void OnDisable()
    {
        Timer.TimeTickMiliseconds -= TimeTickHandler;
    }

    private void SetDay(int day)
    {
        _dayText.text = $"Day {day + 1}";
    }

    private void SetTime(int hour, int minutes)
    {
        _timeText.text = $"{hour:D2}:{minutes:D2}";
    }

    private void TimeTickHandler()
    {
        _totalTimePassed += _timeTickInGameTime;
        if (_totalTimePassed >= _totalMinutesInDay)
        {
            Timer.TimeTickMiliseconds -= TimeTickHandler;
            _totalTimePassed = _totalMinutesInDay;
            References.DukkanPage.OnDayTimeEnded();
        }

        int hour = Configs.LevelConfig.DayTimeInfo.DayStartTime + (int)(_totalTimePassed / 60);
        int minutes = (int)(_totalTimePassed % 60);
        SetTime(hour, minutes);
    }

}
