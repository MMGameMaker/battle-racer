using System;
using System.Collections;
using UnityEngine;
using static Base.Ads.AdsManager;
using MyBox;

namespace Base.Ads
{
    public class MaxHelper : AdsBase<MaxHelper>
    {
        protected static string SDK_KEY = "7PspscCcbGd6ohttmPcZTwGmZCihCW-Jwr7nSJN2a_9Mg0ERPs0tmGdKTK1gs__nr6XHQvK0vTNaTb1uR1mCIN";
        protected static string interId = "b781d4bcd8048ace";
        protected static string rewardId = "3366ea207eab1fb3";
        protected static string bannerId = "8c4303863f07341e";
        protected static string appOpenId = "bf1dbf0c9b8cfe8c";

        public bool fixBannerSmallSize = false;

        public static bool InterIsReady
        {
            get
            {
#if USE_MAX
                return MaxSdk.IsInterstitialReady(interId);
#else
                return false;
#endif
            }
        }

        public static bool RewardIsReady
        {
            get
            {
#if USE_MAX
                return MaxSdk.IsRewardedAdReady(rewardId);
#else
                return false;
#endif
            }
        }

        protected static MaxHelper instance = null;

        public void Awake()
        {
            instance = this;
            TAG = "MAX ";
        }

        public static IEnumerator Init(bool isDebug)
        {
            yield return null;
#if USE_MAX
            if (instance)
            {
                bool isInit = false;
                bool hasError = false;
                try
                {
                    SDK_KEY = Settings.maxSdkKey?.Trim();

                    if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
                    {
                        bannerId = Settings.maxIOSBannerUnitId?.Trim();
                        interId = Settings.maxIOSInterUnitId?.Trim();
                        rewardId = Settings.maxIOSRewaredUnitId?.Trim();
                        appOpenId = Settings.maxIOSOpenUnitId?.Trim();
                    }
                    else
                    {
                        bannerId = Settings.maxAndroidBannerUnitId?.Trim();
                        interId = Settings.maxAndroidInterUnitId?.Trim();
                        rewardId = Settings.maxAndroidRewaredUnitId?.Trim();
                        appOpenId = Settings.maxAndroidOpenUnitId?.Trim();
                    }

                    if (isDebug)
                        MaxSdk.ShowMediationDebugger();

                    MaxSdk.SetCreativeDebuggerEnabled(true);
                    MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
                    {
                        if (!string.IsNullOrEmpty(appOpenId))
                            instance.InitAdOpen();

                        instance.InterInit();
                        instance.RewardInit();
                        if (Settings.useBanner == AdMediation.MAX)
                            InitBanner(AdsManager.BannerPos);

                        Debug.Log(TAG + "Init: " + SDK_KEY + " - deviceUniqueIdentifier: " + SystemInfo.deviceUniqueIdentifier);
                        isInit = true;
                    };

                    MaxSdk.SetSdkKey(SDK_KEY);
                    MaxSdk.InitializeSdk();
                }
                catch (Exception ex)
                {
                    LogError(TAG + "Init Error: " + ex.Message);
#if USE_FIREBASE
                    try
                    {
                        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_init_exception", new Firebase.Analytics.Parameter[]
                        {
                            new Firebase.Analytics.Parameter("errorDescription", ex.Message),
                            new Firebase.Analytics.Parameter("mediation", TAG)
                        });
                    }
                    catch (Exception e)
                    {
                        LogError(TAG + "Log Firebase: " + e.Message);
                    }
#endif
                    hasError = true;
                }

                float timeOut = 5;
                for (int elapsed = 0; elapsed < timeOut; elapsed++)
                {
                    if (isInit || hasError)
                        break;
                    yield return new WaitForSeconds(1);
                }
            }
            else
            {
                LogError(TAG + "instance NULL");
            }
#endif
        }

