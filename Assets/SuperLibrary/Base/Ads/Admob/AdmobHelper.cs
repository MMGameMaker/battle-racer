#if USE_ADMOB
using GoogleMobileAds.Api;
#endif
using System;
using UnityEngine;
using System.Collections;
using static Base.Ads.AdsManager;
using System.Collections.Generic;

namespace Base.Ads
{
    public class AdmobHelper : AdsBase<AdmobHelper>
    {
        protected static string appKey = "UnSupport";

        protected static AdmobHelper instance = null;

        protected void Awake()
        {
            instance = this;
            TAG = "ADMOB ";
        }

        public static bool IsInit = false;
        public static IEnumerator DOInit(bool isDebug, Action onInitDone = null, bool loadOnInitDone = false)
        {
#if USE_ADMOB
            if (instance == null)
            {
                throw new Exception("AdOpen could not find the AdOpen object. Please ensure you have added the AdsManager Prefab to your scene.");
            }

            bool isHasError = false;
            float timeOut = 0f;
            if (IsInit)
            {
                onInitDone?.Invoke();
                yield break;
            }

            try
            {
                MobileAds.SetiOSAppPauseOnBackground(true);

                if (Application.platform == RuntimePlatform.Android)
                    Debug.Log(TAG + "Init: " + Settings.adMobAndroidAppId);
                else
                    Debug.Log(TAG + "Init: " + Settings.adMobIOSAppId);

                MobileAds.DisableMediationInitialization();
                MobileAds.Initialize((initStatus) =>
                {
                    if (loadOnInitDone)
                    {
                        InitInter();
                        InitReward();
                        if (Settings.useBanner == AdMediation.ADMOD)
                            InitBanner(AdsManager.BannerPos);
                    }

                    IsInit = true;

                    Log(TAG + "Initialize: DONE " + initStatus.ToString());

                    onInitDone?.Invoke();
                });
            }
            catch (Exception ex)
            {
                isHasError = true;
                Debug.LogException(ex);
#if USE_FIREBASE
                try
                {
                    LogEvent("ad_init_exception", new Dictionary<string, object>
                    {
                        { "errorDescription", ex.Message },
                        { "mediation", TAG }
                    });
                }

                catch (Exception e)
                {
                    LogError(TAG + "Log Firebase: " + e.Message);
                }
#endif
            }

            while (IsInit == false && isHasError == false && timeOut < 3)
            {
                timeOut += Time.deltaTime;
                yield return null;
            }
#endif
            yield return null;
        }

        #region VIDEO REWARDED
#if USE_ADMOB
        protected static RewardedAd rewardedAd;
#endif


        public static bool RewardIsReady
        {
            get
            {
#if USE_ADMOB
                if (rewardedAd != null)
                    return rewardedAd.IsLoaded();
                return false;
#else
                return false;
#endif
            }
        }

        protected string adRewardUnitId = "";

        public static void InitReward()
        {
            instance?.RewardInit();
        }

        public override void RewardInit()
        {
            adRewardUnitId = "unexpected_platform";
            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
                adRewardUnitId = Settings.admobIOSRewaredUnitId;
            else
                adRewardUnitId = Settings.admobAndroidRewaredUnitId;

            if (TestForceBackup)
            {
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                    adRewardUnitId = "ca-app-pub-3940256099942544/1712485313";
                else
                    adRewardUnitId = "ca-app-pub-3940256099942544/5224354917";
            }
            Log(TAG + "InterLoad: " + adRewardUnitId + " TestForceBackup: " + TestForceBackup);

#if USE_ADMOB
            if (!IsInitReward)
            {
                rewardedAd = new RewardedAd(adRewardUnitId);

                rewardedAd.OnAdLoaded += (o, e) => RewardOnReady(true);
                rewardedAd.OnAdFailedToLoad += (o, e) => RewardOnLoadFailed(e);
                rewardedAd.OnUserEarnedReward += (o, e) => RewardOnShowSuscess(e);
                rewardedAd.OnAdFailedToShow += (o, e) => RewardOnShowFailed(e);
                rewardedAd.OnAdClosed += (o, e) => RewardOnClose();
                rewardedAd.OnPaidEvent += OnPaidEvent;

                IsInitReward = true;
            }

            RewardLoad();
#endif
        }

