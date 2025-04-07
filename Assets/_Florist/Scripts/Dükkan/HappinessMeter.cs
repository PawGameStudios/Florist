using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HappinessMeter : MonoBehaviour
{
    [SerializeField] private Image _emojiImage;
    [SerializeField] private TextMeshProUGUI _happinessText;
    [SerializeField] private List<Sprite> _emojiSprites;
    private int _totalTicksForCustomer, _currentTick = 0;


    private void OnEnable()
    {
        int currentday = SaveSystem.Inst.GeneralData.CurrentDayIndex;
        int customerCount = Configs.LevelConfig.Days[currentday].Customers.Count;
        float totalTickCount = Configs.LevelConfig.DayTimeInfo.DayDuration * 60 * 10f;
        float ticksPerCustomer = totalTickCount / customerCount;
        _totalTicksForCustomer = (int)(ticksPerCustomer * 10);
    }

    private void OnDisable()
    {
        Timer.TimeTickMiliseconds -= TimeTickHandler;
    }

    public void StartCountdown()
    {
        _currentTick = 0;
        Timer.TimeTickMiliseconds -= TimeTickHandler;
        Timer.TimeTickMiliseconds += TimeTickHandler;
    }

    public void StopCountdown()
    {
        Timer.TimeTickMiliseconds -= TimeTickHandler;
    }

    private void TimeTickHandler()
    {
        _currentTick++;
        int value = 100 - (int)((float)_currentTick / _totalTicksForCustomer * 100);
        _happinessText.text = $"{value}%";
    }
}