        #region VIDEO REWARDED
        public override void RewardInit()
        {
#if USE_MAX
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += (s, o) => RewardOnReady(true);
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += (s, e) => RewardOnLoadFailed(e);
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (s, r, o) => RewardOnShowSuscess(o);
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += (s, e, o) => RewardOnShowFailed(e);
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (s, o) => RewardOnClose();
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (id, data) => LogImpressionData(AdMediation.MAX, data, id);

            IsInitReward = true;
            Log(TAG + "InitVideoRewarded " + rewardId);

            RewardLoad();
#endif
        }

        public override void RewardLoad()
        {
#if USE_MAX
            if (!RewardIsReady)
            {
                SetStatus(AdType.Reward, AdEvent.Load, rewardPlacementName, rewardItemName, mediation);
                MaxSdk.LoadRewardedAd(rewardId);
            }
#endif
        }

        public override void RewardOnReady(bool avaiable)
        {
#if USE_MAX
            SetStatus(AdType.Reward, AdEvent.LoadAvaiable, rewardPlacementName, rewardItemName, mediation);
            rewardCountTry = 0;
#endif
        }

        public static void ShowRewarded(Action<AdEvent, AdType> onSuccess, string placementName, string itemName = "")
        {
            if (instance)
            {
                try
                {

                    if (!IsInitReward) instance.RewardInit();

                    rewardPlacementName = placementName;
                    rewardItemName = itemName;

#if USE_MAX
                    if (RewardIsReady)
                    {
                        Log(TAG + "ShowRewarded -> Ready");
                        instance.RewardShow(onSuccess);
                        MaxSdk.ShowRewardedAd(rewardId, rewardPlacementName);
                    }
                    else
                    {
                        if (IsConnected)
                        {
                            LogError(TAG + "ShowRewarded -> NotAvailable");
                            SetStatus(AdType.Reward, AdEvent.ShowNotAvailable, rewardPlacementName, rewardItemName, instance.mediation);
                            onSuccess?.Invoke(AdEvent.ShowNotAvailable, AdType.Reward);
                        }
                        else
                        {
                            LogError(TAG + "ShowRewarded -> NotInternet");
                            SetStatus(AdType.Reward, AdEvent.ShowNoInternet, rewardPlacementName, rewardItemName, instance.mediation);
                            onSuccess?.Invoke(AdEvent.ShowNoInternet, AdType.Reward);
                        }
                        rewardCountTry = 0;
                        instance.RewardLoad();
                    }
#else
                    onSuccess?.Invoke(AdEvent.ShowFailed, AdType.Reward);
#endif
                }
                catch (Exception ex)
                {
                    instance.RewardOnShowFailed(null);
                    LogException(ex);
                }
            }
            else
            {
                instance.RewardOnShowFailed(null);
                LogError(TAG + "instance NULL");
            }
        }

