#if USE_FIREBASE
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
#if USE_FIREBASE_MESSAGE
using Firebase.Messaging;
#endif
using Firebase.RemoteConfig;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Base
{
    public class FirebaseManager : MonoBehaviour
    {
        [SerializeField]
        protected bool initAtStart = true;
        private static bool isDebugMode { get; set; }

        public static WaitForEndOfFrame waitTime = null;
        public static string FirebaseToken = "";
        public static Dictionary<string, object> DefaultRemoteConfig = new Dictionary<string, object>();


        [SerializeField]
        private FirebaseStatus status = FirebaseStatus.UnAvailable;
        public static FirebaseStatus Status
        {
            get
            {
                if (instance)
                    return instance.status;
                return FirebaseStatus.Faulted;
            }
            private set
            {
                if (instance)
                    instance.status = value;
            }
        }

#if USE_FIREBASE_MESSAGE
        public static FirebaseMessage FirebaseMessage { get; set; }
#endif

        public static FirebaseStatus AnalyticStatus = FirebaseStatus.Initialing;
        public static FirebaseStatus RemoteStatus = FirebaseStatus.Initialing;
        public static FirebaseStatus MessageStatus = FirebaseStatus.Initialing;

        private static FirebaseManager instance { get; set; }

        private void Awake()
        {
            try
            {
                if (instance != null)
                    Destroy(gameObject);
                if (instance == null)
                    instance = this;
                DontDestroyOnLoad(gameObject);
            }
            catch (Exception ex)
            {
                Debug.LogError("[FirebaseHelper] Exception: " + ex.Message);
            }

            waitTime = new WaitForEndOfFrame();
        }

        private void Start()
        {
            if (initAtStart)
                StartCoroutine(DoCheckStatus());
        }

#region Base
        public static IEnumerator DoCheckStatus(Dictionary<string, object> remoteDefaultConfig = null, float timeOut = 2.5f)
        {
            if (instance == null)
            {
                Debug.LogError("[Firebase] NULL");
                yield break;
            }

#if USE_FIREBASE
            var elapsedTime = 0f;
            var startCheckingTime = Time.realtimeSinceStartup;
            Status = FirebaseStatus.Checking;

            Debug.Log("[Firebase] CheckDependencies: Checking");
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task != null)
                {
                    if (task.Result == DependencyStatus.Available)
                    {
                        Status = FirebaseStatus.Available;
                        Debug.Log("[Firebase] CheckDependencies: " + task.Result);
                    }
                    else
                    {
                        Debug.LogError("[Firebase] CheckDependencies: " + task.Result);
                    }
                }
                else
                {
                    Debug.LogError("[Firebase] CheckDependencies: task NULL");
                }
            });

            //yield return new WaitUntil(() => Status != FirebaseStatus.Checking || Time.realtimeSinceStartup - startCheckingTime > timeOut);
            while (Status == FirebaseStatus.Checking && elapsedTime < timeOut)
            {
                elapsedTime += Time.deltaTime;
                yield return waitTime;
            }

            if (Status == FirebaseStatus.Available)
            {
                AnalyticStatus = FirebaseStatus.Initialing;
                Debug.Log("[Firebase] Analytics " + "Initialing");

                try
                {
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    AnalyticStatus = FirebaseStatus.Initialized;
                    Debug.Log("[Firebase] Analytics " + "Initialized");
                }
                catch (FirebaseException ex)
                {
                    Debug.LogError("[Firebase] Analytics " + "Initialized " + ex.Message + "\n" + ex.StackTrace);
                    AnalyticStatus = FirebaseStatus.UnkownError;
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Firebase] Analytics " + "Initialized " + ex.Message + "\n" + ex.StackTrace);
                    AnalyticStatus = FirebaseStatus.UnkownError;
                }

                if (remoteDefaultConfig != null && remoteDefaultConfig.Count > 0)
                {
                    DefaultRemoteConfig = remoteDefaultConfig;
                    elapsedTime = 0f;
                    RemoteStatus = FirebaseStatus.Initialing;
                    Debug.Log("[Firebase] RemoteConfig " + "Initialing");
                    if (FirebaseRemoteConfig.DefaultInstance == null)
                    {
                        Debug.Log("[Firebase] RemoteConfig " + "DefaultInstance NULL");
                        while (FirebaseRemoteConfig.DefaultInstance == null && elapsedTime < timeOut)
                        {
                            elapsedTime += Time.deltaTime;
                            yield return waitTime;
                        }
                    }

                    try
                    {
                        FirebaseRemoteConfig.DefaultInstance?.SetDefaultsAsync(DefaultRemoteConfig)?.ContinueWithOnMainThread(task =>
                        {
                            RemoteStatus = LogTaskCompletion(task, "[Firebase] RemoteConfig SetDefaultsAsync");
                        });
                    }
                    catch (FirebaseException ex)
                    {
                        RemoteStatus = FirebaseStatus.Faulted;
                        Debug.LogError("[Firebase] RemoteConfig " + "Initialized " + ex.Message + "\n" + ex.StackTrace);
                    }
                    catch (Exception ex)
                    {
                        RemoteStatus = FirebaseStatus.Faulted;
                        Debug.LogError("[Firebase] RemoteConfig " + "Initialized " + ex.Message + "\n" + ex.StackTrace);
                    }

                    elapsedTime = 0f;
                    while (RemoteStatus == FirebaseStatus.Initialing && elapsedTime < timeOut)
                    {
                        elapsedTime += Time.deltaTime;
                        yield return waitTime;
                    }
                }

                try
                {
                    MessageStatus = FirebaseStatus.Initialing;
                    Debug.Log("[Firebase] Messaging " + "Initialing");

#if USE_FIREBASE_MESSAGE
                    FirebaseMessaging.TokenReceived += OnTokenReceived;
                    FirebaseMessaging.MessageReceived += OnMessageReceived;

                    ///FirebaseMessaging.SubscribeAsync(topic).ContinueWith(task =>
                    ///{
                    ///    LogTaskCompletion(task, "[Firebase] SubscribeAsync");
                    ///});

                    /// This will display the prompt to request permission to receive
                    /// notifications if the prompt has not already been displayed before. (If
                    /// the user already responded to the prompt, thier decision is cached by
                    /// the OS and can be changed in the OS settings).
                    FirebaseMessaging.RequestPermissionAsync().ContinueWith(task =>
                    {
                        MessageStatus = LogTaskCompletion(task, "[Firebase] Messaging RequestPermissionAsync");
                    });
                    Debug.Log("[Firebase] Messaging " + "Initialized");
#endif
                }
                catch (FirebaseException ex)
                {
                    MessageStatus = FirebaseStatus.Faulted;
                    Debug.LogError("[Firebase] Messaging " + "Initialized " + ex.Message + "\n" + ex.StackTrace);
                }
                catch (Exception ex)
                {
                    MessageStatus = FirebaseStatus.Faulted;
                    Debug.LogError("[Firebase] Messaging " + "Initialized " + ex.Message + "\n" + ex.StackTrace);
                }

                elapsedTime = 0f;
                while (MessageStatus == FirebaseStatus.Initialing && elapsedTime < timeOut)
                {
                    elapsedTime += Time.deltaTime;
                    yield return waitTime;
                }
            }
