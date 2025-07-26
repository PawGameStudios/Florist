using UnityEngine;

[DefaultExecutionOrder(-501)]
public class References : MonoBehaviour
{
    private static References s_instance;

    public static TopCanvas TopCanvas => s_instance._topCanvas;
    public static PosController PosController => s_instance._posController;
    public static MainPage MainPage => s_instance._mainPage;
    public static DukkanPage DukkanPage => s_instance._dukkanPage;
    public static ShopPage ShopPage => s_instance._shopPage;
    public static WorkshopPage WorkshopPage => s_instance._workshopPage;
    public static EndDayPage EndDayPage => s_instance._endDayPage;
    public static DayTimeManager DayTimeManager => s_instance._dayTimeManager;
    public static HappinessMeter HappinessMeter => s_instance._happinessMeter;
    public static ProfileMenu ProfileMenu => s_instance._profileMenu;
    public static DecorationManager DecorationManager => s_instance._decorationManager;

    [SerializeField] private TopCanvas _topCanvas = null;
    [SerializeField] private PosController _posController = null;
    [SerializeField] private MainPage _mainPage = null;
    [SerializeField] private DukkanPage _dukkanPage = null;
    [SerializeField] private ShopPage _shopPage = null;
    [SerializeField] private WorkshopPage _workshopPage = null;
    [SerializeField] private EndDayPage _endDayPage = null;
    [SerializeField] private DayTimeManager _dayTimeManager;
    [SerializeField] private HappinessMeter _happinessMeter;
    [SerializeField] private ProfileMenu _profileMenu;
    [SerializeField] private DecorationManager _decorationManager;

    private void Awake() => s_instance = this;
}
