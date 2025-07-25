#if USE_APPSFLYER
using AppsFlyerSDK;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using static Base.Ads.AdsManager;

namespace Base.Ads
{
    public class IronHelper : AdsBase<IronHelper>
    {
        protected static string appKey = "UnSupport";

        public static IronHelper instance = null;

        public static bool InterIsReady
        {
            get
            {
#if USE_IRON
                return IronSource.Agent.isInterstitialReady();
#else
                return false;
#endif
            }
        }

        public static bool RewardIsReady
        {
            get
            {
#if USE_IRON
                return IronSource.Agent.isRewardedVideoAvailable();
#else
                return false;
#endif
            }
        }

        protected void Awake()
        {
            instance = this;
            TAG = "IRON ";
        }

        public static void Init(bool isDebug)
        {
#if USE_IRON
            if (instance)
            {
                try
                {
                    if (Application.platform == RuntimePlatform.Android)
                        appKey = Settings.ironAndroidAppKey?.Trim();
                    else if (Application.platform == RuntimePlatform.IPhonePlayer)
                        appKey = Settings.ironIOSAppKey?.Trim();
                    else
                        appKey = Settings.ironAndroidAppKey?.Trim();

                    interCountMax = Settings.autoRetryMax;
                    rewardCountMax = Settings.autoRetryMax;

                    if (isDebug)
                    {
                        IronSource.Agent.validateIntegration();
                        IronSource.Agent.setAdaptersDebug(isDebug);
                    }

                    IronSource.Agent.shouldTrackNetworkState(true);

                    IronSourceEvents.onSdkInitializationCompletedEvent += () =>
                    {
                        instance.InterInit();
                        instance.RewardInit();

                        if (Settings.useBanner == AdMediation.IRON)
                            InitBanner(AdsManager.BannerPos);

                        Debug.Log(TAG + "Init: " + appKey + " - deviceUniqueIdentifier: " + SystemInfo.deviceUniqueIdentifier);
                    };

                    IronSourceEvents.onImpressionDataReadyEvent += IronSourceEvents_onImpressionDataReadyEvent;

                    if (Settings.useBanner == AdMediation.IRON)
                    {
                        IronSource.Agent.init(appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
                    }
                    else
                        IronSource.Agent.init(appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL);


                    AdQualitySdkInit adQualitySdkInit = new AdQualitySdkInit();
                    ISAdQualityConfig adQualityConfig = new ISAdQualityConfig
                    {
                        AdQualityInitCallback = adQualitySdkInit
                    };
                    IronSourceAdQuality.Initialize(appKey, adQualityConfig);
                }
                catch (Exception ex)
                {
                    LogError(TAG + "Init: " + ex.Message);

                    FirebaseManager.LogEvent("ad_init_exception", new Dictionary<string, object>
                    {
                        {"errorDescription", ex.Message },
                        {"mediation", instance.mediation.ToString() }
                    });
                }
            }
            else
            {
                LogError(TAG + "instance NULL");
            }
#endif
        }

#if USE_IRON
        private static void IronSourceEvents_onImpressionDataReadyEvent(IronSourceImpressionData obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.adNetwork))
                SendEventAF(obj);
        }
        private static void ImpressionSuccessEvent(IronSourceImpressionData impressionData)
        {
            LogImpressionData(AdMediation.IRON, impressionData);
        }
        public static void SendEventAF(IronSourceImpressionData data)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add(AFAdRevenueEvent.COUNTRY, data.country);
            dic.Add(AFAdRevenueEvent.AD_UNIT, data.adUnit);
            dic.Add(AFAdRevenueEvent.AD_TYPE, data.instanceName);
            dic.Add(AFAdRevenueEvent.PLACEMENT, data.placement);
            dic.Add(AFAdRevenueEvent.ECPM_PAYLOAD, data.encryptedCPM);
            //dic.Add("custom", "foo");
            //dic.Add("custom_2", "bar");
            //dic.Add("af_quantity", "1");
            AppsFlyerAdRevenue.logAdRevenue(data.adNetwork, AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeIronSource, data.revenue.Value, "USD", dic);
        }
        protected void OnAdEvent(AdType type, string eventName, IronSourceAdInfo obj)
        {
#if USE_FIREBASE
            try
            {
                var adNetwork = obj.adNetwork;
                var paramBase = ParamsBase(rewardPlacementName, rewardItemName, mediation);
                paramBase.Add("ad_network", adNetwork);
                FirebaseManager.LogEvent("ad_" + type.ToString() + "_" + eventName, paramBase);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
#endif
        }
#endif

        #region VIDEO REWARDED
        public override void RewardInit()
        {
#if USE_IRON
            if (!IsInitReward)
            {
                IronSourceRewardedVideoEvents.onAdReadyEvent += (i) => RewardOnReady(true);
                IronSourceRewardedVideoEvents.onAdLoadFailedEvent += RewardOnLoadFailed;
                IronSourceRewardedVideoEvents.onAdRewardedEvent += (p, i) => RewardOnShowSuscess(p);
                IronSourceRewardedVideoEvents.onAdShowFailedEvent += (e, i) => RewardOnShowFailed(e);
                IronSourceRewardedVideoEvents.onAdClosedEvent += (i) => RewardOnClose();

                IronSourceRewardedVideoEvents.onAdOpenedEvent += (i) => OnAdEvent(AdType.Reward, "open", i);
                IronSourceRewardedVideoEvents.onAdReadyEvent += (i) => OnAdEvent(AdType.Reward, "ready", i);

                IronSourceRewardedVideoEvents.onAdClickedEvent +=(p,i)=> RewardOnClick();

                IronSource.Agent.loadRewardedVideo();
                IsInitReward = true;
                Log(TAG + "InitVideoRewarded " + appKey);
            }

            RewardLoad();
#endif
        }

        public override void RewardLoad()
        {
#if USE_IRON
            if (!RewardIsReady)
            {
                SetStatus(AdType.Reward, AdEvent.Load, rewardPlacementName, rewardItemName, mediation);
                IronSource.Agent.loadRewardedVideo();
            }
#endif
        }

        public override void RewardOnReady(bool avaiable)
        {
#if USE_IRON
            rewardCountTry = 0;
            Debug.Log(TAG + " RewardOnReady " + RewardIsReady);
#endif
        }

        public static void ShowRewarded(Action<AdEvent, AdType> onSuccess, string placementName, string itemName = "")
        {
            if (instance)
            {
                try
                {
#if USE_IRON
                    if (!IsInitReward) instance.RewardInit();

                    rewardPlacementName = placementName;
                    rewardItemName = itemName;

                    if (RewardIsReady)
                    {
                        Log(TAG + "ShowRewarded -> Ready");
                        instance.RewardShow(onSuccess);
                        IronSource.Agent.showRewardedVideo(rewardPlacementName);
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
            base.RewardOnLoadFailed(obj);

            var logParams = ParamsBase(interPlacemenetName, interItemName, mediation);

            try
            {
#if USE_IRON
                if (obj != null && obj is IronSourceError errorIron)
                {
                    logParams.Add("errorCode", errorIron.getErrorCode().ToString());
                    logParams.Add("errorDescription", errorIron.getDescription());
                    logParams.Add("error", errorIron.ToString());

                    LogError(TAG + "RewardOnLoadFailed Error: " + errorIron.getCode() + " " + errorIron.getDescription() + " re-trying in 15 seconds " + interCountTry + "/" + interCountMax);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            LogEvent(AdType.Reward, AdEvent.LoadNotAvaiable, logParams);
        }

        protected override void RewardOnShowFailed(object obj)
        {
            var logParams = ParamsBase(interPlacemenetName, interItemName, mediation);
            try
            {
#if USE_IRON
                if (obj != null && obj is IronSourceError errorIron)
                {
                    logParams.Add("errorCode", errorIron.getErrorCode().ToString());
                    logParams.Add("errorDescription", errorIron.getDescription());
                    logParams.Add("error", errorIron.ToString());

                    LogError(TAG + "InterOnShowFailed Error: " + errorIron.ToString());

                    if (errorIron.getCode() == 520)
                    {
                        onRewardShowSuccess?.Invoke(AdEvent.ShowNoInternet, AdType.Reward);
                    }
                    else
                    {
                        onRewardShowSuccess?.Invoke(AdEvent.ShowFailed, AdType.Reward);
                    }
                }
                else
                {
                    onRewardShowSuccess?.Invoke(AdEvent.ShowFailed, AdType.Reward);
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
#if USE_IRON
            if (!IsInitInter)
            {
                IronSourceInterstitialEvents.onAdReadyEvent += (info) => InterOnReady();
                IronSourceInterstitialEvents.onAdLoadFailedEvent += InterOnLoadFailed;
                IronSourceInterstitialEvents.onAdShowSucceededEvent += (info) => InterOnShowSuscess();
                IronSourceInterstitialEvents.onAdShowFailedEvent += (error, info) => InterOnShowFailed(error);
                IronSourceInterstitialEvents.onAdClosedEvent += (info) => InterOnClose();
                IronSourceInterstitialEvents.onAdClickedEvent += (info) => InterOnClicked();


                IronSourceInterstitialEvents.onAdOpenedEvent += (i) => OnAdEvent(AdType.Inter, "open", i);
                IronSourceInterstitialEvents.onAdReadyEvent += (i) => OnAdEvent(AdType.Inter, "ready", i);
                IsInitInter = true;
                Log(TAG + "InitInterstitial " + appKey);

            }
            InterLoad();
#endif
        }

        public override void InterLoad()
        {
#if USE_IRON
            if (!InterIsReady)
            {
                SetStatus(AdType.Inter, AdEvent.Load, interPlacemenetName, interItemName, mediation);
                IronSource.Agent.loadInterstitial();
            }
#endif
        }

        public override void InterOnReady()
        {
#if USE_IRON
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
                    interPlacemenetName = placementName;
                    interItemName = itemName;

                    if (!IsInitInter) instance.InterInit();

#if USE_IRON
                    if (IronSource.Agent.isInterstitialReady())
                    {
                        Log(TAG + "ShowInterstitial -> Ready");
                        instance.InterShow(onSuccess);
                        IronSource.Agent.showInterstitial(interPlacemenetName);
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
                    onSuccess?.Invoke(AdEvent.ShowFailed, AdType.Reward);
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
#if USE_IRON
                if (obj != null && obj is IronSourceError errorIron)
                {
                    logParams.Add("errorCode", errorIron.getErrorCode().ToString());
                    logParams.Add("errorDescription", errorIron.getDescription());
                    logParams.Add("error", errorIron.ToString());

                    LogError(TAG + "InterOnLoadFailed Error: " + errorIron.getCode() + " " + errorIron.getDescription() + " re-trying in 15 seconds " + interCountTry + "/" + interCountMax);
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
#if USE_IRON
                if (obj != null && obj is IronSourceError errorIron)
                {
                    logParams.Add("errorCode", errorIron.getErrorCode().ToString());
                    logParams.Add("errorDescription", errorIron.getDescription());
                    logParams.Add("error", errorIron.ToString());

                    LogError(TAG + "InterOnShowFailed Error: " + errorIron.ToString());
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
        protected static bool BannerIsInit = true;
        protected static BannerPos bannerPos = BannerPos.BOTTOM;

        public static void InitBanner(BannerPos pos)
        {
            if (instance == null)
                return;

            Log(TAG + "InitBanner " + appKey + " IsNotShowAds: " + IsNotShowAds);

            if (IsNotShowAds)
                return;
#if USE_IRON
            bannerPos = pos;

            IronSourceBannerEvents.onAdLoadedEvent += OnBannerAdLoadedEvent;
            IronSourceBannerEvents.onAdLoadFailedEvent += OnBannerAdLoadFailedEvent;

            BannerIsInit = true;

            LoadBanner();
#endif
        }

        public static void LoadBanner()
        {
            try
            {
#if USE_IRON
                SetStatus(AdType.Banner, AdEvent.Load, "default", "default", instance.mediation);

                if (bannerPos == BannerPos.BOTTOM)
                    IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
                else if (bannerPos == BannerPos.TOP)
                    IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.TOP);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void DestroyBanner()
        {
            try
            {
#if USE_IRON
                if (BannerIsInit)
                {
                    IronSourceBannerEvents.onAdLoadedEvent -= OnBannerAdLoadedEvent;
                    IronSourceBannerEvents.onAdLoadFailedEvent -= OnBannerAdLoadFailedEvent;
                    IronSource.Agent.destroyBanner();
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void HideBanner()
        {
#if USE_IRON
            IronSource.Agent.hideBanner();
#endif
        }

        public static void ShowBanner()
        {
#if USE_IRON
            IronSource.Agent.displayBanner();
#endif
        }

        protected static bool tryLoadBanner = false;
        protected static void OnBannerAdLoadFailedEvent(object obj)
        {
#if USE_IRON
            SetStatus(AdType.Banner, AdEvent.LoadNotAvaiable, "default", "default", instance.mediation);
            if (tryLoadBanner == false)
            {
                tryLoadBanner = true;
                LoadBanner();
            }
#endif
        }
#if USE_IRON
        private static void OnBannerAdLoadedEvent(IronSourceAdInfo info)
        {
            IronSource.Agent.displayBanner();
            SetStatus(AdType.Banner, AdEvent.ShowSuccess, "default", "default", instance.mediation);

    }
#endif
        #endregion
    }
}

#if USE_IRON
public class AdQualitySdkInit : ISAdQualityInitCallback
{

    public void adQualitySdkInitSuccess()
    {
        Debug.Log("unity: adQualitySdkInitSuccess");
    }
    public void adQualitySdkInitFailed(ISAdQualityInitError adQualitySdkInitError, string errorMessage)
    {
        Debug.Log("unity: adQualitySdkInitFailed " + adQualitySdkInitError + " message: " + errorMessage);
    }
}
#endif