#endif
                }

        protected static FirebaseStatus LogTaskCompletion(Task task, string operation)
        {
            FirebaseStatus status = FirebaseStatus.Initialing;
#if USE_FIREBASE
            if (task.IsCanceled)
            {
                Debug.Log(operation + " Canceled");
                status = FirebaseStatus.Canceled;
            }
            else if (task.IsFaulted)
            {
                Debug.Log(operation + " Encounted an error");
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    string errorCode = "";
                    FirebaseException firebaseEx = exception as FirebaseException;
                    if (firebaseEx != null)
                    {
                        errorCode = string.Format("Error.{0}: ",
                          (firebaseEx.ErrorCode).ToString());
                    }
                    Debug.LogError(errorCode + exception.ToString());
                }
                status = FirebaseStatus.Faulted;
            }
            else if (task.IsCompleted)
            {
                Debug.Log(operation + " Completed");
                status = FirebaseStatus.Completed;
            }
#endif
            return status;
        }
#endregion

#region FirebaseRemoteConfigs
        public static IEnumerator DoFetchRemoteData(Action<FirebaseStatus> status, int cacheExpirationHours = 12, float timeOut = 2.5f)
        {

            Debug.Log("[Firebase] DoFetchRemoteData");
            if (instance == null)
            {
                Debug.LogError("[Firebase] NULL");
                status?.Invoke(FirebaseStatus.Faulted);
                yield break;
            }

#if USE_FIREBASE
            LogEvent("DoFetchRemoteData_" + FirebaseStatus.Checking.ToString());

            var elapsedTime = 0f;

            while (elapsedTime < timeOut && RemoteStatus != FirebaseStatus.Completed)
            {
                elapsedTime += Time.deltaTime;
                yield return waitTime;
            }

            if (!IsConnected)
            {
                Debug.LogError("[Firebase] DoFetchRemoteData NoInternet");
                RemoteStatus = FirebaseStatus.NoInternet;
                status?.Invoke(FirebaseStatus.NoInternet);
                LogEvent("DoFetchRemoteData_" + RemoteStatus.ToString());
                yield break;
            }

            if (Status != FirebaseStatus.Available || FirebaseRemoteConfig.DefaultInstance == null)
            {
                Debug.LogError("[Firebase] FirebaseRemoteConfig.DefaultInstance is null");
                RemoteStatus = FirebaseStatus.UnAvailable;
                status?.Invoke(FirebaseStatus.UnAvailable);
                LogEvent("DoFetchRemoteData_" + RemoteStatus.ToString());
                yield break;
            }

            elapsedTime = 0f;
            RemoteStatus = FirebaseStatus.Fetching;
            LogEvent("DoFetchRemoteData_" + RemoteStatus.ToString());

            FetchAsync((s) =>
            {
                if (RemoteStatus == FirebaseStatus.Fetching)
                {
                    RemoteStatus = s;
                    status?.Invoke(RemoteStatus);
                }
                LogEvent("DoFetchRemoteData_" + RemoteStatus.ToString());
            }, cacheExpirationHours);

            while (elapsedTime < timeOut && RemoteStatus == FirebaseStatus.Fetching)
            {
                elapsedTime += Time.deltaTime;
                yield return waitTime;
            }

            if (RemoteStatus == FirebaseStatus.Fetching)
            {
                Debug.LogError("[Firebase] DoFetchRemoteData TimeOut " + elapsedTime.ToString("0.0"));
                RemoteStatus = FirebaseStatus.TimeOut;
                status?.Invoke(RemoteStatus);
                LogEvent("DoFetchRemoteData_" + RemoteStatus.ToString());
            }
#endif
        }

        public static void FetchAsync(Action<FirebaseStatus> status, int cacheExpirationHours = 6)
        {
#if USE_FIREBASE
            if (isDebugMode) cacheExpirationHours = 0;

            FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromHours(cacheExpirationHours)).ContinueWithOnMainThread((fetchTask) =>
            {
                try
                {
                    if (!fetchTask.IsCompleted)
                    {
                        if (fetchTask?.Exception?.Flatten()?.InnerExceptions != null)
                        {
                            foreach (Exception exception in fetchTask.Exception.Flatten().InnerExceptions)
                            {
                                string errorCode = "";
                                FirebaseException firebaseEx = exception as FirebaseException;
                                if (firebaseEx != null)
                                {
                                    errorCode = string.Format("Error.{0}: ", (firebaseEx.ErrorCode).ToString());
                                }
                                Debug.LogError(errorCode + exception.ToString());
                            }
                        }

                        Debug.LogError("[Firebase] DoFetchRemoteData Fetch " + fetchTask?.Status + " " + fetchTask?.Exception?.Message);
                        status?.Invoke(FirebaseStatus.Faulted);
                        status = null;
                    }
                    else
                    {
                        Debug.Log("[Firebase] DoFetchRemoteData Fetch completed successfully!");
                        var info = FirebaseRemoteConfig.DefaultInstance.Info;
                        switch (info.LastFetchStatus)
                        {
                            case LastFetchStatus.Success:
                                if (RemoteStatus == FirebaseStatus.Fetching)
                                {
                                    FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(task =>
                                    {
                                        Debug.Log(string.Format("[Firebase] DoFetchRemoteData Remote data loaded and ready (last fetch time {0}).", info.FetchTime));

                                        string remoteData = "[Firebase] DoFetchRemoteData";
                                        foreach (var i in FirebaseRemoteConfig.DefaultInstance.Keys)
                                        {
                                            string key = i;
                                            remoteData += "\n" + key + ": " + FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
                                        }

                                        Debug.Log(remoteData);
                                        status?.Invoke(FirebaseStatus.Success);
                                        status = null;
                                    });
                                }
                                break;
                            case LastFetchStatus.Failure:
                                switch (info.LastFetchFailureReason)
                                {
                                    case FetchFailureReason.Error:
                                        Debug.LogError("[Firebase] DoFetchRemoteData LastFetchStatus.Failure:  Error -> Unknown reason");
                                        status?.Invoke(FirebaseStatus.UnkownError);
                                        break;
                                    case FetchFailureReason.Throttled:
                                        Debug.LogError("[Firebase] DoFetchRemoteData LastFetchStatus.Failure: Throttled -> until " + info.ThrottledEndTime);
                                        status?.Invoke(FirebaseStatus.TimeOut);
                                        break;
                                    default:
                                        Debug.LogError("[Firebase] DoFetchRemoteData LastFetchStatus.Failure: " + info.LastFetchFailureReason.ToString());
                                        status?.Invoke(FirebaseStatus.UnAvailable);
                                        break;
                                }
                                break;
                            case LastFetchStatus.Pending:
                                Debug.LogError("[Firebase] DoFetchRemoteData Latest Fetch call still pending.");
                                status?.Invoke(FirebaseStatus.Pending);
                                break;
                            default:
                                Debug.LogError("[Firebase] DoFetchRemoteData Unkown " + info.LastFetchStatus.ToString());
                                status?.Invoke(FirebaseStatus.UnAvailable);
                                break;
                        }
                    }
                }
                catch (FirebaseException ex)
                {
                    Debug.LogError("[Firebase] DoFetchRemoteData FirebaseException: " + ex.Message);
                    status?.Invoke(FirebaseStatus.UnkownError);
                    status = null;
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Firebase] DoFetchRemoteData Exception: " + ex.Message);
                    status?.Invoke(FirebaseStatus.UnkownError);
                    status = null;
                }
            });
