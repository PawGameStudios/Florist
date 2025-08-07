using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

public class NotificationManager : MonoBehaviour
{
    private enum NotifType
    {
        General, Merge
    }

    private void Start()
    {
        CheckIfNotificationClicked();
        SendDefaultNotifications();
    }

    private void OnEnable()
    {
        GeneralData.OnNotificationSettingsChanged += OnNotificationSettingsChangedHandler;
    }

    private void OnDisable()
    {
        GeneralData.OnNotificationSettingsChanged -= OnNotificationSettingsChangedHandler;
    }

    private void SendDefaultNotifications()
    {
        SendNotification(GetNotifTitle(NotifType.General), GetNotifText(NotifType.General), "three_day", new TimeSpan(72, 0, 0));
        SendNotification(GetNotifTitle(NotifType.General), GetNotifText(NotifType.General), "tomorrow", new TimeSpan(24, 0, 0));
        SendNotification(GetNotifTitle(NotifType.General), GetNotifText(NotifType.General), "today", new TimeSpan(1, 0, 0));
    }

    private static string GetNotifTitle(NotifType notifType)
    {
        string title = "";
        switch (notifType)
        {
            case NotifType.General:
                string[] titles = new string[]
                    {
                        "💎 We missed you!",
                        "💎 Haven't seen you in a while!",
                        "💎 Let's train your brain!",
                        "💎 Are you bored?",
                        "💎 Long time no see...",
                    };
                int randInd = UnityEngine.Random.Range(0, titles.Length);
                title = titles[randInd];
                break;
            case NotifType.Merge:
                titles = new string[]
                    {
                        "💰 Claim Your Merge Reward! 💰",
                        "💰 Your Merge is Waiting! 💰",
                        "💰 Merge Wheel is READY! 💰",
                        "💎 Claim Your Merge Reward! 💎",
                        "💎 Your Daily Merge is Ready! 💎",
                        "👑 Daily Merge is Ready for Claim! 👑"
                    };
                randInd = UnityEngine.Random.Range(0, titles.Length);
                title = titles[randInd];
                break;
            default:
                break;
        }
        return title;
    }

    private static string GetNotifText(NotifType notifType)
    {
        string title = "";
        switch (notifType)
        {
            case NotifType.General:
                string[] titles = new string[]
                    {
                        "Click the notification to receive your reward!",
                        "Click the notification to play!",
                        "Come and play!",
                    };
                int randInd = UnityEngine.Random.Range(0, titles.Length);
                title = titles[randInd];
                break;
            case NotifType.Merge:
                titles = new string[]
                    {
                        "Your daily merge has been replenished!",
                        "Merge the items and win prizes!",
                    };
                randInd = UnityEngine.Random.Range(0, titles.Length);
                title = titles[randInd];
                break;
            default:
                break;
        }
        return title;
    }

    private void CheckIfNotificationClicked()
    {
#if UNITY_ANDROID

        try
        {
            var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
            if (notificationIntentData != null)
            {
                FirebaseController.Instance.SetCrashlyticsLog("NotificationClicked");

                string notifMessage = notificationIntentData.Notification.IntentData;
                FirebaseController.CustomParams customParams = new()
                {
                    EventParams = new List<string>() { "notifId" },
                    ParamValueStrings = new List<string>() { notifMessage }
                };
                FirebaseController.Instance.SendCustomEventParam(eventName: "notif_time", customParams);

                // If intent data is not null, it will not be cleared for this session thus each time menu is opened, 
                // new events will be send causing wrong data events. Prevent this by using a flag.
                // GameManager.GameOpenedAfterNotif = true;
            }
        }
        catch (Exception e)
        {
            string errLog = "CheckIfNotificationClicked\n";
            FirebaseController.Instance.SetCrashlyticsLog(errLog);
            FirebaseController.Instance.SendException(e);
        }
#endif
    }

    private void OnNotificationsCancelled()
    {
        AndroidNotificationCenter.CancelAllNotifications();
    }

    private static int SendNotification(string notifTitle, string notifText, string intent, TimeSpan notifTime)
    {
        int notifId = -1;
        if (!SaveSystem.Inst.GeneralData.IsNotificationsOn)
            return notifId;

        DateTime fireTime = DateTime.Now.Add(notifTime);
        var twoDaysNotification = new AndroidNotification
        {
            Title = notifTitle,
            Text = notifText,
            LargeIcon = "icon_large",
            FireTime = fireTime,
            IntentData = intent
        };

        notifId = AndroidNotificationCenter.SendNotification(twoDaysNotification, "channel_id");

        return notifId;
    }

    private void OnNotificationSettingsChangedHandler()
    {
        if (!SaveSystem.Inst.GeneralData.IsNotificationsOn)
            OnNotificationsCancelled();
    }

}