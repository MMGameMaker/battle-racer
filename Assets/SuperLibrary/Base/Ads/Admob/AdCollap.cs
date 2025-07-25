#if USE_ADCOLLAP
using GoogleMobileAds.Api;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Base.Ads.AdsManager;

public class AdCollap : MonoBehaviour
{
    public static bool IsReady => instance.isReady;

    [SerializeField] string TAG = "ADMOB COLLAP ";
    public bool useAdmobTestAdUnitId = false;
    protected AdType adType = AdType.BannerCollap;
    protected AdMediation mediation = AdMediation.ADMOD;
    protected string bannerAdUnitId = "";
    protected string placementName = "default";
    protected string itemName = "default";

#if USE_ADCOLLAP
    protected BannerView bannerView = null;
#endif

    protected AdEvent _status = AdEvent.None;
    protected AdEvent status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                SetStatus(AdType.AppOpen, value, placementName, placementName, mediation);
            }
        }
    }
    bool isReady => status == AdEvent.LoadAvaiable;
    protected Action<AdEvent, AdType> onLoad = null;
    static AdCollap instance;
    private void Awake()
    {
        instance = this;        
    }

    public IEnumerator DOInitAd()
    {
        if (useAdmobTestAdUnitId)
        {
            bannerAdUnitId = "ca-app-pub-3940256099942544/2014213617";
        }
        else
        {
            if (Application.platform == RuntimePlatform.Android)
                bannerAdUnitId = Settings.admobAndroidBannerUnitId;
            else
                bannerAdUnitId = Settings.admobIOSBannerUnitId;
        }

        Log(TAG + "Banner Collaps: " + bannerAdUnitId);
        yield return null;
    }
    public IEnumerator DoLoadAd()
    {
        if (IsNotShowAds)
        {
            status = AdEvent.LoadNotAvaiable;
            yield break;
        }

        if (string.IsNullOrEmpty(bannerAdUnitId))
        {
            status = AdEvent.LoadNotAvaiable;
            Log(TAG + "Load: adUnitId NULL or EMPTY --> return");
            yield break;
        }

        if (status == AdEvent.Load)
        {
            Log(TAG + $"Load : is loading --> return");
            yield break;
        }

        if (status == AdEvent.LoadAvaiable)
        {
            Log(TAG + $"Load : is avaiable --> return");
            yield break;
        }

        status = AdEvent.Load;

#if USE_ADCOLLAP
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        bannerView.OnBannerAdLoaded += OnBannerAdLoadedEvent;
        bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailedEvent;

        AdRequest request = new AdRequest();
        request.Extras.Add("collapsible", "bottom");
        request.Extras.Add("collapsible_request_id", System.Guid.NewGuid().ToString());
        bannerView.LoadAd(request);
#endif
    }

    public IEnumerator WaitUntilLoaded(Action<AdEvent, AdType> load, float timeOut = 3)
    {
        placementName = "default";
        itemName = "default";
        onLoad = load;

        if (!UMP.CanRequestAds)
        {
            Log(TAG + "WaitUntilLoaded: CanRequestAds=" + UMP.CanRequestAds + " --> return");
            instance.StartCoroutine(UMP.DOGatherConsent(null));
            onLoad?.Invoke(AdEvent.LoadNotAvaiable, AdType.BannerCollap);
            onLoad = null;
            yield break;
        }
        if (!isMobileAdsInitialize)
        {
            Log(TAG + "WaitUntilLoaded: isMobileAdsInitialize=" + isMobileAdsInitialize + " --> return");
            yield break;
        }

        if (isReady)
        {
            onLoad?.Invoke(AdEvent.LoadAvaiable, AdType.BannerCollap);
            onLoad = null;
            yield break;
        }
        else
        {
            yield return DOInitAd();
            yield return DoLoadAd();
        }

        float elapsedTime = 0;
        while (status == AdEvent.Load && elapsedTime < timeOut)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (status == AdEvent.Load && elapsedTime >= timeOut)
        {
            status = AdEvent.LoadTimeout;
        }

        onLoad?.Invoke(status, adType);
        onLoad = null;
    }

#if USE_ADCOLLAP
    private void OnBannerAdLoadFailedEvent(LoadAdError error)
    {
        status = AdEvent.LoadNotAvaiable;
        LogError(TAG + "OnLoadFailed: " + bannerAdUnitId + " with error: " + error.GetMessage());
    }

    private void OnBannerAdLoadedEvent()
    {
        status = AdEvent.LoadAvaiable;
        onLoad?.Invoke(AdEvent.LoadAvaiable, AdType.BannerCollap);
        onLoad = null;

        HideBanner();
    }
#endif

    public void DestroyBanner()
    {
#if USE_ADCOLLAP
        status = AdEvent.None;
        if (bannerView != null)
        {
            bannerView.OnBannerAdLoaded -= OnBannerAdLoadedEvent;
            bannerView.OnBannerAdLoadFailed -= OnBannerAdLoadFailedEvent;
            bannerView.Destroy();
            bannerView = null;
        }
#endif
    }
    public void HideBanner()
    {
#if USE_ADCOLLAP
        if (bannerView != null)
            bannerView.Hide();
#endif
    }

    public void ShowBanner()
    {
#if USE_ADCOLLAP
        if (isReady && bannerView != null)
            bannerView.Show();
#endif
    }
}
