#if USE_FIREBASE
using Firebase.Analytics;
#endif
#if USE_ADJUST
using com.adjust.sdk;
#endif
#if USE_GADSME
using Gadsme;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Globalization;
using Cysharp.Threading.Tasks;


namespace Base.Ads
{
    public class AdsManager : MonoBehaviour
    {
        [SerializeField]
        private List<AdMediation> adsNetWork = new List<AdMediation> { AdMediation.IRON };
        public static List<AdMediation> AdsNetwork { get => instance?.adsNetWork; }
        public static AdMediation DefaultMediation { get { return AdsNetwork.FirstOrDefault(); } }

        [Tooltip("Interstitial NOT READY --> switch to VideoReward")]
        public static bool interToReward = false;
        [Tooltip("VideoReward NOT READY --> switch to Interstitial")]
        public static bool rewardToInter = false;

        protected static AdsSettings adsSettings = null;
        public static AdsSettings Settings
        {
            get
            {
                if (adsSettings == null)
                    adsSettings = Resources.Load<AdsSettings>(AdsSettings.fileName);
                if (adsSettings == null)
                    throw new Exception("[AdsManager]" + "AdsSettings NULL --> Please creat from Base SDK/Ads Settings");
                else
                    return adsSettings;
            }
            private set => adsSettings = value;
        }
        [SerializeField]
        protected List<GameObject> bannerGameObjectList = null;

        private static bool IsDebug => DebugMode.IsDebugMode;
        public static bool isLowMemory;

        [SerializeField] AdOpen Adopen;
        [SerializeField] AdCollap AdCollap;

        [Header("Options")]
        [SerializeField]
        protected Button interstitialButton = null;
        [SerializeField]
        protected Button videoRewarded = null;
        [SerializeField]
        protected Toggle removeAdsToggle = null;

        [Header("Backup")]
        [SerializeField]
        protected bool loadBackupOnInitDone = false;
        public static bool LoadBackupOnInitDone => instance.loadBackupOnInitDone;
        [SerializeField]
        protected bool testForceBackup = false;
        public static bool TestForceBackup => instance.testForceBackup;
        [SerializeField]
        protected string testDeviceId = "";
        public static string TestDeviceId => instance.testDeviceId;

        [Header("Banner Safe Area")]
        [SerializeField]
        protected float bannerHeight = 68f;
        protected RectTransform parentTransform;
        public static BannerPos BannerPos => Settings.bannerPosition;
        public static List<AdsBannerArea> AdsBannerAreaList = new List<AdsBannerArea>();

        private static int totalInterSuccess { get; set; }
        private static int totalRewardSuccess { get; set; }
        private static int totalSuccess { get; set; }

        public static bool IsInit { get; private set; }

        private static DateTime lastTimeShowAd = DateTime.Now.AddSeconds(-600);
        public static DateTime LastTimeShowAd { get => lastTimeShowAd; set => lastTimeShowAd = value; }

        private static DateTime lastTimeShowRv = DateTime.Now.AddSeconds(-600);
        public static DateTime LastTimeShowRv { get => lastTimeShowRv; set => lastTimeShowRv = value; }

        protected static UserData userData = new UserData();
        public static UserData UserData
        {
            get
            {
                if (DataManager.UserData != null)
                    userData = DataManager.UserData;
                return userData;
            }
        }

        protected static GameConfig gameConfig = new GameConfig { timePlayToShowAd = 15, timePlayToShowAdReduce = 0, timePlayToShowOpenAd = 15, timeToWaitOpenAd = 5 };
        public static GameConfig GameConfig
        {
            get
            {
                if (DataManager.GameConfig != null)
                    gameConfig = DataManager.GameConfig;
                return gameConfig;
            }
        }

        public static bool RewardIsReady
        {
            get
            {
                bool isReady = false;
#if USE_IRON || USE_MAX || USE_ADMOB
                if (IronHelper.RewardIsReady)
                    isReady = true;
                else if (MaxHelper.RewardIsReady)
                    isReady = true;
                else if (AdmobHelper.RewardIsReady)
                    isReady = true;
#else
                isReady = true;
#endif
                return isReady;
            }
        }

        public static bool InterIsReady
        {
            get
            {
                bool isReady = false;
#if USE_IRON || USE_MAX || USE_ADMOB
                if (IronHelper.InterIsReady)
                    isReady = true;
                else if (MaxHelper.InterIsReady)
                    isReady = true;
                else if (AdmobHelper.InterIsReady)
                    isReady = true;
#else
                isReady = true;
#endif
                return isReady;
            }
        }

        protected static bool isTimeToShowAds = true;
        protected static bool isTimeDoneRv = true;
        /// <summary>
        /// Check time play more than game config to show ads
        /// </summary>
        public static bool IsTimeToShowAds
        {
            get
            {
                if (UserData != null && GameConfig != null)
                {
                    isTimeToShowAds = false;
                    isTimeDoneRv = false;

                    if (IsNotShowAds)
                    {
                        Log("isVIP: " + UserData.isVIP + " isRemovedAds: " + UserData.isRemovedAds);
                        return isTimeToShowAds && isTimeDoneRv;
                    }

                    float timePlayToShowAds = GameConfig.timePlayToShowAd;
                    float timeCappingInterAfterRv = GameConfig.timeCappingInterAfterRv;
                    float totalTimePlay = (float)(DateTime.Now - LastTimeShowAd).TotalSeconds;
                    float totalTimeRv = (float)(DateTime.Now - LastTimeShowRv).TotalSeconds;
                    isTimeToShowAds = totalTimePlay >= timePlayToShowAds;
                    isTimeDoneRv = totalTimeRv >= timeCappingInterAfterRv;

                    Log("[AdsManager] isTimeToShowAds:  " + isTimeToShowAds + " - totalTimePlay: " + totalTimePlay.ToString("#0.0") + " - timePlayToShowAds: " + timePlayToShowAds + " - totalSuccess: " + totalSuccess);
                }
                return isTimeToShowAds && isTimeDoneRv;
            }
        }

