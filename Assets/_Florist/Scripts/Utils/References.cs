using UnityEngine;

public class References : MonoBehaviour
{
    private static References s_instance;

    public static DukkanPage DukkanPage => s_instance._dukkanPage;
    public static ShopPage ShopPage => s_instance._shopPage;
    public static WorkshopPage WorkshopPage => s_instance._workshopPage;
    public static EndDayPage EndDayPage => s_instance._endDayPage;
    public static DayTimeManager DayTimeManager => s_instance._dayTimeManager;
    public static HappinessMeter HappinessMeter => s_instance._happinessMeter;

    [SerializeField] private DukkanPage _dukkanPage = null;
    [SerializeField] private ShopPage _shopPage = null;
    [SerializeField] private WorkshopPage _workshopPage = null;
    [SerializeField] private EndDayPage _endDayPage = null;
    [SerializeField] private DayTimeManager _dayTimeManager;
    [SerializeField] private HappinessMeter _happinessMeter;

    private void Awake() => s_instance = this;
}
