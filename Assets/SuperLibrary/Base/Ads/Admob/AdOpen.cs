#if USE_ADOPEN
using GoogleMobileAds.Api;
using MyBox;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Base.Ads.AdsManager;

namespace Base.Ads
{
    public class AdOpen : MonoBehaviour
    {
        public const string TAG = "ADOPEN ";

        public AdMediation mediation = AdMediation.APPOPEN;

        public ScreenOrientation screenOrientation = ScreenOrientation.AutoRotation;
        public bool useAdmobTestAdUnitId = false;

        protected static string adUnitId = "";

        protected static string placementName = "app_open";

        public static AdOpen instance = null;

        public static bool IsInit = false;

        protected int idTierIndex = 0;

        private void Awake()
        {
            if (instance != null)
                Destroy(gameObject);
            instance = this;
            DontDestroyOnLoad(gameObject);

            idTierIndex = 0;
        }

        private void Start()
        {
#if USE_ADOPEN
            MobileAds.SetiOSAppPauseOnBackground(true);
#endif
        }

        public static void DOInit(Action callback)
        {
            //CheckInstance();

            if (instance == null)
            {
                throw new Exception("AdOpen could not find the AdOpen object. Please ensure you have added the AdsManager Prefab to your scene.");
            }

            try
            {
#if USE_ADOPEN
                if (TestForceBackup && !string.IsNullOrEmpty(TestDeviceId))
                {
                    List<string> deviceIds = new List<string>() { AdRequest.TestDeviceSimulator, TestDeviceId };
                    RequestConfiguration requestConfiguration = new RequestConfiguration.Builder()
                       .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.True)
                       .SetTestDeviceIds(deviceIds).build();
                    MobileAds.SetRequestConfiguration(requestConfiguration);
                }

                MobileAds.Initialize((initStatus) =>
                {
                    Log(TAG + "Initialize: DONE " + initStatus.ToString());

                    if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
                        adUnitId = Settings.openAndroidUnitIdTier1?.Trim();
                    else
                        adUnitId = Settings.openAndroidUnitIdTier1.Trim();

                    Log(TAG + "AppOpenLoad: " + adUnitId + " TestForceBackup: " + TestForceBackup);

                    IsInit = true;
                    callback?.Invoke();
                });
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
#if USE_FIREBASE
                try
                {
                    FirebaseManager.LogEvent("ad_init_exception", new Dictionary<string, object>
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
        }

        protected static DateTime LastTimeLoaded = new DateTime();

#if USE_ADOPEN
        private AppOpenAd ad;
#endif
        public static AdEvent Status = AdEvent.Offer;

        public static bool IsReady
        {
            get
            {
#if USE_ADOPEN
                return instance?.ad != null && (instance?.ad.CanShowAd()).GetValueOrDefault(false);
#else
                return false;
#endif
            }
        }

        protected static void Load(bool reset, Action callback)
        {
#if USE_ADOPEN
            try
            {
                if (instance == null)
                {
                    LogError($"{TAG} Load -> instance NULL --> return");
                    return;
                }

                if (instance.ad != null)
                {
                    if ((DateTime.Now - LastTimeLoaded).TotalHours >= 3.5)
                    {
                        LogError($"{TAG} Load -> TimeOut --> return");
                        DestroyAd();
                    }
                    else
                    {
                        Log($"{TAG} Load -> is Loaded --> return");
                        callback?.Invoke();
                        return;
                    }
                }

                if (instance.useAdmobTestAdUnitId)
                {
                    if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
                        adUnitId = "ca-app-pub-3940256099942544/5662855259";
                    else
                        adUnitId = "ca-app-pub-3940256099942544/3419835294";
                }
                else
                {
                    if (reset)
                        instance.idTierIndex = 0;

                    instance.idTierIndex %= 3;

                    switch (instance.idTierIndex)
                    {
                        case 0:
                            adUnitId = Settings.openAndroidUnitIdTier1?.Trim();
                            break;
                        case 1:
                            adUnitId = Settings.openAndroidUnitIdTier2?.Trim();
                            break;
                        case 2:
                            adUnitId = Settings.openAndroidUnitIdTier3?.Trim();
                            break;
                    }
                }

                if (string.IsNullOrEmpty(adUnitId))
                {
                    LogError($"{TAG} Load ({instance.idTierIndex}) -> adUnitId NULL or EMPTY --> return");
                    return;
                }

                Log($"{TAG} Load ({instance.idTierIndex} | '{adUnitId}') -> Status: " + Status.ToString());

                if (Status == AdEvent.Load && reset)
                {
                    Log($"{TAG} Load: is loading --> return");
                    return;
                }

                Status = AdEvent.Load;

                var loadTime = DateTime.Now;

                SetStatus(AdType.AppOpen, AdEvent.Load, placementName, placementName, instance.mediation);

                AdRequest request = new AdRequest.Builder().Build();

                if (instance.screenOrientation == ScreenOrientation.AutoRotation)
                    instance.screenOrientation = Screen.orientation;

#if UNITY_EDITOR
                System.Threading.Thread.Sleep(1000);
#endif

                // Load an app open ad for portrait orientation
                AppOpenAd.Load(adUnitId, instance.screenOrientation, request, (ad, error) =>
                {
                    var logParams = ParamsBase(placementName, placementName, instance.mediation);
                    logParams.Add("load_time", (DateTime.Now - loadTime).TotalSeconds.ToString("#0.00"));

                    if (ad == null || error != null)
                    {
                        try
                        {
                            var errorDescription = $"{error.GetMessage()} ({error.GetCode()})";
                            LogError(TAG + "Load Failed Error: " + errorDescription);
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                        finally
                        {
                            instance.idTierIndex++;
                            if (instance.idTierIndex < 3 && !instance.useAdmobTestAdUnitId)
                            {
                                instance.DelayedAction(0.1f, () => Load(false, callback), true);
                            }
                            else
                            {
                                instance.idTierIndex = 0;
                                if (IsConnected)
                                {
                                    Status = AdEvent.LoadNotAvaiable;
                                    LogEvent(AdType.AppOpen, AdEvent.LoadNotAvaiable, logParams);
                                }
                                else
                                {
                                    Status = AdEvent.LoadNoInternet;
                                    LogEvent(AdType.AppOpen, AdEvent.LoadNoInternet, logParams);
                                }
                            }
                        }
                        return;
                    }

                    // App open ad is loaded.
                    instance.ad = ad;
                    LastTimeLoaded = DateTime.Now;
                    instance.idTierIndex = 0;
                    Status = AdEvent.LoadAvaiable;

                    SetStatus(AdType.AppOpen, AdEvent.LoadAvaiable, placementName, placementName, instance.mediation);

                    Log(TAG + "Load: isLoaded " + (DateTime.Now - loadTime).TotalSeconds.ToString("#0.00"));
                    callback?.Invoke();
                });
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                DestroyAd();
            }
#endif
        }

        public static void ShowOpenAdIfAvailable(string placement)
        {
#if USE_ADOPEN
            placementName = placement;

            if (!IsInit)
            {
                Log(TAG + "ShowAdIfAvailable: IsInit: " + IsInit + " --> return");
                return;
            }

            if (!IsTimeToShowAdOpen)
            {
                Log(TAG + "ShowAdIfAvailable: IsTimeToShowAdOpen: " + IsTimeToShowAdOpen + " --> return");
                return;
            }

            if (instance.ad != null && (DateTime.Now - LastTimeLoaded).TotalHours >= 3.5)
            {
                Log(TAG + "ShowAdIfAvailable: OutTime: " + (DateTime.Now - LastTimeLoaded).TotalHours.ToString("0.0") + " --> return");
                DestroyAd();
            }

            Status = AdEvent.ShowStart;
            SetStatus(AdType.AppOpen, AdEvent.ShowStart, placementName, placementName, instance.mediation);

            if (IsReady)
            {
                SetStatus(AdType.AppOpen, AdEvent.Show, placementName, placementName, instance.mediation);
                instance.Show(placement);
            }
            else
            {
                Log(TAG + "ShowAdIfAvailable: Status: " + Status.ToString() + " --> Load");
                Load(true, () =>
                {
                    if (placement.Equals("app_open"))
                        instance.Show(placement);
                });
            }
#endif
        }

        protected void Show(string placement = "app_open")
        {
#if USE_ADOPEN && !UNITY_EDITOR

            if (!IsReady)
            {
                LogError($"{TAG} Show -> IsReady: {IsReady} -> AdOpen is NULL or not READY");

                DestroyAd();
                Load(true, null);
                return;
            }

            try
            {
                placementName = placement;
                Status = AdEvent.Show;
                SetStatus(AdType.AppOpen, AdEvent.Show, placementName, placementName, instance.mediation);
                ad.OnAdFullScreenContentClosed += OnClose;
                ad.OnAdFullScreenContentFailed += OnShowFailed;
                ad.OnAdFullScreenContentOpened += HandleAdDidPresentFullScreenContent;
                ad.OnAdImpressionRecorded += HandleAdDidRecordImpression;
                ad.OnAdPaid += HandlePaidEvent;

                PauseApp(true);
                ad.Show();
            }
            catch (Exception ex)
            {
                LogException(ex);

                SetStatus(AdType.AppOpen, AdEvent.Exception, placementName, placementName, instance.mediation);

                DestroyAd();
                Load(true, null);
            }
#endif
        }

#if USE_ADOPEN
        protected static void OnClose()
        {
            Status = AdEvent.Close;
            PauseApp(false);
            Log(TAG + "Closed app open ad");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.

            LastTimeShowAd = DateTime.Now;
            SetStatus(AdType.AppOpen, AdEvent.Close, placementName, placementName, instance.mediation);

            DestroyAd();
            Load(true, null);

            if (DataManager.adInterOrRewardClicked)
                DataManager.adInterOrRewardClicked = false;
        }

        protected static void OnShowFailed(AdError err)
        {
            Status = AdEvent.ShowFailed;
            PauseApp(false);
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.

            if (err != null)
            {
                var errorMessage = err.GetMessage();
                LogError(TAG + "Failed to present the ad " + errorMessage);
            }

            SetStatus(AdType.AppOpen, AdEvent.ShowFailed, placementName, placementName, instance.mediation);

            DestroyAd();
            Load(true, null);
        }

        protected static void HandleAdDidPresentFullScreenContent()
        {
            Status = AdEvent.ShowSuccess;
            Debug.Log(TAG + "Displayed app open ad Success");

            SetStatus(AdType.AppOpen, AdEvent.ShowSuccess, placementName, placementName, instance.mediation);
        }

        protected static void HandleAdDidRecordImpression()
        {
            Debug.Log(TAG + "Recorded ad impression");
        }

        protected static void HandlePaidEvent(AdValue adValue)
        {
            if (adValue != null)
            {
                var currency = adValue.CurrencyCode;
                var revenue = adValue.Value;

                if (UnityMainThreadDispatcher.Exists())
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        Debug.LogFormat(TAG + "Received paid event. (currency: {0}, value: {1}", currency, revenue);
                        LogImpressionData(AdMediation.APPOPEN, adValue, placementName);
                    });
                }
            }
        }

        protected static void DestroyAd()
        {
            try
            {
                if (instance != null && instance.ad != null)
                {
                    instance.ad.OnAdFullScreenContentClosed -= OnClose;
                    instance.ad.OnAdFullScreenContentFailed -= OnShowFailed;
                    instance.ad.OnAdFullScreenContentOpened -= HandleAdDidPresentFullScreenContent;
                    instance.ad.OnAdImpressionRecorded -= HandleAdDidRecordImpression;
                    instance.ad.OnAdPaid -= HandlePaidEvent;
                    instance.ad.Destroy();
                    instance.ad = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
#endif
    }
}

