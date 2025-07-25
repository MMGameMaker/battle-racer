#if USE_APPSFLYER
using AppsFlyerSDK;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

public class AppsflyerHelper : MonoBehaviour
{
    private static AppsflyerHelper instance;

    private static string TAG;

    private void Awake()
    {
        TAG = "[" + name + "] ";
        instance = this;
    }

    private void Start()
    {
        try
        {
#if USE_APPSFLYER
            AppsFlyer.setIsDebug(false);
            //LoadEvent();
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError(TAG + " Init Exception: " + ex.Message);
        }
    }


    public static void Log(eventId id)
    {
        if (instance == null && eventDic == null)
            return;

        try
        {
            int count = SetEvent(id);

            if (id == eventId.level_up || id == eventId.session)
            {
#if USE_APPSFLYER
                AppsFlyer.sendEvent(id.ToString(), new Dictionary<string, string> { { id.ToString(), count.ToString() } });
#endif
                Debug.Log(TAG + "Log " + id.ToString() + " " + count);
                return;
            }

            if (id == eventId.level_up
                || id == eventId.ad_banner_show || id == eventId.ad_banner_click
                || id == eventId.ad_interstitial_show || id == eventId.ad_interstitial_click
                || id == eventId.ad_videorewared_show || id == eventId.ad_videorewared_click)
            {
#if USE_APPSFLYER
                AppsFlyer.sendEvent(id.ToString(), new Dictionary<string, string> { { id.ToString(), count.ToString() } });
#endif
                Debug.Log(TAG + "Log " + id.ToString() + " " + count);
            }

            string counter = count.ToString();
            if (count > 10)
                counter = "N";
            string eventName = id + "_" + counter;

            if ((id == eventId.ad_interstitial_show || id == eventId.ad_interstitial_click) && count >= 5 && count % 5 != 0)
            {
                Debug.Log(TAG + "Log " + eventName + " RETURN " + count);
                return;
            }
            else if ((id == eventId.ad_videorewared_show || id == eventId.ad_videorewared_click) && count >= 5 && count % 5 != 0)
            {
                Debug.Log(TAG + "Log " + eventName + " RETURN " + count);
                return;
            }
            else if ((id == eventId.ad_banner_show || id == eventId.ad_banner_click) && count >= 10 && count % 10 != 0)
            {
                Debug.Log(TAG + "Log " + eventName + " RETURN " + count);
                return;
            }
#if USE_APPSFLYER
            AppsFlyer.sendEvent(eventName.ToLower(), new Dictionary<string, string> { { id.ToString(), count.ToString() } });
#endif
            Debug.Log(TAG + "Log " + eventName + " " + count);
        }
        catch (Exception ex)
        {
            Debug.LogError(TAG + "Log " + ex.Message);
        }
    }

    public static void LogUser(string eventName, UserData userData)
    {
        if (instance == null)
            return;

#if USE_APPSFLYER
        AppsFlyer.sendEvent(eventName.ToLower(),
            new Dictionary<string, string>
            {
                    //{ "session", user.Session.ToString() },
                    //{ "total_play", user.TotalPlay.ToString() },
                    //{ "total_ad_rewarded", user.TotalAdRewarded.ToString() },
                    //{ "total_ad_interstitial", user.TotalAdInterstitial.ToString() },
                    //{ "total_days_plays", ((DateTime.Now - user.FistTimeOpen).TotalDays).ToString("#0.0") },
                    //{ "current_version", user.VersionCurrent.ToString() },
                    //{ "install_version", user.VersionInstall.ToString() },
                    //{ "ab_testing", user.ABTesting }
            });
#endif
        Debug.Log(TAG + "LogUser " + eventName);
    }

    private static Dictionary<eventId, int> eventDic;
    private static void LoadEvent()
    {
        eventDic = new Dictionary<eventId, int>();
        foreach (eventId e in Enum.GetValues(typeof(eventId)))
        {
            int count = PlayerPrefs.GetInt(e.ToString(), 0);
            eventDic.Add(e, count);
        }
    }

    public static void SaveEvent()
    {
        if (eventDic != null)
        {
            foreach (var e in eventDic)
            {
                PlayerPrefs.SetInt(e.Key.ToString(), e.Value);
            }
            PlayerPrefs.Save();
        }
    }

    private static int SetEvent(eventId id, bool autoSave = true)
    {
        if (eventDic != null && eventDic.ContainsKey(id))
        {
            eventDic[id]++;
            int count = eventDic[id];
            if (autoSave)
            {
                PlayerPrefs.SetInt(id.ToString(), count);
                PlayerPrefs.Save();
            }
            return count;
        }
        return 0;
    }

    public enum eventId
    {
        ad_banner_show,
        ad_banner_click,
        ad_interstitial_show,
        ad_interstitial_click,
        ad_videorewared_show,
        ad_videorewared_click,
        session,
        level_up,
    }
}