#endif
        }

        public static string RemoteGetValueString(string title)
        {
            try
            {
#if USE_FIREBASE
                if (FirebaseRemoteConfig.DefaultInstance.Keys != null && FirebaseRemoteConfig.DefaultInstance.Keys.Contains(title))
                {
                    return FirebaseRemoteConfig.DefaultInstance.GetValue(title).StringValue;
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return "";
        }

        public static int RemoteGetValueInt(string title)
        {
            try
            {
#if USE_FIREBASE
                if (FirebaseRemoteConfig.DefaultInstance.Keys != null && FirebaseRemoteConfig.DefaultInstance.Keys.Contains(title))
                {
                    return (int)FirebaseRemoteConfig.DefaultInstance.GetValue(title).LongValue;
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return 0;
        }

        public static bool RemoteGetValueBoolean(string title)
        {
            try
            {
#if USE_FIREBASE
                if (FirebaseRemoteConfig.DefaultInstance.Keys != null && FirebaseRemoteConfig.DefaultInstance.Keys.Contains(title))
                {
                    return FirebaseRemoteConfig.DefaultInstance.GetValue(title).BooleanValue;
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return false;
        }

        public static float RemoteGetValueFloat(string title)
        {
            try
            {
#if USE_FIREBASE

                if (FirebaseRemoteConfig.DefaultInstance.Keys != null && FirebaseRemoteConfig.DefaultInstance.Keys.Contains(title))
                {
                    var style = NumberStyles.AllowDecimalPoint;
                    var culture = CultureInfo.CreateSpecificCulture("en-US");
                    float d = -1f;
                    if (float.TryParse(FirebaseRemoteConfig.DefaultInstance.GetValue(title).StringValue, style, culture, out d))
                    {
                        //Debug.Log("-------> Firebase Remote: " + title + "   " + FirebaseRemoteConfig.DefaultInstance.GetValue(title).StringValue + "  |  " + d);
                        return d;
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return -1;
        }

        public static byte[] RemoteGetValueArray(string title)
        {
            try
            {
#if USE_FIREBASE
                if (FirebaseRemoteConfig.DefaultInstance.Keys != null && FirebaseRemoteConfig.DefaultInstance.Keys.Contains(title))
                {
                    return (byte[])FirebaseRemoteConfig.DefaultInstance.GetValue(title).ByteArrayValue;
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return null;
        }
#endregion

#region FirebaseAnalytic
        public static void SetUserId()
        {
            if (DataManager.UserData != null)
            {
                string user_id = SystemInfo.deviceUniqueIdentifier;
#if USE_FIREBASE
                SetUser("user_id", user_id);
#endif
                string appsflyer_id = "";
#if USE_APPSFLYER
                //appsflyer_id = AppsFlyerSDK.AppsFlyer.getAppsFlyerId();
                //SetUser("appsflyer_id", appsflyer_id);
#endif

                string advertising_id = "";
                string ironSource_id = "";
#if USE_IRON
                Application.RequestAdvertisingIdentifierAsync((string ads_id, bool trackingEnabled, string error) =>
                {
                    advertising_id = ads_id;
                    SetUser("advertising_id", advertising_id);
                });

                ironSource_id = IronSource.Agent.getAdvertiserId();
                SetUser("ironSource_id", ironSource_id);
#endif
                LogEvent("set_user_property", new Dictionary<string, object>
            {
                { "user_id", user_id },
                { "appsflyer_id", appsflyer_id },
                { "advertising_id", advertising_id },
                { "ironSource_id", ironSource_id }
            });

                Debug.Log("SetUserId:"
                    + " user_id: " + user_id
                    + " appsflyer_id: " + appsflyer_id
                    + " advertising_id: " + advertising_id
                    + " ironSource_id: " + ironSource_id);
            }
        }

        public static void SetUser(string title, object property)
        {
#if USE_FIREBASE
            try
            {
                if (instance == null || AnalyticStatus != FirebaseStatus.Initialized || property == null)
                {
                    if (instance == null)
                    {
                        Debug.LogWarning("[Firebase] NULL");
                    }
                    else
                    {
                        Debug.LogWarning("[Firebase] AnalyticStatus: " + AnalyticStatus);
                    }
                    return;
                }

                FirebaseAnalytics.SetUserProperty(title, property.ToString());

                Debug.Log("[Firebase] SetUser: " + title + " property: " + property.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError("[Firebase] SetUser: " + ex.Message);
            }
#endif
        }

        public static void LogATTrackingStatus()
        {
#if UNITY_IOS
        LogEvent("ATTrackingStatus", new Dictionary<string, object> { { "att_status", Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus() } });
#endif
        }

        public static void LogEvent(string eventName, Dictionary<string, object> dictionary = null)
        {
#if USE_FIREBASE
            try
            {
                if (instance == null || AnalyticStatus != FirebaseStatus.Initialized)
                {
                    if (instance == null)
                    {
                        Debug.LogWarning("[Firebase] NULL");
                    }
                    else
                    {
                        Debug.LogWarning("[Firebase] AnalyticStatus: " + AnalyticStatus + " " + eventName);
                    }
                    return;
                }

                if (string.IsNullOrEmpty(eventName))
                {
                    Debug.LogWarning("eventName IsNullOrEmpty");
                    return;
                }

                if (isDebugMode)
                {
                    return;
                }

                if (eventName.Length >= 32)
                    eventName = eventName.Substring(0, 32);
                eventName = eventName.ToLower().Replace("ı", "i").Replace(".", "_");

                if (dictionary != null)
                {
                    var param = dictionary.Select(x =>
                    {
                        if (x.Key != null && x.Value != null)
                        {
                            if (x.Value is float)
                                return new Parameter(x.Key.ToLower(), (float)x.Value);
                            else if (x.Value is double)
                                return new Parameter(x.Key.ToLower(), (double)x.Value);
                            else if (x.Value is long)
                                return new Parameter(x.Key.ToLower(), (long)x.Value);
                            else if (x.Value is int)
                                return new Parameter(x.Key.ToLower(), (int)x.Value);
                            else if (x.Value is string)
                                return new Parameter(x.Key.ToLower(), !x.Value.ToString().Contains("_") ? Regex.Replace(x.Value.ToString(), @"\B[A-Z]", m => "_" + m.ToString()).ToLower().Replace("ı", "i") : x.Value.ToString().Replace("ı", "i"));
                            else
                                return new Parameter(x.Key.ToLower(), x.Value.ToString());
                        }
                        return null;
                    }).ToArray();

                    if (param != null)
                        FirebaseAnalytics.LogEvent(eventName.Replace("ı", "i"), param);
                    else
                        FirebaseAnalytics.LogEvent(eventName.Replace("ı", "i"));
                }
                else
                {
                    FirebaseAnalytics.LogEvent(eventName.Replace("ı", "i"));
                }
                Debug.Log("[Firebase] LogEvent: " + eventName);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
#endif
        }

        public static void LogEarnVirtualCurrency(int value, string name)
        {
#if USE_FIREBASE
            if (instance == null || AnalyticStatus != FirebaseStatus.Initialized)
            {
                if (instance == null)
                {
                    Debug.LogWarning("[Firebase] NULL");
                }
                else
                {
                    Debug.LogWarning("[Firebase] AnalyticStatus: " + AnalyticStatus);
                }
                return;
            }

            FirebaseAnalytics.LogEvent(
              FirebaseAnalytics.EventEarnVirtualCurrency,
              new Parameter[]
              {
            new Parameter(FirebaseAnalytics.ParameterValue, value),
            new Parameter(FirebaseAnalytics.ParameterVirtualCurrencyName, name)
              });
#endif
        }

        public static void LogSpendVirtualCurrency(int value, string name)
        {
#if USE_FIREBASE
            if (instance == null || AnalyticStatus != FirebaseStatus.Initialized)
            {
                if (instance == null)
                {
                    Debug.LogWarning("[Firebase] NULL");
                }
                else
                {
                    Debug.LogWarning("[Firebase] AnalyticStatus: " + AnalyticStatus);
                }
                return;
            }

            FirebaseAnalytics.LogEvent(
              FirebaseAnalytics.EventSpendVirtualCurrency,
                  new Parameter[] {
                    new Parameter(FirebaseAnalytics.ParameterValue, value),
                    new Parameter(FirebaseAnalytics.ParameterVirtualCurrencyName, name) });
#endif
        }

        public static void LogTutorial(int step, bool complete, int totalStep = 10)
        {
#if USE_FIREBASE
            if (step == 0 && !complete)
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventTutorialBegin);
            else if (complete)
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventTutorialComplete);


            FirebaseAnalytics.LogEvent("tutotial_" + step + "_" + totalStep);
#endif
        }

        public static void LogLevelUp(int level, string characterId)
        {
#if USE_FIREBASE
            if (instance == null || AnalyticStatus != FirebaseStatus.Initialized)
            {
                if (instance == null)
                {
                    Debug.LogWarning("[Firebase] NULL");
                }
                else
                {
                    Debug.LogWarning("[Firebase] AnalyticStatus: " + AnalyticStatus);
                }
                return;
            }

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelUp,
                new Parameter[] {
                    new Parameter(FirebaseAnalytics.ParameterCharacter, characterId),
                    new Parameter(FirebaseAnalytics.ParameterLevel, level) });
#endif
        }

        public static void LogLevelStart(int index, string name)
        {
#if USE_FIREBASE
            if (instance == null || AnalyticStatus != FirebaseStatus.Initialized)
            {
                if (instance == null)
                {
                    Debug.LogWarning("[Firebase] NULL");
                }
                else
                {
                    Debug.LogWarning("[Firebase] AnalyticStatus: " + AnalyticStatus);
                }
                return;
            }

            FirebaseAnalytics.LogEvent(
              FirebaseAnalytics.EventLevelStart,
                  new Parameter[] {
                    new Parameter(FirebaseAnalytics.ParameterLevel, index),
                    new Parameter(FirebaseAnalytics.ParameterLevelName, name) });

            FirebaseAnalytics.LogEvent($"level_start_{index}");
#endif
        }

        public static void LogLevelEnd(int index, string name)
        {
#if USE_FIREBASE
            if (instance == null || AnalyticStatus != FirebaseStatus.Initialized)
            {
                if (instance == null)
                {
                    Debug.LogWarning("[Firebase] NULL");
                }
                else
                {
                    Debug.LogWarning("[Firebase] AnalyticStatus: " + AnalyticStatus);
                }
                return;
            }

            FirebaseAnalytics.LogEvent(
              FirebaseAnalytics.EventLevelEnd,
                  new Parameter[] {
                    new Parameter(FirebaseAnalytics.ParameterLevel, index),
                    new Parameter(FirebaseAnalytics.ParameterLevelName, name) });

            FirebaseAnalytics.LogEvent($"level_end_{index}");
#endif
        }
        public static void LogLevelFalse(int index, string name)
        {
#if USE_FIREBASE
            if (instance == null || AnalyticStatus != FirebaseStatus.Initialized)
            {
                if (instance == null)
                {
                    Debug.LogWarning("[Firebase] NULL");
                }
                else
                {
                    Debug.LogWarning("[Firebase] AnalyticStatus: " + AnalyticStatus);
                }
                return;
            }

            FirebaseAnalytics.LogEvent(
              "EventLevelFalse",
                  new Parameter[] {
                    new Parameter(FirebaseAnalytics.ParameterLevel, index),
                    new Parameter(FirebaseAnalytics.ParameterLevelName, name) });

            FirebaseAnalytics.LogEvent($"level_false_{index}");
#endif
        }

#region FirebaseMessage
#if USE_FIREBASE_MESSAGE
        private static void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            FirebaseToken = token.Token;
#if UNITY_ANDROID && USE_APPSFLYER
            AppsFlyerSDK.AppsFlyer.updateServerUninstallToken(FirebaseToken);
#endif

            Debug.Log("[Firebase] On Token Received:" + "\n" + FirebaseToken);
        }

        public static void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            FirebaseMessage = e.Message;

            if (FirebaseMessage != null)
            {
                string logData = "FirebaseMessage: Received a new message";
                var notification = FirebaseMessage?.Notification;
                if (notification != null)
                {
                    logData += "\n" + "Title: " + notification.Title;
                    logData += "\n" + "Body: " + notification.Body;
                    var android = notification.Android;
                    if (android != null)
                        logData += "\n" + "ChannelId: " + android.ChannelId;
                }

                if (FirebaseMessage.From.Length > 0)
                    logData += "\n" + "From: " + e.Message.From;

                if (FirebaseMessage.Link != null)
                    logData += "\n" + "Link: " + e.Message.Link.ToString();

                if (FirebaseMessage.Data != null)
                {
                    logData += "\n" + "Data:";
                    foreach (KeyValuePair<string, string> i in e.Message.Data)
                        logData += "\n" + i.Key + ": " + i.Value;
                }
            }
        }
#endif
        #endregion

        public static bool IsConnected
        {
            get
            {
                switch (Application.internetReachability)
                {
                    case NetworkReachability.ReachableViaLocalAreaNetwork:
                        return true;
                    case NetworkReachability.ReachableViaCarrierDataNetwork:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public static void LogStatus()
        {
            string remoteData = "------------------";
            remoteData += "\n" + "Firebase Status: " + Status;
            remoteData += "\n" + "Analytic Status: " + AnalyticStatus;
            remoteData += "\n" + "Remote Status: " + RemoteStatus;
            remoteData += "\n" + "Message Status: " + MessageStatus;

#if USE_FIREBASE
            remoteData += "\n" + JsonUtility.ToJson(DataManager.GameConfig);
#endif
            Debug.Log(remoteData);
        }
    }
#endregion

    public enum FirebaseStatus
    {
        UnAvailable,
        Checking,
        Available,
        Initialing,
        Initialized,
        Getting,
        Completed,
        Faulted,
        Canceled,
        TimeOut,
        NoInternet,
        UnkownError,
        Success,
        Fetching,
        Pending
    }

    public enum IAPEvent
    {
        Show,
        Click,
        Success,
        Failed
    }
}