        public override void RewardLoad()
        {
            try
            {
#if USE_ADMOB
                if (!RewardIsReady)
                {
                    SetStatus(AdType.Reward, AdEvent.Load, rewardPlacementName, rewardItemName, mediation);
                    AdRequest request = new AdRequest.Builder().Build();
                    rewardedAd.LoadAd(request);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public override void RewardOnReady(bool avaiable)
        {
#if USE_ADMOB
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
#if USE_ADMOB
                    if (!IsInitReward) instance.RewardInit();

                    rewardPlacementName = placementName;
                    rewardItemName = itemName;

                    if (RewardIsReady)
                    {
                        Log(TAG + "ShowRewarded -> Ready");
                        instance.RewardShow(onSuccess);
                        rewardedAd.Show();
                    }
                    else
                    {
                        if (IsConnected)
                        {
                            LogError(TAG + "ShowRewarded -> NotAvailable");
                            SetStatus(AdType.Reward, AdEvent.ShowNotAvailable, placementName, itemName, instance.mediation);
                            onSuccess?.Invoke(AdEvent.ShowNotAvailable, AdType.Reward);
                        }
                        else
                        {
                            LogError(TAG + "ShowRewarded -> NotInternet");
                            SetStatus(AdType.Reward, AdEvent.ShowNoInternet, placementName, itemName, instance.mediation);
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

            try
            {
#if USE_ADMOB
                if (obj != null && obj is AdFailedToLoadEventArgs args && args.LoadAdError != null)
                {
                    var errorDescription = "";
                    if (DebugMode.IsDebugMode)
                    {
                        var error = args.LoadAdError;
                        errorDescription = error.GetMessage();

                        var adError = error.GetCause();
                        if (adError != null)
                            errorDescription += " " + adError.GetMessage() + " ";
                    }
                    LogError(TAG + "RewardOnLoadFailed Error: " + errorDescription + " re-trying in 15 seconds " + interCountTry + "/" + interCountMax);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            if (IsConnected)
                SetStatus(AdType.Reward, AdEvent.LoadNotAvaiable, rewardPlacementName, rewardItemName, mediation);
            else
                SetStatus(AdType.Reward, AdEvent.LoadNoInternet, rewardPlacementName, rewardItemName, mediation);

            base.RewardOnLoadFailed(obj);
        }

        protected override void RewardOnShowFailed(object obj)
        {
            try
            {
#if USE_ADMOB
                if (obj != null && obj is AdErrorEventArgs args && args.AdError != null)
                {
                    var errorDescription = "";
                    if (DebugMode.IsDebugMode)
                    {
                        var error = args.AdError;
                        errorDescription = error.GetMessage();
                    }
                    LogError(TAG + "RewardOnShowFailed Error: " + errorDescription + " re-trying in 15 seconds " + rewardCountTry + "/" + rewardCountMax);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            SetStatus(AdType.Reward, AdEvent.ShowFailed, rewardPlacementName, rewardItemName, mediation);

            base.RewardOnShowFailed(obj);
        }
        #endregion

        #region INTERSTITIAL
        public static AdEvent StatusAdInter = AdEvent.Offer;


        public static bool InterIsReady
        {
            get
            {
#if USE_ADMOB
                if (interstitial != null)
                    return interstitial.IsLoaded();
                return false;
#else
                return false;
#endif
            }
        }

#if USE_ADMOB
        protected static InterstitialAd interstitial;
#endif

        protected string adInterUnitId = "";
        public static void InitInter()
        {
            instance?.InterInit();
        }

        public override void InterInit()
        {
#if USE_ADMOB
            adInterUnitId = "unexpected_platform";

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
                adInterUnitId = Settings.admobIOSInterUnitId;
            else
                adInterUnitId = Settings.admobAndroidInterUnitId;

            if (TestForceBackup)
            {
                if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
                    adInterUnitId = "ca-app-pub-3940256099942544/4411468910";
                else
                    adInterUnitId = "ca-app-pub-3940256099942544/1033173712";
            }
            Log(TAG + "InterLoad: " + adInterUnitId + " TestForceBackup: " + TestForceBackup);

            if (!IsInitInter)
            {
                interstitial = new InterstitialAd(adInterUnitId);

                interstitial.OnAdLoaded += (o, e) => InterOnReady();
                interstitial.OnAdFailedToLoad += (o, e) => InterOnLoadFailed(e);
                interstitial.OnAdDidRecordImpression += (o, e) => InterOnShowSuscess();
                interstitial.OnAdFailedToShow += (o, e) => InterOnShowFailed(e);
                interstitial.OnAdClosed += (o, e) => InterOnClose();
                interstitial.OnPaidEvent += OnPaidEvent;

                IsInitInter = true;
            }

            InterLoad();
#endif  
        }

#if USE_ADMOB
        private void OnPaidEvent(object sender, AdValueEventArgs e)
        {
            Debug.Log(TAG + "OnPaidEvent");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (e != null && e.AdValue != null)
                {
                    LogImpressionData(AdMediation.ADMOD, e);
                }
            });
        }
#endif

        public override void InterLoad()
        {
            try
            {
#if USE_ADMOB
                if (!InterIsReady)
                {
                    StatusAdInter = AdEvent.Load;
                    SetStatus(AdType.Inter, AdEvent.Load, interPlacemenetName, interItemName, mediation);
                    AdRequest request = new AdRequest.Builder().Build();
                    interstitial.LoadAd(request);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public override void InterOnReady()
        {
#if USE_ADMOB
            StatusAdInter = AdEvent.LoadAvaiable;
            SetStatus(AdType.Inter, AdEvent.LoadAvaiable, interPlacemenetName, interItemName, mediation);
            interCountTry = 0;
#endif
        }

        public static IEnumerator WaitToShowInterstitial(float timeOut)
        {
            float loadAdsIn = 0;
            AdEvent adEvent = AdEvent.Load;

#if USE_ADMOB
            string adInterUnitId = "unexpected_platform";

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
                adInterUnitId = Settings.admobIOSInterUnitId;
            else
                adInterUnitId = Settings.admobAndroidInterUnitId;

            if (TestForceBackup)
            {
                if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
                    adInterUnitId = "ca-app-pub-3940256099942544/4411468910";
                else
                    adInterUnitId = "ca-app-pub-3940256099942544/1033173712";
            }

            if (string.IsNullOrEmpty(adInterUnitId))
            {
                LogError("WaitToShowInterstitial admobAndroidInterUnitId NULL or EMPTY");
                yield break;
            }

            Log(TAG + "InterLoad: " + adInterUnitId + " TestForceBackup: " + TestForceBackup);

            var startLoad = DateTime.Now;
            interstitial = new InterstitialAd(adInterUnitId);
            interstitial.OnAdLoaded += (o, e) =>
            {
                adEvent = AdEvent.LoadAvaiable;
                SetStatus(AdType.AppOpen, adEvent, "app_open_inter", "app_open", instance.mediation);
#if USE_FIREBASE
                FirebaseManager.LogEvent("app_open_inter_avaiable", new Dictionary<string, object>
                {
                    {"load_time", (DateTime.Now - startLoad).TotalSeconds.ToString("#0.0") },
                    {"mediation", TAG }
                });
#endif
            };
            interstitial.OnAdFailedToLoad += (o, args) =>
            {
                if (IsConnected)
                    adEvent = AdEvent.LoadNotAvaiable;
                else
                    adEvent = AdEvent.LoadNoInternet;
                SetStatus(AdType.AppOpen, adEvent, "app_open_inter", "app_open", instance.mediation);

#if USE_FIREBASE
                try
                {

                    if (DebugMode.IsDebugMode && args != null)
                    {
                        var error = args.LoadAdError;
                        var errorCode = error.GetCode();
                        var errorDescription = error.GetMessage();
                        LogError(TAG + "WaitToShowInterstitial: " + errorDescription);
                    }

                    FirebaseManager.LogEvent("app_open_inter_failed", new Dictionary<string, object>
                    {
                        {"load_time", (DateTime.Now - startLoad).TotalSeconds.ToString("#0.0") },
                        {"mediation", TAG }
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    LogError(TAG + "Log Firebase: " + ex.Message);
                }
#endif
            };
            interstitial.OnAdDidRecordImpression += (o, e) =>
            {
                adEvent = AdEvent.ShowSuccess;
                SetStatus(AdType.AppOpen, adEvent, "app_open_inter", "app_open", instance.mediation);
            };
            interstitial.OnAdFailedToShow += (o, e) =>
            {
                adEvent = AdEvent.ShowFailed;
                SetStatus(AdType.AppOpen, adEvent, "app_open_inter", "app_open", instance.mediation);
            };
            interstitial.OnAdClosed += (o, e) =>
            {
                adEvent = AdEvent.Close;
                SetStatus(AdType.AppOpen, adEvent, "app_open_inter", "app_open", instance.mediation);
            };

            AdRequest request = new AdRequest.Builder().Build();
            interstitial.LoadAd(request);

            while ((loadAdsIn < timeOut || adEvent == AdEvent.Load) && adEvent != AdEvent.LoadNoInternet && adEvent != AdEvent.LoadNotAvaiable)
            {
                loadAdsIn += Time.deltaTime;
                Log(TAG + "WaitToShowInterstitial: " + " is " + adEvent.ToString() + " load in " + loadAdsIn.ToString("#0.00"));
                yield return null;
            }

            float showAdsIn = 0;
            adEvent = AdEvent.Show;
            SetStatus(AdType.AppOpen, adEvent, "app_open_inter");

            if (interstitial.IsLoaded())
            {
                interstitial.Show();
                adEvent = AdEvent.ShowStart;
                SetStatus(AdType.AppOpen, adEvent, "app_open_inter");

                while (showAdsIn < timeOut || adEvent == AdEvent.ShowStart || adEvent == AdEvent.ShowSuccess)
                {
                    showAdsIn += Time.deltaTime;
                    Log(TAG + "WaitToShowInterstitial: " + " is " + adEvent.ToString() + " load in " + showAdsIn.ToString("#0.00"));
                    yield return null;
                }
            }
            else
            {
                if (IsConnected)
                    adEvent = AdEvent.ShowNotAvailable;
                else
                    adEvent = AdEvent.ShowNoInternet;
                SetStatus(AdType.AppOpen, adEvent, "app_open_inter");
            }
#endif
                yield return null;
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

#if USE_ADMOB
                    if (InterIsReady)
                    {
                        Log(TAG + "ShowInterstitial -> Ready");
                        instance.InterShow(onSuccess);
                        interstitial.Show();
                    }
                    else
                    {
                        if (IsConnected)
                        {
                            LogError(TAG + "ShowInterstitial -> Not Ready");
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
            try
            {
#if USE_ADMOB
                if (obj != null && obj is AdFailedToLoadEventArgs args && args.LoadAdError != null)
                {
                    var errorDescription = "";
                    if (DebugMode.IsDebugMode)
                    {
                        var error = args.LoadAdError;
                        errorDescription = error.GetMessage();

                        var adError = error.GetCause();
                        if (adError != null)
                            errorDescription += " " + adError.GetMessage() + " ";
                    }
                    LogError(TAG + "InterOnLoadFailed Error: " + errorDescription + " re-trying in 15 seconds " + interCountTry + "/" + interCountMax);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            if (IsConnected)
                StatusAdInter = AdEvent.LoadNotAvaiable;
            else
                StatusAdInter = AdEvent.LoadNoInternet;

            SetStatus(AdType.Inter, StatusAdInter, interPlacemenetName, interItemName, mediation);

            base.InterOnLoadFailed(obj);
        }

        protected override void InterOnShowFailed(object obj = null)
        {
            try
            {
#if USE_ADMOB
                if (obj != null && obj is AdErrorEventArgs args && args.AdError != null)
                {
                    var errorDescription = "";
                    if (DebugMode.IsDebugMode)
                    {
                        var error = args.AdError;
                        errorDescription = error.GetMessage();
                    }
                    LogError(TAG + "InterOnShowFailed Error: " + errorDescription + " re-trying in 15 seconds " + interCountTry + "/" + interCountMax);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            SetStatus(AdType.Inter, AdEvent.ShowFailed, interPlacemenetName, interItemName, mediation);

            base.InterOnShowFailed(obj);
        }
        #endregion

        #region BANNER

#if USE_ADMOB
        protected static BannerView bannerView = null;
        protected static BannerView bannerMediumRectangle = null;
        protected static bool bannerMediumRectangleIsLoaded = false;
        protected static bool bannerMediumRectangleIsRequestShow = false;
#endif

        public static string lastPlacementBannerMediumRectangle = "default";
        public static void ShowBannerMediumRectangle(string placement = "default", Vector2 customPosition = default)
        {
            ShowBannerMediumRectangle(AdBannerPosition.TopLeft, placement, customPosition);
        }

        public static void ShowBannerMediumRectangle(AdBannerPosition position, string placement = "default", Vector2 customPosition = default, string adRectangleUnitId = null)
        {
#if USE_ADMOB
            bannerMediumRectangleIsRequestShow = true;
            if (bannerMediumRectangle == null)
            {
                if (string.IsNullOrEmpty(placement))
                    placement = lastPlacementBannerMediumRectangle;

                if (string.IsNullOrEmpty(adRectangleUnitId))
                {
                    if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
                        adRectangleUnitId = Settings.admobIOSBannerRectangleUnitId;
                    else
                        adRectangleUnitId = Settings.admobAndroidBannerRectangleUnitId;
                }

                if (string.IsNullOrEmpty(adRectangleUnitId))
                {
                    Debug.Log("ShowBannerMediumRectangle: adRectangleUnitId IsNullOrEmpty!");
                    return;
                }
                else
                {
                    Debug.Log("ShowBannerMediumRectangle: adRectangleUnitId: " + adRectangleUnitId);
                }

                if (customPosition == default)
                    bannerMediumRectangle = new BannerView(adRectangleUnitId, AdSize.MediumRectangle, (AdPosition)position);
                else
                    bannerMediumRectangle = new BannerView(adRectangleUnitId, AdSize.MediumRectangle, (int)customPosition.x, (int)customPosition.y);

                bannerMediumRectangle.OnAdLoaded += (o, e) =>
                {
                    SetStatus(AdType.Banner, AdEvent.ShowSuccess, placement, "MediumRectangle", instance.mediation);
                    bannerMediumRectangleIsLoaded = true;
                    if (bannerMediumRectangleIsRequestShow)
                        bannerMediumRectangle.Show();
                };
                bannerMediumRectangle.OnAdFailedToLoad += (o, e) =>
                {
                    SetStatus(AdType.Banner, AdEvent.LoadNotAvaiable, placement, "MediumRectangle", instance.mediation);
                };
            }

            if (!bannerMediumRectangleIsLoaded)
            {
                AdRequest request = new AdRequest.Builder().Build();
                SetStatus(AdType.Banner, AdEvent.Load, placement, "MediumRectangle", instance.mediation);
                if (bannerMediumRectangle != null)
                    bannerMediumRectangle.LoadAd(request);
            }
            else
            {
                if (bannerMediumRectangle != null)
                    bannerMediumRectangle.Show();
            }
#endif
        }

        public static void HideBannerMediumRectangle()
        {
#if USE_ADMOB
            bannerMediumRectangleIsRequestShow = false;
            if (bannerMediumRectangle != null)
                bannerMediumRectangle.Hide();
#endif
        }

        protected static string bannerAdUnitId = "";
        /// <summary>
        /// Default banner position, set your position
        /// </summary>
        /// <param name="adUnitId">null is getting from Settings.admobAndroidBannerUnitId</param>
        /// <param name="position"></param>
        /// <param name="size">Banner = new Vector2(320, 50) - MediumRectangle = new Vector2(300, 250) See more: @https://developers.google.com/admob/unity/banner</param>
        /// <param name="loadOnInitDone">load banner on Init Done</param>
        /// <param name="showOnLoaded">show banner on Load Done</param>
        public static void InitBanner(BannerPos bannerPos)
        {
            if (instance == null)
                return;

            if (string.IsNullOrEmpty(bannerAdUnitId))
            {
                if (Application.platform == RuntimePlatform.Android)
                    bannerAdUnitId = Settings.admobAndroidBannerUnitId;
                else
                    bannerAdUnitId = Settings.admobIOSBannerUnitId;
            }

            Log(TAG + "InitBanner " + bannerAdUnitId + " IsNotShowAds: " + IsNotShowAds);

            if (IsNotShowAds)
                return;

#if USE_ADMOB
            if (bannerView == null && !string.IsNullOrEmpty(bannerAdUnitId))
            {
                LoadBanner();
            }
#endif
        }

        protected static BannerPos bannerPos = BannerPos.BOTTOM;
        public static void LoadBanner()
        {
#if USE_ADMOB
            DestroyBanner();

            if (bannerPos == BannerPos.BOTTOM)
                bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
            else
                bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Top);

            bannerView.OnAdLoaded += OnBannerAdLoadedEvent;
            bannerView.OnAdFailedToLoad += OnBannerAdLoadFailedEvent;

            AdRequest request = new AdRequest.Builder().Build();
            SetStatus(AdType.Banner, AdEvent.Load, "default", "default", instance.mediation);
            bannerView.LoadAd(request);
#endif
        }

        public static void DestroyBanner()
        {
#if USE_ADMOB
            if (bannerView != null)
            {
                bannerView.OnAdLoaded -= OnBannerAdLoadedEvent;
                bannerView.OnAdFailedToLoad -= OnBannerAdLoadFailedEvent;
                bannerView.Destroy();
                bannerView = null;
            }
#endif
        }

        public static void HideBanner()
        {
#if USE_ADMOB
            if (bannerView != null)
                bannerView.Hide();
#endif
        }

        public static void ShowBanner()
        {
#if USE_ADMOB
            if (bannerView != null)
                bannerView.Show();
#endif
        }

        protected static bool tryLoadBanner = false;
        protected static void OnBannerAdLoadFailedEvent(object sender, object obj)
        {
            try
            {
#if USE_ADMOB
                if (obj != null && obj is AdFailedToLoadEventArgs args && args.LoadAdError != null)
                {
                    var error = args.LoadAdError;
                    var errorCode = error.GetCode();
                    var errorDescription = error.GetMessage();

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        var logParams = ParamsBase(interPlacemenetName, interItemName, instance.mediation);
                        logParams.Add("errorCode", errorCode);
                        if (!string.IsNullOrEmpty(errorDescription))
                        {
                            logParams.Add("errorDescription", errorDescription);
                            LogError(TAG + "BannerAdLoadFailedEvent Error: " + errorDescription);
                        }
                        SetStatus(AdType.Banner, AdEvent.LoadNotAvaiable, "default", "default", instance.mediation);
                    });
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            if (tryLoadBanner == false)
            {
                tryLoadBanner = true;
                LoadBanner();
            }
        }

        private static void OnBannerAdLoadedEvent(object sender, EventArgs e)
        {
#if USE_ADMOB
            if (bannerView != null)
                bannerView.Show();
            SetStatus(AdType.Banner, AdEvent.ShowSuccess, "default", "default", instance.mediation);
#endif
        }
        #endregion
    }

    public enum AdBannerPosition
    {
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }
}
