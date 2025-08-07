using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Crashlytics;
using Firebase.Analytics;
using System.Collections;

public class FirebaseController : MonoBehaviour
{

    #region Variables
    private static FirebaseController _instance;
    public static FirebaseController Instance
    {
        get { return _instance; }
    }

    private bool _firebaseReady = false;
    private FirebaseApp _firebaseApp;
    private bool _sendTutorialStartEvent = false;
    private bool _sendTutorialEndEvent = false;
    private List<CustomEventParams> _customEventsToSend = new();
    private List<CustomEventStringParams> _customEventsStringToSend = new();
    private List<CustomEventDoubleParams> _customEventsDoubleToSend = new();
    private List<CustomEventIntParams> _customEventsIntToSend = new();
    private List<CustomEventCustomParams> _customEventsWithCustomParamsToSend = new();
    private List<LevelUpEventParams> _levelUpEventsToSend = new();
    private List<LevelStartEventParams> _levelStartEventsToSend = new();
    private List<LevelEndEventParams> _levelEndEventsToSend = new();
    private List<MoneyEarnEventParams> _moneyEarnEventsToSend = new();
    private List<MoneySpendEventParams> _moneySpendEventsToSend = new();
    private List<AdImpressionEventParams> _adImpressionEventsToSend = new();
    #endregion