        protected static bool isTimeToShowAdOpen = true;
        /// <summary>
        /// Check time play more than game config to show ads
        /// </summary>
        public static bool IsTimeToShowAdOpen
        {
            get
            {
                if (UserData != null && GameConfig != null)
                {
                    isTimeToShowAdOpen = false;

                    if (IsNotShowAds)
                    {
                        Log("[AdsManager] isVIP: " + UserData.isVIP + " isRemovedAds: " + UserData.isRemovedAds);
                        return isTimeToShowAdOpen;
                    }

                    if ((LastAdType == AdType.Reward || LastAdType == AdType.Inter || LastAdType == AdType.AppOpen)
                        && (LastAdEvent == AdEvent.ShowStart || LastAdEvent == AdEvent.ShowSuccess || LastAdEvent == AdEvent.Close))
                    {
                        Debug.LogWarning("[AdsManager] is " + LastAdType + " is " + LastAdEvent + " --> return");
                        return isTimeToShowAdOpen;
                    }

                    float timePlayToShowAds = GameConfig.timePlayToShowOpenAd;
                    float totalTimePlay = (float)(DateTime.Now - LastTimeShowAd).TotalSeconds;
                    isTimeToShowAdOpen = totalTimePlay >= timePlayToShowAds;

                    Log("[AdsManager] IsTimeToShowAdOpen:  " + isTimeToShowAdOpen + " - totalTimePlay: " + totalTimePlay.ToString("#0.0") + " - timePlayToShowAds: " + timePlayToShowAds);
                }
                return isTimeToShowAdOpen;
            }
        }

        public delegate void AdsDelegate(AdType currentType, AdEvent currentEvent, string currentPlacement, string currentItem);
        public static AdsDelegate OnStateChanged;

        protected static AdsManager instance = null;
        public static bool isMobileAdsInitialize = false;

        private void Awake()
        {
            try
            {
                if (instance != null)
                    Destroy(gameObject);
                if (instance == null)
                    instance = this;
                DontDestroyOnLoad(gameObject);

                isLowMemory = false;
                isMobileAdsInitialize = false;
                //Application.lowMemory += Application_lowMemory;
            }
            catch (Exception ex)
            {
                Debug.LogError("[AdsManager] Exception: " + ex.Message);
            }
        }

        private void Application_lowMemory()
        {
            isLowMemory = true;
        }

        public static void CheckInstance()
        {
            if (instance == null)
            {
                var prefab = Resources.Load<AdsManager>("AdsManager");
                if (prefab != null)
                    instance = Instantiate(prefab);
            }

            if (instance == null)
                throw new Exception("AdsManager could not find the AdsManager object. Please ensure you have added the Base/Plugins/Resources/AdsManager Prefab to your scene.");
        }

        private void Start()
        {
            interstitialButton?.onClick.RemoveAllListeners();
            interstitialButton?.onClick.AddListener(TestInterstitial);


            videoRewarded?.onClick.RemoveAllListeners();
            videoRewarded?.onClick.AddListener(TestVideoReward);


            removeAdsToggle?.onValueChanged.RemoveAllListeners();
            removeAdsToggle?.onValueChanged.AddListener((isOn) =>
            {
                //UIToast.ShowNotice("Not show ads is " + isOn);
                UpdateBannerArea();
            });

            UserData.OnVIPChanged += OnVIPChanged;
            UserData.OnRemovedAdsChanged += OnRemovedAdsChanged;
        }
        private void OnEnable()
        {
            
        }

#if USE_IRON
        private void IronSourceEvents_onImpressionDataReadyEvent(IronSourceImpressionData obj)
        {

            Log("unity - scrip: I got ImpressionDataReadyEvent ToString(): " + obj.ToString());
            Log("unity - scrip: I got ImpressionDataReadyEvent allData: " + obj.allData);

            if (obj != null && !string.IsNullOrEmpty(obj.adNetwork))
            {
                //FirebaseAnalytics
            }
        }
#endif

        private void IronSourceEvents_onSdkInitializationCompletedEvent()
        {
        }


        private void OnDestroy()
        {
            UserData.OnVIPChanged -= OnVIPChanged;
            UserData.OnRemovedAdsChanged -= OnRemovedAdsChanged;
        }

        private void OnVIPChanged(bool isVip)
        {
            Log("UserData_OnVIPChanged");
            UpdateBannerArea();
        }

        private void OnRemovedAdsChanged(bool isRemoveAds)
        {
            Log("OnRemovedAdsChanged");
            UpdateBannerArea();
        }

