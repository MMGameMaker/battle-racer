using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_ANDROID && USE_NOTI
using Unity.Notifications.Android;
#endif
#if UNITY_IOS && USE_NOTI
using Unity.Notifications.iOS;
#endif
using UnityEngine;

public class NotifiManager : MonoBehaviour
{
    public static string channelId = "default";
    public static bool IsInit = false;
    public bool IsAutoCreate = false;

    [Header("Creat --> Notification Manager --> NotiItem")]
    public List<NotiItem> notiItems;

    /// <summary>
    /// The delegate type for the notification received callbacks.
    /// </summary>
    public delegate void NotificationReceivedCallback(string data);

    /// <summary>
    /// Subscribe to this event to receive callbacks whenever a scheduled notification is shown to the user.
    /// </summary>
    public static event NotificationReceivedCallback OnNotificationReceived = delegate { };

    public static NotifiManager instance = null;

    private void Awake()
    {
        instance = this;
    }
    private void OnApplicationQuit()
    {
        if (IsAutoCreate)
            CreateAll();
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            if (IsAutoCreate)
                CreateAll();
        }
        else
        {
            if (IsAutoCreate)
                CancelAll();
        }
    }

    public static IEnumerator DOInit()
    {
#if UNITY_ANDROID && USE_NOTI
        var channel = new AndroidNotificationChannel()
        {
            Id = channelId,
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "Generic notifications",
            EnableLights = true,
            EnableVibration = true,
            CanShowBadge = true,
            LockScreenVisibility = LockScreenVisibility.Public,
            VibrationPattern = new long[] { 100, 200, 300, 400, 500 }
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        AndroidNotificationCenter.OnNotificationReceived += ReceivedNotificationHandler;
#endif

#if UNITY_IOS && USE_NOTI
        var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
        using (var req = new AuthorizationRequest(authorizationOption, true))
        {
            while (!req.IsFinished)
            {
                yield return null;
            };

            string res = "\n RequestAuthorization:";
            res += "\n finished: " + req.IsFinished;
            res += "\n granted :  " + req.Granted;
            res += "\n error:  " + req.Error;
            res += "\n deviceToken:  " + req.DeviceToken;
            Debug.Log(res);
        }

        iOSNotificationCenter.OnNotificationReceived += ReceivedNotificationHandler;
        iOSNotificationCenter.OnRemoteNotificationReceived += ReceivedNotificationHandler;
#endif

        IsInit = true;
        yield return null;

        CancelAll();
    }

#if UNITY_ANDROID && USE_NOTI
    private static void ReceivedNotificationHandler(AndroidNotificationIntentData data)
    {
        if (!IsInit)
        {
            Debug.LogWarning("NotifiManager NOT init");
            return;
        }

        if (data != null)
        {
            string msg = "Notification received : " + data.Id + "\n";
            msg += "\n .Title: " + data.Notification.Title;
            msg += "\n .Body: " + data.Notification.Text;
            msg += "\n .Channel: " + data.Channel;
            msg += "\n .Data: " + data.Notification.IntentData;
            Debug.Log(msg);
            OnNotificationReceived?.Invoke(data.Notification.IntentData);
        }
    }
#endif

#if UNITY_IOS && USE_NOTI
    private static void ReceivedNotificationHandler(iOSNotification notification)
    {
        if (!IsInit)
        {
            Debug.LogWarning("NOT init");
            return;
        }

        if (notification != null)
        {
            string msg = "Notification received: " + notification.Identifier + "\n";
            msg += "\n - .Title: " + notification.Title;
            msg += "\n - .Badge: " + notification.Badge;
            msg += "\n - .Body: " + notification.Body;
            msg += "\n - .CategoryIdentifier: " + notification.CategoryIdentifier;
            msg += "\n - .Subtitle: " + notification.Subtitle;
            msg += "\n - .Data: " + notification.Data;
            Debug.Log(msg);


            OnNotificationReceived?.Invoke(notification.Data);
        }
    }
#endif

    public void CreateAll()
    {
        foreach (var item in notiItems)
        {
            if (item.titles != null && item.titles.Length > 0)
                item.title = item.titles[0];
            if (item.descriptions != null && item.descriptions.Length > 0)
                item.description = item.descriptions[UnityEngine.Random.Range(0, item.descriptions.Length)];
            CreateNoti(item);
        }
    }

    public static void CreateNoti(NotiItem item)
    {
        CreateNoti(item.title, item.description, item.fireTime, item.data, item.iconSmall, item.iconLarge);
    }

    public static void CreateNoti(string tile, string text, DateTime fireTime, string data = null, string iconSmall = null, string iconLarge = null)
    {
        if (!IsInit)
        {
            Debug.LogWarning("NotifiManager NOT init");
            return;
        }

#if UNITY_ANDROID && USE_NOTI
        try
        {
            var notification = new AndroidNotification
            {
                Title = tile,
                Text = text,
                FireTime = fireTime,
                SmallIcon = !string.IsNullOrEmpty(iconSmall) ? iconSmall : "icon_small",
                LargeIcon = !string.IsNullOrEmpty(iconLarge) ? iconLarge : "icon_large",
                GroupAlertBehaviour = GroupAlertBehaviours.GroupAlertAll
            };

            //JsonData
            if (!string.IsNullOrEmpty(data)) notification.IntentData = data;

            AndroidNotificationCenter.SendNotification(notification, channelId);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
#endif

#if UNITY_IOS && USE_NOTI
        var notification = new iOSNotification()
        {
            // You can specify a custom identifier which can be used to manage the notification later.
            // If you don't provide one, a unique string will be generated automatically.
            Identifier = "identifier_01",
            Title = tile,
            // Body = "Scheduled at: " + DateTime.Now.ToShortDateString() + " triggered in " + fireTime.ToString(),
            Subtitle = text,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = channelId,
            ThreadIdentifier = "threadIdentifier",
            Trigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = fireTime - DateTime.Now,
                Repeats = false
            }
        };

        //JsonData
        if (!string.IsNullOrEmpty(data)) notification.Data = data;

        iOSNotificationCenter.ScheduleNotification(notification);
#endif

        Debug.Log("Create Noti " + fireTime.ToString("HH:mm dd:MM:yyyy") + " " + tile);
    }


    public static NotiItem lastNotiItem = null;

    public void GetLast()
    {
        GetLastNotification(true);
    }

    public static NotiItem GetLastNotification(bool onDoneCancelAll)
    {
        if (!IsInit)
        {
            Debug.LogWarning("NotifiManager NOT init");
            return null;
        }

        string msg = "GetLastNotification : ";
#if UNITY_ANDROID && USE_NOTI
        var adnroidData = AndroidNotificationCenter.GetLastNotificationIntent();
        if (adnroidData != null)
        {
            lastNotiItem = new NotiItem(adnroidData.Id.ToString(), adnroidData.Notification.Title, adnroidData.Notification.Text, adnroidData.Notification.IntentData);
            AndroidNotificationCenter.CancelDisplayedNotification(adnroidData.Id);

            msg += adnroidData.Id + "\n";
            msg += "\n .Title: " + adnroidData.Notification.Title;
            msg += "\n .Body: " + adnroidData.Notification.Text;
            msg += "\n .Data: " + adnroidData.Notification.IntentData;
        }
#endif


#if UNITY_IOS && USE_NOTI
        var iOSData = iOSNotificationCenter.GetLastRespondedNotification();
        if (iOSData != null)
        {
            lastNotiItem = new NotiItem(iOSData.Identifier, iOSData.Title, iOSData.Subtitle, iOSData.Data);
            iOSNotificationCenter.RemoveDeliveredNotification(iOSData.Identifier);

            msg += iOSData.Identifier + "\n";
            msg += "\n .Title: " + iOSData.Title;
            msg += "\n .Body: " + iOSData.Subtitle;
            msg += "\n .Data: " + iOSData.Data;
        }
#endif

        Debug.Log(msg);

        if (onDoneCancelAll)
            CancelAll();
        return lastNotiItem;
    }

    public static void CancelAll()
    {
        if (!IsInit)
        {
            Debug.LogWarning("NotifiManager NOT init");
            return;
        }

#if UNITY_ANDROID && USE_NOTI
        AndroidNotificationCenter.CancelAllNotifications();
#endif

#if UNITY_IOS && USE_NOTI
        iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
    }

    public static void Cancel(string id)
    {
        if (!IsInit)
        {
            Debug.LogWarning("NotifiManager NOT init");
            return;
        }

#if UNITY_ANDROID && USE_NOTI
        if (int.TryParse(id, out int androidId))
            AndroidNotificationCenter.CancelScheduledNotification(androidId);
#endif

#if UNITY_IOS && USE_NOTI
        iOSNotificationCenter.RemoveScheduledNotification(id);
#endif
    }
}