    #region Initialization
    private void Awake()
    {
        if (!_instance)
        {
            _customEventsToSend = new List<CustomEventParams>();
            _customEventsStringToSend = new List<CustomEventStringParams>();
            _customEventsDoubleToSend = new List<CustomEventDoubleParams>();
            _customEventsIntToSend = new List<CustomEventIntParams>();
            _customEventsWithCustomParamsToSend = new List<CustomEventCustomParams>();
            _levelUpEventsToSend = new List<LevelUpEventParams>();
            _levelStartEventsToSend = new List<LevelStartEventParams>();
            _levelEndEventsToSend = new List<LevelEndEventParams>();
            _moneyEarnEventsToSend = new List<MoneyEarnEventParams>();
            _moneySpendEventsToSend = new List<MoneySpendEventParams>();
            _adImpressionEventsToSend = new List<AdImpressionEventParams>();

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _firebaseReady = false;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(Initializer());
    }

    private IEnumerator Initializer()
    {
        yield return new WaitForSeconds(.3f);
        InitializeFirebase();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                _firebaseApp = FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
                _firebaseReady = true;

#if UNITY_IOS
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
#endif

                //check event to send
                for (int i = 0; i < _customEventsToSend.Count; i++)
                    SendCustomEvent(_customEventsToSend[i].EventName);
                _customEventsToSend.Clear();

                for (int i = 0; i < _customEventsStringToSend.Count; i++)
                    SendCustomEvent(_customEventsStringToSend[i].EventName, _customEventsStringToSend[i].EventParam, _customEventsStringToSend[i].ParamValue);
                _customEventsStringToSend.Clear();

                for (int i = 0; i < _customEventsDoubleToSend.Count; i++)
                    SendCustomEvent(_customEventsDoubleToSend[i].EventName, _customEventsDoubleToSend[i].EventParam, _customEventsDoubleToSend[i].ParamValue);
                _customEventsDoubleToSend.Clear();

                for (int i = 0; i < _customEventsIntToSend.Count; i++)
                    SendCustomEvent(_customEventsIntToSend[i].EventName, _customEventsIntToSend[i].EventParam, _customEventsIntToSend[i].ParamValue);
                _customEventsIntToSend.Clear();

                for (int i = 0; i < _customEventsWithCustomParamsToSend.Count; i++)
                    SendCustomEventParam(_customEventsWithCustomParamsToSend[i].EventName, _customEventsWithCustomParamsToSend[i].CustomParams);
                _customEventsWithCustomParamsToSend.Clear();

                for (int i = 0; i < _levelUpEventsToSend.Count; i++)
                    SendLevelUpEvent(_levelUpEventsToSend[i].ParamCount, _levelUpEventsToSend[i].CharacterName, _levelUpEventsToSend[i].Level);
                _levelUpEventsToSend.Clear();

                for (int i = 0; i < _levelStartEventsToSend.Count; i++)
                    SendLevelStartEvent(_levelStartEventsToSend[i].ParamCount, _levelStartEventsToSend[i].LevelName);
                _levelStartEventsToSend.Clear();

                for (int i = 0; i < _levelEndEventsToSend.Count; i++)
                    SendLevelEndEvent(_levelEndEventsToSend[i].ParamCount, _levelEndEventsToSend[i].LevelName, _levelEndEventsToSend[i].LevelSuccess);
                _levelEndEventsToSend.Clear();

                for (int i = 0; i < _moneyEarnEventsToSend.Count; i++)
                    SendMoneyEarnEvent(_moneyEarnEventsToSend[i].ParamCount, _moneyEarnEventsToSend[i].CurrencyName, _moneyEarnEventsToSend[i].CurrencyValue);
                _moneyEarnEventsToSend.Clear();

                for (int i = 0; i < _moneySpendEventsToSend.Count; i++)
                    SendMoneySpendEvent(_moneySpendEventsToSend[i].ParamCount, _moneySpendEventsToSend[i].ItemBought, _moneySpendEventsToSend[i].CurrencyName, _moneySpendEventsToSend[i].CurrencyValue);
                _moneySpendEventsToSend.Clear();

                for (int i = 0; i < _adImpressionEventsToSend.Count; i++)
                    SendAdImpressionEvent(_adImpressionEventsToSend[i].ParamCount, _adImpressionEventsToSend[i].AdPlatform, _adImpressionEventsToSend[i].AdFormat, _adImpressionEventsToSend[i].AdSource, _adImpressionEventsToSend[i].AdUnitName, _adImpressionEventsToSend[i].AdCurrency, _adImpressionEventsToSend[i].AdValue);
                _adImpressionEventsToSend.Clear();

                if (_sendTutorialStartEvent)
                {
                    _sendTutorialStartEvent = false;
                    SendTutorialStartEvent();
                }

                if (_sendTutorialEndEvent)
                {
                    _sendTutorialEndEvent = false;
                    SendTutorialEndEvent();
                }
            }
            else
            {
                Debug.LogError(string.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }
    #endregion


    #region Crashlytics
    public void SendException(Exception e)
    {
        Debug.LogError(e.ToString());
        if (!_firebaseReady) return;
        Crashlytics.Log(e.Message);
        Crashlytics.LogException(e);
    }

    public void SetCrashlyticsLog(string log)
    {
        Debug.LogWarning(log);
        if (!_firebaseReady) return;
        Crashlytics.Log(log);
    }

    public void SetCrashlyticsKeyValue(string key, string value)
    {
        if (!_firebaseReady) return;
        Crashlytics.SetCustomKey(key, value);
    }
    #endregion


    #region Analytics
    // for all events:
    // https://firebase.google.com/docs/reference/unity/class/firebase/analytics/firebase-analytics

    public void SendCustomEvent(string eventName)
    {
        if (!_firebaseReady)
        {
            CustomEventParams eventToSend = new(eventName);
            _customEventsToSend.Add(eventToSend);
            return;
        }

        Debug.Log($"#event# SendCustomEvent: {eventName}");
        FirebaseAnalytics.LogEvent(eventName);
    }

    public void SendCustomEvent(string eventName, string paramName, string paramValue)
    {
        if (!_firebaseReady)
        {
            CustomEventStringParams eventToSend = new(eventName, paramName, paramValue);
            _customEventsStringToSend.Add(eventToSend);
            return;
        }

        Debug.Log($"#event# SendCustomEvent: {eventName}");
        FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
    }

    public void SendCustomEvent(string eventName, string paramName, double paramValue)
    {
        if (!_firebaseReady)
        {
            CustomEventDoubleParams eventToSend = new(eventName, paramName, paramValue);
            _customEventsDoubleToSend.Add(eventToSend);
            return;
        }

        Debug.Log($"#event# SendCustomEvent: {eventName}");
        FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
    }

    public void SendCustomEvent(string eventName, string paramName, int paramValue)
    {
        if (!_firebaseReady)
        {
            CustomEventIntParams eventToSend = new(eventName, paramName, paramValue);
            _customEventsIntToSend.Add(eventToSend);
            return;
        }

        Debug.Log($"#event# SendCustomEvent: {eventName}");
        FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
    }

    public void SendCustomEventParam(string eventName, CustomParams customParams)
    {
        if (!_firebaseReady)
        {
            CustomEventCustomParams eventToSend = new(eventName, customParams);
            _customEventsWithCustomParamsToSend.Add(eventToSend);
            return;
        }

        Debug.Log($"#event# SendCustomEventParam: {eventName}");
        if (customParams != null)
        {
            int paramIndex = 0;
            Parameter[] paramArray = new Parameter[customParams.EventParams.Count];
            for (int i = 0; i < customParams.EventParams.Count; i++)
            {
                paramArray[paramIndex++] = new Parameter(customParams.EventParams[i], customParams.ParamValueStrings[i]);
            }
            FirebaseAnalytics.LogEvent(eventName, paramArray);
        }
        else
        {
            FirebaseAnalytics.LogEvent(eventName);
        }
    }

    public void SendLevelUpEvent(int paramCount, string characterName = "", int level = int.MaxValue)
    {
        if (!_firebaseReady)
        {
            LevelUpEventParams eventToSend = new LevelUpEventParams(paramCount, characterName, level);
            _levelUpEventsToSend.Add(eventToSend);
            return;
        }
        int paramIndex = 0;
        Parameter[] paramArray = new Parameter[paramCount];
        if (characterName.Length != 0)
            paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterCharacter, characterName);
        if (level != int.MaxValue)
            paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterLevel, level);

        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelUp, paramArray);
    }

    public void SendTutorialStartEvent()
    {
        if (!_firebaseReady)
        {
            _sendTutorialStartEvent = true;
            return;
        }
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventTutorialBegin);
    }

    public void SendTutorialEndEvent()
    {
        if (!_firebaseReady)
        {
            _sendTutorialEndEvent = true;
            return;
        }
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventTutorialComplete);
    }

    public void SendLevelStartEvent(int paramCount, string levelName = "")
    {
        if (!_firebaseReady)
        {
            LevelStartEventParams eventToSend = new LevelStartEventParams(paramCount, levelName);
            _levelStartEventsToSend.Add(eventToSend);
            return;
        }
        if (paramCount == 0)
        {
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelStart);
        }
        else
        {
            int paramIndex = 0;
            Parameter[] paramArray = new Parameter[paramCount];
            if (levelName.Length != 0)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterLevelName, levelName);

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelStart, paramArray);
        }
    }

    public void SendLevelEndEvent(int paramCount, string levelName = "", string levelSuccess = "")
    {
        if (!_firebaseReady)
        {
            LevelEndEventParams eventToSend = new(paramCount, levelName, levelSuccess);
            _levelEndEventsToSend.Add(eventToSend);
            return;
        }
        if (paramCount == 0)
        {
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelEnd);
        }
        else
        {
            int paramIndex = 0;
            Parameter[] paramArray = new Parameter[paramCount];
            if (levelName.Length != 0)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterLevelName, levelName);
            if (levelSuccess.Length != 0)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterSuccess, levelSuccess);

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelEnd, paramArray);
        }
    }

    public void SendMoneyEarnEvent(int paramCount, CurrencyUsed currencyName = CurrencyUsed.none, double currencyValue = double.MaxValue)
    {
        if (!_firebaseReady)
        {
            MoneyEarnEventParams eventToSend = new(paramCount, currencyName, currencyValue);
            _moneyEarnEventsToSend.Add(eventToSend);
            return;
        }
        if (paramCount == 0)
        {
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventEarnVirtualCurrency);
        }
        else
        {
            int paramIndex = 0;
            Parameter[] paramArray = new Parameter[paramCount];
            if (currencyName != CurrencyUsed.none)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterVirtualCurrencyName, currencyName.ToString());
            if (currencyValue != double.MaxValue)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterValue, currencyValue);

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventEarnVirtualCurrency, paramArray);
        }
    }

    public void SendMoneySpendEvent(int paramCount, ItemTypes itemBought = ItemTypes.none, CurrencyUsed currencyName = CurrencyUsed.none, double currencyValue = double.MaxValue)
    {
        if (!_firebaseReady)
        {
            MoneySpendEventParams eventToSend = new(paramCount, itemBought, currencyName, currencyValue);
            _moneySpendEventsToSend.Add(eventToSend);
            return;
        }
        if (paramCount == 0)
        {
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSpendVirtualCurrency);
        }
        else
        {
            int paramIndex = 0;
            Parameter[] paramArray = new Parameter[paramCount];
            if (itemBought != ItemTypes.none)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterItemName, itemBought.ToString());
            if (currencyName != CurrencyUsed.none)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterVirtualCurrencyName, currencyName.ToString());
            if (currencyValue != double.MaxValue)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterValue, currencyValue);

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSpendVirtualCurrency, paramArray);
        }
    }

    public void SendAdImpressionEvent(int paramCount, string adPlatform = "", string adFormat = "", string adSource = "", string adUnitName = "", string adCurrency = "", double adValue = double.MaxValue)
    {
        if (!_firebaseReady)
        {
            AdImpressionEventParams eventToSend = new(paramCount, adPlatform, adFormat, adSource, adUnitName, adCurrency, adValue);
            _adImpressionEventsToSend.Add(eventToSend);
            return;
        }
        if (paramCount == 0)
        {
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAdImpression);
        }
        else
        {
            int paramIndex = 0;
            Parameter[] paramArray = new Parameter[paramCount];
            if (adPlatform.Length != 0)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterAdPlatform, adPlatform);
            if (adFormat.Length != 0)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterAdFormat, adFormat);
            if (adSource.Length != 0)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterAdSource, adSource);
            if (adUnitName.Length != 0)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterAdUnitName, adUnitName);
            if (adCurrency.Length != 0)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterCurrency, adCurrency);
            if (adValue != double.MaxValue)
                paramArray[paramIndex++] = new Parameter(FirebaseAnalytics.ParameterValue, adValue);

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAdImpression, paramArray);
        }
    }

    public enum ItemTypes
    {
        none,
    }

    public enum CurrencyUsed
    {
        none, gold, gem
    }
    #endregion


    #region EventParameters
    class CustomEventParams
    {
        public string EventName;

        public CustomEventParams(string eventName)
        {
            EventName = eventName;
        }
    }

    class CustomEventStringParams
    {
        public string EventName;
        public string EventParam;
        public string ParamValue;

        public CustomEventStringParams(string eventName, string eventParam, string paramValue)
        {
            EventName = eventName;
            EventParam = eventParam;
            ParamValue = paramValue;
        }
    }

    class CustomEventDoubleParams
    {
        public string EventName;
        public string EventParam;
        public double ParamValue;

        public CustomEventDoubleParams(string eventName, string eventParam, double paramValue = double.MaxValue)
        {
            EventName = eventName;
            EventParam = eventParam;
            ParamValue = paramValue;
        }
    }

    class CustomEventIntParams
    {
        public string EventName;
        public string EventParam;
        public int ParamValue;

        public CustomEventIntParams(string eventName, string eventParam, int paramValue = int.MaxValue)
        {
            EventName = eventName;
            EventParam = eventParam;
            ParamValue = paramValue;
        }
    }

    class CustomEventCustomParams
    {
        public string EventName;
        public CustomParams CustomParams;

        public CustomEventCustomParams(string eventName, CustomParams customParams)
        {
            EventName = eventName;
            CustomParams = customParams;
        }
    }

    class LevelUpEventParams
    {
        public int ParamCount;
        public string CharacterName;
        public int Level;

        public LevelUpEventParams(int paramCount, string characterName, int level)
        {
            ParamCount = paramCount;
            CharacterName = characterName;
            Level = level;
        }
    }

    class LevelStartEventParams
    {
        public int ParamCount;
        public string LevelName;

        public LevelStartEventParams(int paramCount, string levelName)
        {
            LevelName = levelName;
            ParamCount = paramCount;
        }
    }

    class LevelEndEventParams
    {
        public int ParamCount;
        public string LevelName;
        public string LevelSuccess;

        public LevelEndEventParams(int paramCount, string levelName, string levelSuccess)
        {
            LevelName = levelName;
            ParamCount = paramCount;
            LevelSuccess = levelSuccess;
        }
    }

    class MoneyEarnEventParams
    {
        public int ParamCount;
        public CurrencyUsed CurrencyName;
        public double CurrencyValue;

        public MoneyEarnEventParams(int paramCount, CurrencyUsed currencyName, double currencyValue)
        {
            ParamCount = paramCount;
            CurrencyName = currencyName;
            CurrencyValue = currencyValue;
        }
    }

    class MoneySpendEventParams
    {
        public int ParamCount;
        public ItemTypes ItemBought;
        public CurrencyUsed CurrencyName;
        public double CurrencyValue;

        public MoneySpendEventParams(int paramCount, ItemTypes itemBought, CurrencyUsed currencyName, double currencyValue)
        {
            ParamCount = paramCount;
            ItemBought = itemBought;
            CurrencyName = currencyName;
            CurrencyValue = currencyValue;
        }
    }

    class AdImpressionEventParams
    {
        public int ParamCount;
        public string AdPlatform;
        public string AdFormat;
        public string AdSource;
        public string AdUnitName;
        public string AdCurrency;
        public double AdValue;

        public AdImpressionEventParams(int paramCount, string adPlatform, string adFormat, string adSource, string adUnitName, string adCurrency, double adValue)
        {
            ParamCount = paramCount;
            AdPlatform = adPlatform;
            AdFormat = adFormat;
            AdSource = adSource;
            AdUnitName = adUnitName;
            AdCurrency = adCurrency;
            AdValue = adValue;
        }
    }

    public class CustomParams
    {
        public List<string> EventParams;
        public List<string> ParamValueStrings;

        public CustomParams()
        {
        }

        public CustomParams(List<string> eventParam, List<string> paramValueString)
        {
            EventParams = eventParam;
            ParamValueStrings = paramValueString;
        }
    }
    #endregion


}