        public static IEnumerator DOInit()
        {
            CheckInstance();

            if (IsInit)
                yield break;

            yield return new WaitForEndOfFrame();

            yield return ATTHelper.DOCheckATT();

#if USE_ADOPEN || USE_ADCOLLAP
            yield return UMP.DOGatherConsent(() => {
                MobileAds.Initialize((initStatus) =>
                {
                    isMobileAdsInitialize = true;
                    ShowAdOpen();
                });
            });
#endif


#if !USE_MAX && !USE_IRON && !USE_ADMOB && !USE_UNITY
            if (!instance)
            {
                //throw new Exception("AdsManager could not find the AdsManager object. Please ensure you have added the AdsManager Prefab to your scene.");
                yield break;
            }
#else

            if (DefaultMediation == AdMediation.MAX)
            {
                Log("[AdsManager] Init " + DefaultMediation);
                yield return MaxHelper.Init(IsDebug);
                yield return new WaitForSeconds(0.25f);

                //AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
            }
            else if (DefaultMediation == AdMediation.IRON)
            {
                Log("[AdsManager] Init " + DefaultMediation);
                IronHelper.Init(IsDebug);
                yield return new WaitForSeconds(0.25f);
            }
            else if (DefaultMediation == AdMediation.ADMOD)
            {
                Log("[AdsManager] Init " + DefaultMediation);
                yield return AdmobHelper.DOInit(IsDebug, () => Debug.Log("AdmobHelper Init DONE --> AUTO LOAD"), true);
                yield return new WaitForSeconds(0.25f);
            }
            else
            {
                LogError("[AdsManager] Init " + DefaultMediation + " NOT SUPPORT");
            }

            UpdateBannerArea();

            //yield return AdvertyHelper.DOInit();
#endif
            IsInit = true;
        }