        protected override void RewardOnLoadFailed(object obj)
        {
            var logParams = ParamsBase(interPlacemenetName, interItemName, mediation);
            try
            {
#if USE_MAX
                if (obj != null && obj is MaxSdkBase.ErrorInfo errorMax)
                {
                    logParams.Add("errorCode", errorMax.Code.ToString());
                    logParams.Add("errorDescription", errorMax.Message);
                    logParams.Add("error", errorMax.ToString());

                    if (errorMax.WaterfallInfo != null)
                    {
                        logParams.Add("waterfall", errorMax.WaterfallInfo.Name);
                        logParams.Add("latency", errorMax.WaterfallInfo.LatencyMillis.ToString());

                        Debug.Log("Waterfall Name: " + errorMax.WaterfallInfo.Name + " and Test Name: " + errorMax.WaterfallInfo.TestName);
                        Debug.Log("Waterfall latency was: " + errorMax.WaterfallInfo.LatencyMillis + " milliseconds");

                        foreach (var networkResponse in errorMax.WaterfallInfo.NetworkResponses)
                        {
                            Debug.Log("Network -> " + networkResponse.MediatedNetwork +
                                  "\n...latency: " + networkResponse.LatencyMillis + " milliseconds" +
                                  "\n...credentials: " + networkResponse.Credentials +
                                  "\n...error: " + networkResponse.Error);
                        }
                    }

                    LogError(TAG + "RewardOnLoadFailed Error: " + errorMax.Code.ToString() + " " + errorMax.Message + " re-trying in 15 seconds " + interCountTry + "/" + interCountMax);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            LogEvent(AdType.Reward, AdEvent.LoadNotAvaiable, logParams);

            base.RewardOnLoadFailed(obj);
        }

        protected override void RewardOnShowFailed(object obj)
        {
            var logParams = ParamsBase(interPlacemenetName, interItemName, mediation);
            try
            {
#if USE_MAX
                if (obj != null && obj is MaxSdkBase.ErrorInfo errorMax)
                {
                    logParams.Add("errorCode", errorMax.Code.ToString());
                    logParams.Add("errorDescription", errorMax.Message);
                    logParams.Add("error", errorMax.ToString());


                    LogError(TAG + "InterOnShowFailed Error: " + errorMax.ToString());

                    if (errorMax.Code == MaxSdkBase.ErrorCode.NoNetwork)
                    {
                        onRewardShowSuccess?.Invoke(AdEvent.ShowNoInternet, AdType.Inter);
                    }
                    else
                    {
                        onRewardShowSuccess?.Invoke(AdEvent.ShowFailed, AdType.Inter);
                    }
                }
                else
                {
                    onRewardShowSuccess?.Invoke(AdEvent.ShowFailed, AdType.Inter);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            LogEvent(AdType.Reward, AdEvent.ShowFailed, logParams);

            base.RewardOnShowFailed(obj);
        }
        #endregion

        #region INTERSTITIAL
        public override void InterInit()
        {
#if USE_MAX
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += (s, o) => InterOnReady();
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += (s, e) => InterOnLoadFailed(e);
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += (s, o) => InterOnShowSuscess();
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += (s, e, o) => InterOnShowFailed(e);
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += (s, o) => InterOnClose();
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (id, data) => LogImpressionData(AdMediation.MAX, data, id);
            MaxSdk.LoadInterstitial(interId);
            IsInitInter = true;
            Log(TAG + "InitInterstitial " + interId);
#endif
        }

        public override void InterLoad()
        {
#if USE_MAX
            if (!InterIsReady)
            {
                SetStatus(AdType.Inter, AdEvent.Load, interPlacemenetName, interItemName, mediation);
                MaxSdk.LoadInterstitial(interId);
            }
#endif
        }

        public override void InterOnReady()
        {
#if USE_MAX
            SetStatus(AdType.Inter, AdEvent.LoadAvaiable, interPlacemenetName, interItemName, mediation);
            interCountTry = 0;
#endif
        }

        public static void ShowInterstitial(Action<AdEvent, AdType> onSuccess, string placementName, string itemName)
        {
            if (instance)
            {
                try
                {
                    if (!IsInitInter) instance.InterInit();

                    interPlacemenetName = placementName;
                    interItemName = itemName;

#if USE_MAX
                    if (InterIsReady)
                    {
                        Log(TAG + "ShowInterstitial -> Ready");
                        instance.InterShow(onSuccess);
                        MaxSdk.ShowInterstitial(interId, interPlacemenetName);
                    }
                    else
                    {
                        if (IsConnected)
                        {
                            LogError(TAG + "ShowInterstitial -> NotAvailable");
                            SetStatus(AdType.Inter, AdEvent.ShowNotAvailable, interPlacemenetName, interItemName, instance.mediation);
                            onSuccess?.Invoke(AdEvent.ShowNotAvailable, AdType.Inter);
                        }
                        else
                        {
                            LogError(TAG + "ShowInterstitial -> NotInternet");
                            SetStatus(AdType.Inter, AdEvent.ShowNoInternet, interPlacemenetName, interItemName, instance.mediation);
                            onSuccess?.Invoke(AdEvent.ShowNoInternet, AdType.Inter);
                        }
                        interCountTry = 0;
                        instance.InterLoad();
                    }
#else
                    onSuccess?.Invoke(AdEvent.ShowFailed, AdType.Inter);
#endif
                }
                catch (Exception ex)
                {
                    instance.InterOnShowFailed(null);
                    LogException(ex);
                }
            }
            else
            {
                instance.InterOnShowFailed(null);
                LogError(TAG + "instance NULL");
            }
        }

        protected override void InterOnLoadFailed(object obj = null)
        {
            var logParams = ParamsBase(interPlacemenetName, interItemName, mediation);

            try
            {
#if USE_MAX
                if (obj != null && obj is MaxSdkBase.ErrorInfo errorMax)
                {
                    logParams.Add("errorCode", errorMax.Code.ToString());
                    logParams.Add("errorDescription", errorMax.Message);
                    logParams.Add("error", errorMax.ToString());
                    if (errorMax.WaterfallInfo != null)
                    {
                        logParams.Add("waterfall", errorMax.WaterfallInfo.Name);
                        logParams.Add("latency", errorMax.WaterfallInfo.LatencyMillis.ToString());

                        Debug.Log("Waterfall Name: " + errorMax.WaterfallInfo.Name + " and Test Name: " + errorMax.WaterfallInfo.TestName);
                        Debug.Log("Waterfall latency was: " + errorMax.WaterfallInfo.LatencyMillis + " milliseconds");

                        foreach (var networkResponse in errorMax.WaterfallInfo.NetworkResponses)
                        {
                            Debug.Log("Network -> " + networkResponse.MediatedNetwork +
                                  "\n...latency: " + networkResponse.LatencyMillis + " milliseconds" +
                                  "\n...credentials: " + networkResponse.Credentials +
                                  "\n...error: " + networkResponse.Error);
                        }
                    }

                    LogError(TAG + "InterOnLoadFailed Error: " + errorMax.Code.ToString() + " " + errorMax.Message + " re-trying in 15 seconds " + interCountTry + "/" + interCountMax);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            LogEvent(AdType.Inter, AdEvent.LoadNotAvaiable, logParams);

            base.InterOnLoadFailed(obj);
        }

        protected override void InterOnShowFailed(object obj = null)
        {
            var logParams = ParamsBase(interPlacemenetName, interItemName, mediation);

            try
            {
#if USE_MAX
                if (obj != null && obj is MaxSdkBase.ErrorInfo errorMax)
                {
                    logParams.Add("errorCode", errorMax.Code.ToString());
                    logParams.Add("errorDescription", errorMax.Message);
                    logParams.Add("error", errorMax.ToString());


                    LogError(TAG + "InterOnShowFailed Error: " + errorMax.ToString());
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            LogEvent("ad_" + AdType.Inter + "show_error", logParams);

            base.InterOnShowFailed(obj);
        }
        #endregion

        #region BANNER
        protected static bool BannerIsInit = false;
        protected static BannerPos bannerPos = BannerPos.BOTTOM;
        public static void InitBanner(BannerPos pos)
        {
            if (instance == null)
                return;

            if (string.IsNullOrEmpty(bannerId))
            {
                LogError(TAG + "Banner Id is NULL or EMPTY");
                return;
            }

            Log(TAG + "InitBanner " + bannerId + " IsNotShowAds: " + IsNotShowAds);

            if (IsNotShowAds)
                return;

#if USE_MAX
            DestroyBanner();

            bannerPos = pos;

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += (s, o) => OnBannerAdLoadedEvent();
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += (s, e) => OnBannerAdLoadFailedEvent();
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += (id, data) => LogImpressionData(AdMediation.MAX, data, id);

            BannerIsInit = true;

            LoadBanner();
#endif
        }

        public static void LoadBanner()
        {
            if (string.IsNullOrEmpty(bannerId))
            {
                LogError(TAG + "Banner Id is NULL or EMPTY");
                return;
            }

            try
            {
#if USE_MAX
                SetStatus(AdType.Banner, AdEvent.Load, "default", "default", instance.mediation);
                if (bannerPos == BannerPos.BOTTOM)
                    MaxSdk.CreateBanner(bannerId, MaxSdkBase.BannerPosition.BottomCenter);
                else if (bannerPos == BannerPos.TOP)
                    MaxSdk.CreateBanner(bannerId, MaxSdkBase.BannerPosition.TopCenter);

                if (instance != null && instance.fixBannerSmallSize)
                    MaxSdk.SetBannerWidth(bannerId, 320);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void DestroyBanner()
        {
            if (string.IsNullOrEmpty(bannerId))
            {
                LogError(TAG + "Banner Id is NULL or EMPTY");
                return;
            }

            try
            {
#if USE_MAX
                MaxSdkCallbacks.Banner.OnAdLoadedEvent -= (s, o) => OnBannerAdLoadedEvent();
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= (s, e) => OnBannerAdLoadFailedEvent();
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= (id, data) => LogImpressionData(AdMediation.MAX, data, id);
                MaxSdk.DestroyBanner(bannerId);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void HideBanner()
        {
#if USE_MAX
            MaxSdk.HideBanner(bannerId);
#endif
        }

        public static void ShowBanner()
        {
#if USE_MAX
            MaxSdk.ShowBanner(bannerId);
#endif
        }

        protected static bool tryLoadBanner = false;
        protected static void OnBannerAdLoadFailedEvent()
        {
#if USE_MAX
            if (instance != null)
                SetStatus(AdType.Banner, AdEvent.LoadNotAvaiable, "default", "default", instance.mediation);
            if (tryLoadBanner == false)
            {
                tryLoadBanner = true;
                LoadBanner();
            }
#endif
        }

        protected static void OnBannerAdLoadedEvent()
        {
#if USE_MAX
            MaxSdk.ShowBanner(bannerId);
            if (instance != null)
                SetStatus(AdType.Banner, AdEvent.ShowSuccess, "default", "default", instance.mediation);
#endif
        }
        #endregion

        #region OPEN
        public static AdEvent StatusOpen { private set; get; } = AdEvent.Offer;
        protected static int openCountMax = 1;
        protected static int openCountTry = 0;
        protected static string lastPlacement = "default";

        public void InitAdOpen()
        {
#if USE_MAXOPEN
            if (!string.IsNullOrEmpty(appOpenId))
            {
                Log(TAG + "InitAdOpen " + appOpenId);
                MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += AppOpen_OnAdHiddenEvent;
                MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += AppOpen_OnAdDisplayedEvent;
                MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += AppOpen_OnAdLoadedEvent;
                MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += AppOpen_OnAdLoadFailedEvent;
                MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += AppOpen_OnAdDisplayFailedEvent;
                AppOpenIsInit = true;
                AppOpenLoad();
            }
            else
            {
                Debug.LogError(TAG + " appOpenId NULL or EMPTY");
                StatusOpen = AdEvent.LoadNotAvaiable;
            }
#endif
        }

        public static bool AppOpenIsInit { private set; get; } = false;

        public static bool OpenIsReady
        {
            get
            {
#if USE_MAXOPEN
                if (instance != null)
                {
                    return MaxSdk.IsAppOpenAdReady(appOpenId);
                }
                return false;
#else
                return false;
#endif
            }
        }


        public static void ShowOpenAdIfAvailable(string placement = "default")
        {
            openCountTry = 0;
            lastPlacement = placement;

            Log(TAG + "ShowOpenAdIfAvailable-> " + placement);
#if USE_MAXOPEN
            StatusOpen = AdEvent.Show;
            SetStatus(AdType.AppOpen, AdEvent.Show, lastPlacement, "default", AdMediation.MAX);

            if (OpenIsReady)
            {
                StatusOpen = AdEvent.ShowStart;
                SetStatus(AdType.AppOpen, AdEvent.ShowStart, lastPlacement, "default", AdMediation.MAX);
                MaxSdk.ShowAppOpenAd(appOpenId);
            }
            else
            {
                if (IsConnected)
                {
                    StatusOpen = AdEvent.ShowNotAvailable;
                    SetStatus(AdType.AppOpen, AdEvent.ShowNotAvailable, lastPlacement, "default", AdMediation.MAX);

                    Log($"{TAG} ShowOpenAdIfAvailable -> Reload -> {StatusOpen}");
                    AppOpenLoad();
                }
                else
                {
                    StatusOpen = AdEvent.ShowNoInternet;
                    SetStatus(AdType.AppOpen, AdEvent.ShowNoInternet, lastPlacement, "default", AdMediation.MAX);
                }
            }
#endif
        }

#if USE_MAXOPEN
        public static void AppOpenLoad()
        {
            if (StatusOpen == AdEvent.Load && Time.time - instance.lastTimeOpenLoad < 300)
            {
                instance.lastTimeOpenLoad = 0;
                Log($"{TAG} AppOpenLoad -> call but something is loading before {Time.time - instance.lastTimeOpenLoad}s");
                return;
            }

            if (instance.loadCoroutine != null)
            {
                instance.StopCoroutine(instance.loadCoroutine);
                instance.loadCoroutine = null;
            }
            instance.lastTimeOpenLoad = Time.time;
            StatusOpen = AdEvent.Load;
            SetStatus(AdType.AppOpen, AdEvent.Load, lastPlacement, "default", AdMediation.MAX);


            Log($"{TAG} LoadAppOpenAd -> {appOpenId}");
            MaxSdk.LoadAppOpenAd(appOpenId);
        }

        private void AppOpen_OnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo info)
        {
            if (IsConnected)
            {
                StatusOpen = AdEvent.LoadNotAvaiable;
                SetStatus(AdType.AppOpen, AdEvent.LoadNotAvaiable, lastPlacement, "default", AdMediation.MAX);
            }
            else
            {
                StatusOpen = AdEvent.LoadNoInternet;
                SetStatus(AdType.AppOpen, AdEvent.LoadNoInternet, lastPlacement, "default", AdMediation.MAX);
            }

            if (openCountTry < openCountMax && lastPlacement.Equals("app_open"))
            {
                LogError($"{TAG} AppOpenOnLoadFailed Error: {info?.Message}");
                openCountTry++;
                loadCoroutine = this.DelayedAction(openCountTry, () => {
                    Log($"{TAG} OnAdLoadFailedEvent -> Reload -> AppOpenLoad: {openCountTry}/{openCountMax}");
                    AppOpenLoad();
                });
            }
        }
        Coroutine loadCoroutine;
        float lastTimeOpenLoad;

        private void AppOpen_OnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo arg2, MaxSdkBase.AdInfo arg3)
        {
            StatusOpen = AdEvent.ShowFailed;
            SetStatus(AdType.AppOpen, AdEvent.ShowFailed, lastPlacement, "default", AdMediation.MAX);

            Log($"{TAG} OnAdDisplayFailedEvent -> Reload -> AppOpenLoad");
            AppOpenLoad();
        }

        private void AppOpen_OnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo info)
        {
            openCountTry = 0;
            StatusOpen = AdEvent.LoadAvaiable;
            SetStatus(AdType.AppOpen, AdEvent.LoadAvaiable, lastPlacement, "default", AdMediation.MAX);

            Log($"{TAG} LoadAvaiable -> '{lastPlacement}' -> {appOpenId}");
            //if (lastPlacement.Equals("app_open"))
            //{
            //    StatusOpen = AdEvent.ShowStart;
            //    SetStatus(AdType.AppOpen, AdEvent.ShowStart, lastPlacement, "default", AdMediation.MAX);
            //    MaxSdk.ShowAppOpenAd(appOpenId);
            //}
        }

        private void AppOpen_OnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo info)
        {
            StatusOpen = AdEvent.Close;
            SetStatus(AdType.AppOpen, AdEvent.Close, lastPlacement, "default", AdMediation.MAX);
            LastTimeShowAd = DateTime.Now;

            Log($"{TAG} OnAdHiddenEvent -> Reload -> AppOpenLoad");
            AppOpenLoad();
        }

        private void AppOpen_OnAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo info)
        {
            StatusOpen = AdEvent.ShowSuccess;
            SetStatus(AdType.AppOpen, AdEvent.ShowSuccess, lastPlacement, "default", AdMediation.MAX);
        }
#endif
        #endregion
    }
}