        /// <summary>
        /// Should call show then FirebaseManager initiated. Flow game monetization on Gameover
        /// </summary>
        /// <returns></returns>
        public static void ShowAdOpen()
        {
#if UNITY_IOS && (USE_ADOPEN || USE_MAXOPEN) && !UNITY_EDITOR
                while (Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                    return;
#endif
#if USE_ADOPEN
            Log(AdOpen.TAG + "WaitToShow: " + AdOpen.Status.ToString() + " IsReady " + AdOpen.IsReady);
            AdOpen.ShowOpenAdIfAvailable("app_open");
#endif
#if USE_MAXOPEN
            Log(MaxHelper.TAG + "WaitToShow: Status " + MaxHelper.StatusOpen + " IsReady " + MaxHelper.OpenIsReady);
            MaxHelper.ShowOpenAdIfAvailable("app_open");
#endif
        }
        public static IEnumerator LoadAdCollap(Action<AdEvent, AdType> load)
        {
            if (GameConfig != null)
            {
                yield return instance.AdCollap.WaitUntilLoaded(load);
            }
        }

        public void TestInterstitial()
        {
            LastTimeShowAd = DateTime.Now.AddSeconds(-180);

            ShowInterstitial((s, a) =>
            {
                LogWarning("Test Interstitial: " + s);
            }, "default");
        }

        public void ShowInterstitial(string item)
        {
            if (GameConfig.forceInterEverywhere)
            {
                ShowInterstitial((s, a) =>
                {

                }, "forceInterEverywhere", item);
            }
        }

        /// <summary>
        /// Befor call should show loading then check status to do something. Flow game monetization on Gameover
        /// <para/>
        /// A.on RESULT SCREEN: 
        /// if (timePlayInGame >= timePlayToShowAds)
        /// {
        ///     ShowInterstitial((status) =>
        ///     {
        ///         do something
        ///     }
        ///}
        ///<para/>
        ///B.on CONTINUE SCREEN: 
        ///if (userClickRebornByAds) 
        ///     => reset timePlayInGame
        /// else 
        ///     => flow on RESULT SCREEN
        /// </summary>
        public static void ShowInterstitial(Action<AdEvent, AdType> onSuccess, string placementName, string itemName = "default", bool forceToShow = false)
        {
            try
            {
                if (instance == null)
                {
                    Debug.LogError("[AdsManager] could not find the AdsManager object. Please ensure you have added the AdsManager Prefab to your scene.");
                    onSuccess?.Invoke(AdEvent.ShowNotAvailable, AdType.Inter);
                    return;
                }

                instance.StartCoroutine(DOInit());

                interToReward = GameConfig.forceInterToReward;

                Debug.Log(string.Format("[AdsManager] ShowInterstitial IRON {0} MAX {1} ADMOB {2} LastAdEvent {3} IsTime {4} TotalSuccess {5} ", IronHelper.InterIsReady, MaxHelper.InterIsReady, AdmobHelper.InterIsReady, LastAdEvent, IsTimeToShowAds, totalSuccess));

                //if (DataManager.IsTutorial)
                //{
                //    Debug.LogWarning("[AdsManager] Tutorial --> return");
                //    return;
                //}

                if (!IsTimeToShowAds && !forceToShow || IsNotShowAds)
                {
                    onSuccess?.Invoke(AdEvent.ShowNotTime, AdType.Inter);
                    return;
                }

                if (Settings.ratioInterPerReward > 0 && (totalInterSuccess + 0.001) / (totalRewardSuccess + 1) >= Settings.ratioInterPerReward)
                {
                    Log(string.Format("[AdsManager] ShowInterstitial RatioInterPerReward {0}/{1}", (totalInterSuccess + 0.001) / (totalRewardSuccess + 1), Settings.ratioInterPerReward));
                    ShowVideoReward(onSuccess, placementName, itemName);
                    return;
                }

                SetStatus(AdType.Inter, AdEvent.Show, placementName, itemName, DefaultMediation);

#if UNITY_EDITOR && !USE_ADMOB
                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
                {
                    onSuccess?.Invoke(AdEvent.ShowSuccess, AdType.Inter);
                    SetStatus(AdType.Inter, AdEvent.ShowSuccess, placementName, itemName, DefaultMediation);
                    return;
                }
#endif

#if !USE_MAX && !USE_IRON && !USE_ADMOB && !USE_UNITY
                onSuccess?.Invoke(AdEvent.ShowSuccess, AdType.Inter);
                SetStatus(AdType.Inter, AdEvent.ShowSuccess, placementName, itemName, DefaultMediation);
                return;
#else

                if (string.IsNullOrEmpty(placementName))
                    placementName = "default";

                if (string.IsNullOrEmpty(itemName))
                    itemName = "default";

                if (DefaultMediation == AdMediation.IRON)
                {
                    IronHelper.ShowInterstitial(onSuccess, placementName, itemName);
                    //CountInterToOfferNoAds();
                }
                else if (DefaultMediation == AdMediation.MAX)
                {
                    MaxHelper.ShowInterstitial(onSuccess, placementName, itemName);
                    //CountInterToOfferNoAds();
                }
                else
                {
                    LogWarning("[AdsManager] ShowInterstitial Show --> NOT AVAIABLE ------------------------------");
                    onSuccess?.Invoke(AdEvent.ShowNotAvailable, AdType.Inter);
                    SetStatus(AdType.Inter, AdEvent.ShowNotAvailable, placementName, itemName, DefaultMediation);
                }
#endif
            }
            catch (Exception ex)
            {
                LogException(ex);
                onSuccess?.Invoke(AdEvent.ShowFailed, AdType.Inter);
            }
        }

        public void TestVideoReward()
        {
            LastTimeShowAd = DateTime.Now.AddSeconds(-60);

            ShowVideoReward((s, a) =>
            {
                LogWarning("Test VideoReward: " + s);
            }, "default");
        }

        /// <summary>
        /// Befor call should show loading then check status to do something. Flow game monetization on Gameover
        /// <para/>
        /// LOGIC:
        /// ShowVideoReward((status) =>
        /// {
        ///     if (status == AdEvent.ShowSuccess)
        ///     {
        ///         resetTimePlayInGame();
        ///         do something
        ///     }
        ///     else
        ///     {
        ///        do something
        ///     }
        /// }
        /// </summary>
        public static void ShowVideoReward(Action<AdEvent, AdType> onSuccess, string placementName, string itemName = "default")
        {
            try
            {
                if (!instance)
                {
                    Debug.LogError("[AdsManager] could not find the AdsManager object. Please ensure you have added the AdsManager Prefab to your scene.");
                    onSuccess?.Invoke(AdEvent.ShowNotAvailable, AdType.Reward);
                    return;
                }

                if (IsNotShowAds)
                {
                    Debug.LogError($"Not Show Ads => Reward");
                    onSuccess?.Invoke(AdEvent.ShowSuccess, AdType.Reward);
                    return;
                }

                instance.StartCoroutine(DOInit());

                Debug.Log(string.Format("[AdsManager] ShowVideoReward IRON {0} MAX {1} ADMOB {2} LastAdEvent {3} IsTime {4}", IronHelper.RewardIsReady, MaxHelper.RewardIsReady, AdmobHelper.RewardIsReady, LastAdEvent, IsTimeToShowAds));

                rewardToInter = GameConfig.forceRewardToInter;

                SetStatus(AdType.Reward, AdEvent.Show, placementName, itemName, DefaultMediation);

#if (UNITY_EDITOR && !USE_ADMOB) || REMOVE_ADS
                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
                {
                    onSuccess?.Invoke(AdEvent.ShowSuccess, AdType.Reward);
                    SetStatus(AdType.Reward, AdEvent.ShowSuccess, placementName, itemName, DefaultMediation);
                    return;
                }
#endif

#if !USE_MAX && !USE_IRON && !USE_ADMOB && !USE_UNITY
                onSuccess?.Invoke(AdEvent.ShowSuccess, AdType.Reward);
                SetStatus(AdType.Reward, AdEvent.ShowSuccess, placementName, itemName, DefaultMediation);
                return;
#else

                if (string.IsNullOrEmpty(placementName))
                    placementName = "default";

                if (string.IsNullOrEmpty(itemName))
                    itemName = "default";

                if (DefaultMediation == AdMediation.IRON)
                {
                    IronHelper.ShowRewarded(onSuccess, placementName, itemName);
                }
                else if (DefaultMediation == AdMediation.MAX)
                {
                    MaxHelper.ShowRewarded(onSuccess, placementName, itemName);
                }
                else
                {
                    Debug.LogWarning("[AdsManager] ShowVideoReward --> NOT DefaultMediation ------------------------------");

                    onSuccess?.Invoke(AdEvent.ShowNotAvailable, AdType.Reward);

                    SetStatus(AdType.Reward, AdEvent.ShowNotAvailable, placementName, itemName, DefaultMediation);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                onSuccess?.Invoke(AdEvent.Exception, AdType.Reward);
            }
        }

        public static void InitBanner(BannerPos bannerPos)
        {
            if (instance == null)
            {
                Debug.LogError("[AdsManager] could not find the AdsManager object. Please ensure you have added the AdsManager Prefab to your scene.");
                return;
            }

            if (Settings.useBanner == AdMediation.IRON)
            {
                IronHelper.InitBanner(bannerPos);
            }
            else if (Settings.useBanner == AdMediation.MAX)
            {
                MaxHelper.InitBanner(bannerPos);
            }
            else if (Settings.useBanner == AdMediation.ADMOD)
            {
                AdmobHelper.InitBanner(bannerPos);
            }
        }

        public static void DestroyBanner()
        {
            if (Settings.useBanner == AdMediation.IRON)
            {
                IronHelper.DestroyBanner();
                instance.AdCollap.DestroyBanner();
            }
            else if (Settings.useBanner == AdMediation.MAX)
            {
                MaxHelper.DestroyBanner();
            }
            else if (Settings.useBanner == AdMediation.ADMOD)
            {
                AdmobHelper.DestroyBanner();
            }
        }

        public static void ShowBanner(bool canShowCollap)
        {
            if (canShowCollap && AdCollap.IsReady)
            {
                HideBanner();
                instance.AdCollap.ShowBanner();
            }
            else
            {
                instance.AdCollap.DestroyBanner();

                if (Settings.useBanner == AdMediation.IRON)
                {
                    IronHelper.ShowBanner();
                }
                else if (Settings.useBanner == AdMediation.MAX)
                {
                    MaxHelper.ShowBanner();
                }
                else if (Settings.useBanner == AdMediation.ADMOD)
                {
                    AdmobHelper.ShowBanner();
                }
            }
        }

        public static void HideBanner()
        {
            if (Settings.useBanner == AdMediation.IRON)
            {
                IronHelper.HideBanner();
            }
            else if (Settings.useBanner == AdMediation.MAX)
            {
                MaxHelper.HideBanner();
            }
            else if (Settings.useBanner == AdMediation.ADMOD)
            {
                AdmobHelper.HideBanner();
            }
        }

        public static void ShowBannerMediumRectangle(string placementName, Vector2 customPosition = default)
        {
            AdmobHelper.ShowBannerMediumRectangle(placementName, customPosition);
        }

        public static void HideBannerMediumRectangle()
        {
            AdmobHelper.HideBannerMediumRectangle();
        }

        public static AdEvent LastAdEvent { get; set; }
        public static AdType LastAdType { get; set; }
        public static AdMediation LastMediation { get; set; }

        public static void SetStatus(AdType adType, AdEvent adEvent, string placementName = "default", string itemName = "default", AdMediation mediation = AdMediation.NONE)
        {
            if (instance)
            {
                try
                {
                    if (UserData == null)
                        return;

                    if (adType != AdType.Banner)
                    {
                        if (adEvent == AdEvent.Close)
                        {
                            LastTimeShowAd = DateTime.Now;
                        }
                        else if (adEvent == AdEvent.ShowSuccess)
                        {
                            if (adType == AdType.Inter)
                            {
                                LastTimeShowAd = DateTime.Now;
                                totalSuccess++;
                                totalInterSuccess++;
                                UserData.TotalAdInterstitial++;
                                LogEvent("ad_success", ParamsBase(placementName, itemName, mediation));
                                DataManager.Save();
                            }

                            if (adType == AdType.Reward)
                            {
                                LastTimeShowAd = DateTime.Now;
                                LastTimeShowRv = DateTime.Now;
                                totalSuccess++;
                                totalRewardSuccess++;
                                UserData.TotalAdRewarded++;
                                LogEvent("ad_success", ParamsBase(placementName, itemName, mediation));
                                DataManager.Save();
                            }
                        }

                        LastAdEvent = adEvent;
                        LastAdType = adType;
                        LastMediation = mediation;

                        if (adEvent == AdEvent.Load)
                            Debug.Log(mediation.ToString() + " " + adType.ToString() + " " + adEvent.ToString());
                    }

                    OnStateChanged?.Invoke(adType, adEvent, placementName, itemName);

                    LogEvent(adType, adEvent, ParamsBase(placementName, itemName, mediation));
                    LogAppsFlyer(adType, adEvent, placementName, itemName);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public static bool IsNotShowAds
        {
            get
            {
                if (instance != null && instance.removeAdsToggle != null && instance.removeAdsToggle.isOn)
                    return true;
                if (UserData != null)
                    return UserData.isVIP || UserData.isRemovedAds;
                return false;
            }
        }

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

        public static string InternetStatus
        {
            get
            {
                switch (Application.internetReachability)
                {
                    case NetworkReachability.ReachableViaLocalAreaNetwork:
                        return "Wifi";
                    case NetworkReachability.ReachableViaCarrierDataNetwork:
                        return "Carrier";
                    default:
                        return "None";
                }
            }
        }

        public static Dictionary<string, object> ParamsBase(string placementName = "default", string itemName = "default", AdMediation mediation = AdMediation.NONE)
        {
            return new Dictionary<string, object>
            {
                { "internet", InternetStatus},
                { "in_session", totalSuccess.ToString() },
                { "session", UserData.Session.ToString() },
                { "ad_interstitial", UserData.TotalAdInterstitial.ToString() },
                { "ad_rewarded", UserData.TotalAdRewarded.ToString() },
                { "ad_platform", mediation.ToString() },
                { "placement", placementName.ToLower() },
                { "item", itemName.ToLower() },
                { "total", (UserData.TotalAdRewarded + UserData.TotalAdInterstitial).ToString()}
            };
        }

        public static void ShowNotice(AdEvent onSuccess)
        {
            var task = instance.AsyncShowNotice(onSuccess);
        }
        private async UniTask AsyncShowNotice(AdEvent onSuccess)
        {
            await UniTask.SwitchToMainThread();
            try
            {
                if (onSuccess == AdEvent.ShowNoInternet)
                    UIToast.ShowNotice("Please check your internet connection...!");
                else if (onSuccess == AdEvent.ShowNotAvailable)
                    UIToast.ShowNotice("Video not ready, please try again...!");
                else if (onSuccess == AdEvent.Exception)
                    UIToast.ShowNotice("Something wrong, please try again...!");
                else if (onSuccess == AdEvent.ShowSuccess)
                    Log("Show Ads success!");
                else if (onSuccess == AdEvent.ShowStart)
                    Log("Time to show ADs!");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void UpdateBannerArea()
        {
            if (instance != null && Settings.useBanner != AdMediation.NONE)
            {
                var bannerObjs = FindObjectsOfType<AdsBannerObj>();
                foreach (var i in bannerObjs)
                {
                    if (i != null)
                        i.SetActive(!IsNotShowAds);
                }

                if (instance.parentTransform == null)
                    instance.parentTransform = UIAnimManager.RootRectTransform;

                if (instance.parentTransform != null && AdsBannerAreaList != null)
                {
                    //foreach (var i in AdsBannerAreaList)
                    //    i.SetArea(instance.bannerHeight / instance.parentTransform.rect.height, IsNotShowAds ? BannerPos.NONE : BannerPos);
                }

                if (IsNotShowAds)
                    DestroyBanner();
            }
        }

        public static void Log(string value)
        {
            Logger.d(value);
        }

        public static void LogWarning(string value)
        {
            Logger.w(value);
        }

        public static void LogError(string value)
        {
            Logger.e(value);
        }

        public static void LogException(Exception ex)
        {
            Logger.CrashlyticsException(ex);
        }

        public static void LogEvent(AdType adType, AdEvent adEvent, Dictionary<string, object> eventParams)
        {
            string eventName = "ad_" + adType.ToString().ToLower() + "_" + adEvent.ToString().ToLower();
            LogEvent(eventName, eventParams);
        }

        public static void LogEvent(string eventName, Dictionary<string, object> eventParams)
        {
            if (eventParams == null)
                eventParams = ParamsBase();

#if USE_FIREBASE
            FirebaseManager.LogEvent(eventName, eventParams);
#else
            string stringLog = eventName + "\n";
            foreach (var k in eventParams)
            {
                stringLog += string.Format("{0}: {1} \n", k.Key, k.Value);
            }
            Debug.Log("--------> LogEvent " + stringLog);
#endif
        }

        public static CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
        public static void LogImpressionData(AdMediation mediation, object data, string adUnitId = null)
        {
            try
            {
                double revenue = 0;
                string ad_network = "";
                string ad_unit_name = "";
                string ad_format = "";
                string country = "";
                double lifetime_revenue = 0;
                string currency = "USD";
                string placement = "";
#if USE_GADSME
                if (data != null && data is GadsmeImpressionData)
                {
                    var impressionData = data as GadsmeImpressionData;
                    ad_network = impressionData.platform;
                    revenue = impressionData.netRevenue;
                    ad_unit_name = impressionData.placementId;
                    ad_format = impressionData.adFormat.GetName();
                    country = impressionData.countryCode;
                    currency = impressionData.currency;
                }
#endif
#if USE_MAX
                if (data != null && data is MaxSdkBase.AdInfo)
                {
                    var impressionData = data as MaxSdkBase.AdInfo;

                    ad_network = impressionData.NetworkName;
                    ad_unit_name = impressionData.AdUnitIdentifier;
                    ad_format = impressionData.Placement;
                    revenue = impressionData.Revenue;
                    placement = impressionData.Placement;
                }

                AnalyticsRevenueAds.SendEvent(new ImpressionData()
                {

                });
#endif

#if USE_IRON
                if (data != null && data is IronSourceImpressionData)
                {
                    var impressionData = data as IronSourceImpressionData;
                    ad_network = impressionData.adNetwork;
                    ad_unit_name = impressionData.adUnit;
                    ad_format = impressionData.instanceName;
                    revenue = (double)(impressionData.revenue != null ? impressionData.revenue : 0.0f);
                    country = impressionData.country;
                    lifetime_revenue = (double)(impressionData.lifetimeRevenue != null ? impressionData.lifetimeRevenue : 0.0f);

                    AnalyticsRevenueAds.SendRevToAdjust(impressionData);
                }
#endif


#if USE_ADSOPEN || USE_ADMOB
                if (data != null && data is GoogleMobileAds.Api.AdValueEventArgs)
                {
                    var impressionData = data as GoogleMobileAds.Api.AdValueEventArgs;
                    if (impressionData != null || impressionData.AdValue != null)
                    {
                        revenue = impressionData.AdValue != null ? impressionData.AdValue.Value : 0.0f;
                        currency = impressionData.AdValue.CurrencyCode;
                    }
                }
#endif

#if USE_FIREBASE
                if (FirebaseManager.AnalyticStatus == FirebaseStatus.Initialized)
                {
                    Firebase.Analytics.Parameter[] AdParameters = {
                    new Firebase.Analytics.Parameter("ad_platform", "applovin"),
             new Firebase.Analytics.Parameter("ad_source", RegexString(ad_network)),
             new Firebase.Analytics.Parameter("ad_unit_name", RegexString(ad_unit_name)),
             new Firebase.Analytics.Parameter("currency","USD"),
             new Firebase.Analytics.Parameter("value",revenue),
             new Firebase.Analytics.Parameter("placement",RegexString(placement)),
             new Firebase.Analytics.Parameter("country_code",country),
             new Firebase.Analytics.Parameter("ad_format",RegexString(ad_format)),
        };
                    Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression_rocket_max", AdParameters);

                    //FirebaseAnalytics.LogEvent("ad_impression_ironsource",
                    //    new Parameter[] {
                    //    new Parameter("is_connected", RegexString(InternetStatus)),
                    //    new Parameter("ad_platform", "iron_source"),
                    //    new Parameter("ad_source",RegexString( ad_network)),
                    //    new Parameter("ad_unit_name",RegexString( ad_unit_name)),
                    //    new Parameter("ad_format", RegexString(ad_format)),
                    //    new Parameter("value", revenue ),
                    //    new Parameter("currency", "USD"),
                    //    new Parameter("country_code", country),
                    //    new Parameter("lifetime_revenue", lifetime_revenue)
                    //});

                    //FirebaseAnalytics.LogEvent("ad_impression",
                    //    new Parameter[] {
                    //    new Parameter("is_connected", RegexString(InternetStatus)),
                    //    new Parameter("ad_platform", RegexString(mediation)),
                    //    new Parameter("ad_source",RegexString( ad_network)),
                    //    new Parameter("ad_unit_name",RegexString( ad_unit_name)),
                    //    new Parameter("ad_format", RegexString(ad_format)),
                    //    new Parameter("value", revenue ),
                    //    new Parameter("currency", currency),
                    //    new Parameter("country", country),
                    //    new Parameter("lifetime_revenue", lifetime_revenue)
                    //});
                }
#endif


                //#if USE_APPSFLYER && USE_IRON
                //                if (data != null && data is IronSourceImpressionData)
                //                {
                //                    var impressionData = data as IronSourceImpressionData;
                //                    var afParameters = new Dictionary<string, string>
                //                     {
                //                        { /*"af_currency"*/AFAdRevenueEvent.COUNTRY, impressionData.country },
                //                        {/* "af_content_id"*/AFAdRevenueEvent.AD_UNIT,impressionData.adUnit },
                //                        { /*"af_revenue"*/AFAdRevenueEvent.AD_TYPE, impressionData.instanceName/*revenue.ToString("#0.00000", culture)*/ },
                //                        { /*"af_level"*/AFAdRevenueEvent.PLACEMENT, /*DataManager.UserData.TotalWin.ToString()*/impressionData.placement },
                //                        { AFAdRevenueEvent.ECPM_PAYLOAD, impressionData.encryptedCPM}
                //                     };

                //                    AppsFlyerSDK.AppsFlyer.sendEvent(ad_unit_name.ToLower(), afParameters);
                //                    AppsFlyerSDK.AppsFlyerAdRevenue.logAdRevenue(impressionData.adNetwork, AppsFlyerSDK.AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeIronSource, impressionData.revenue.Value, "USD", afParameters);
                //                }
                //#endif

                //                string stringLog = "ad_impression" + " " + mediation.ToString() + " " + ad_network + " " + ad_unit_name + " " + ad_format + " " + revenue.ToString("#0.00000", culture) + " " + currency.ToString();

                //                Debug.Log("--------> " + stringLog);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static string RegexString(object value)
        {
            if (value != null)
                return Regex.Replace(value.ToString(), @"\B[A-Z]", m => "_" + m.ToString()).ToLower().Replace("ı", "i");
            return "";
        }

        private void OnApplicationPause(bool isPaused)
        {
            Log("OnApplicationPause " + isPaused);
#if USE_IRON
            IronSource.Agent.onApplicationPause(isPaused);
#endif
//            if (!isPaused)
//            {
//#if USE_MAXOPEN
//                if (MaxHelper.OpenIsReady && IsTimeToShowAdOpen)
//                {
//                    MaxHelper.ShowOpenAdIfAvailable("app_pause");
//                    return;
//                }
//                else
//                {
//                    Log(MaxHelper.TAG + "WaitToShow: " + MaxHelper.StatusOpen.ToString() + " " + MaxHelper.OpenIsReady + " ---> Check Inter DEFAULT");
//                }
//#endif
//            }
        }

//        private static void OnAppStateChanged(AppState state)
//        {
//            if (state == AppState.Foreground && IsTimeToShowAdOpen)
//            {
//#if USE_ADOPEN
//                if (GameConfig != null)
//                {
//                    AdOpen.ShowOpenAdIfAvailable("app_pause");
//                    return;
//                }
//                else
//                {
//                    Log(AdOpen.TAG + "WaitToShow: " + AdOpen.Status.ToString() + " " + AdOpen.IsReady + " ---> Check Open MAX");
//                }
//#endif

//#if USE_MAXOPEN
//                if (MaxHelper.AppOpenIsInit && MaxHelper.OpenIsReady)
//                {
//                    MaxHelper.ShowOpenAdIfAvailable("app_pause");
//                    return;
//                }
//                else
//                {
//                    Log(MaxHelper.TAG + "Pause-> WaitToShow: " + MaxHelper.StatusOpen.ToString() + " ---> OpenIsReady: " + MaxHelper.OpenIsReady);
//                }
//#endif
//            }
//        }

        public static void LogAppsFlyer(AdType adType, AdEvent adEvent, string placementName, string itemName)
        {
#if USE_APPSFLYER
            var param = new Dictionary<string, string>()
            {
                { "placement", placementName.ToLower() },
                { "item", itemName.ToLower() },
                { "internet", InternetStatus.ToLower()}
            };
            if (adType == AdType.Inter)
            {
                if (adEvent == AdEvent.Show)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_inters_ad_eligible", param); //Bắn lên khi gọi hàm show quảng cáo inter của game (bắn lên khi ấn nút show ads theo logic của game)
                else if (adEvent == AdEvent.ShowStart)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_inters_api_called", param); //Bắn lên nếu quảng cáo có sẵn trong game khi gọi hàm show quảng cáo (bắn lên khi ads available)
                else if (adEvent == AdEvent.ShowSuccess)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_inters_displayed", param); //Bắn lên khi ad hiện lên màn hình cho user xem (open inter)
            }
            else if (adType == AdType.Reward)
            {
                if (adEvent == AdEvent.Show)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_rewarded_ad_eligible", param); //Bắn lên khi gọi hàm show quảng cáo reward của game(bắn lên khi ấn nút show ads theo logic của game)
                else if (adEvent == AdEvent.ShowStart)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_rewarded_api_called", param); //Bắn lên nếu quảng cáo có sẵn trong game khi gọi hàm show quảng cáo (bắn lên khi ads available)
                else if (adEvent == AdEvent.ShowSuccess)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_rewarded_ad_displayed", param); //Bắn lên khi ad hiện lên màn hình cho user xem(open reward)
            }
            else if (adType == AdType.AppOpen)
            {
                if (adEvent == AdEvent.Show)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_appopen_ad_eligible", param); //Bắn lên khi gọi hàm show quảng cáo appopen của game(bắn lên khi ấn nút show ads theo logic của game)
                else if (adEvent == AdEvent.ShowStart)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_appopen_api_called", param); //Bắn lên nếu quảng cáo có sẵn trong game khi gọi hàm show quảng cáo (bắn lên khi ads available)
                else if (adEvent == AdEvent.ShowSuccess)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_appopen_ad_displayed", param); //Bắn lên khi ad hiện lên màn hình cho user xem (open appopen)
            }
            else if (adType == AdType.Banner)
            {
                if (adEvent == AdEvent.Show)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_banner_ad_eligible", param); //Bắn lên khi gọi hàm show quảng cáo small banner của game(bắn lên khi ấn nút show ads theo logic của game)
                else if (adEvent == AdEvent.ShowStart)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_banner_api_called", param); //Bắn lên nếu quảng cáo có sẵn trong game khi gọi hàm show quảng cáo (bắn lên khi ads available)
                else if (adEvent == AdEvent.ShowSuccess)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_banner_ad_displayed", param); //Bắn lên khi ad hiện lên màn hình cho user xem (open small banner)
            }
            else if (adType == AdType.Rectangle)
            {
                if (adEvent == AdEvent.Show)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_rectanggle_ad_eligible", param); //Bắn lên khi gọi hàm show quảng cáo rectangle banner của game(bắn lên khi ấn nút show ads theo logic của game)
                else if (adEvent == AdEvent.ShowStart)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_rectanggle_api_called", param); //Bắn lên nếu quảng cáo có sẵn trong game khi gọi hàm show quảng cáo (bắn lên khi ads available)
                else if (adEvent == AdEvent.ShowSuccess)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_rectanggle_ad_displayed", param); //Bắn lên khi ad hiện lên màn hình cho user xem (open rectangle banner)
            }

            if (adEvent == AdEvent.ShowSuccess)
            {
                AppsFlyerSDK.AppsFlyer.sendEvent("ad_" + adType.ToString().ToLower(), param);
            }
#endif
        }
        public static void LogAdjust(AdType adType, AdEvent adEvent, string placementName, string itemName)
        {
        }
        public static void PauseApp(bool pause)
        {
            //if (pause)
            //{
            //    SetVolumeFade(0, 0.25f);
            //    Time.timeScale = 0;
            //}
            //else
            //{
            //    Time.timeScale = 1;
            //    SetVolumeFade(1, 1f);
            //}
        }

        public static void SetVolumeFade(float endValue, float duration)
        {
            DOTween.Kill("SetVolumeFade", true);
            var volume = AudioListener.volume;
            DOVirtual.Float(volume, endValue, duration, (v) =>
            {
                AudioListener.volume = v;
            })
            .SetId("SetVolumeFade")
            .SetUpdate(true);
        }
        public static async UniTask ActionAfterBackToMain(Action callback)
        {
            await UniTask.SwitchToMainThread();
            await UniTask.Yield();
            callback?.Invoke();
        }
        //public static void CountInterToOfferNoAds()
        //{
        //    if (!UserData.isRemovedAds)
        //    {
        //        UserData.interCountToOfferNoAds++;
        //        if (UserData.interCountToOfferNoAds >= GameConfig.totalInterWatchedToOfferNoAds)
        //        {
        //            UserData.interCountToOfferNoAds = 0;
        //            instance.PostEvent((int)EventID.OnOfferRemoveAds);
        //        }
        //    }
        //}
    }
}
