using System;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;


[Serializable]
public class UserData : UserAnalysic
{
    public delegate void VIPChangedDelegate(bool isVip);
    public static event VIPChangedDelegate OnVIPChanged;

    public delegate void RemovedAdsChangedDelegate(bool isRemoveAds);
    public static event RemovedAdsChangedDelegate OnRemovedAdsChanged;

    [Header("Data")]
    public int level = 0;
    private string lastTimeUpdate = new DateTime(1999, 1, 1).ToString();

    public DateTime LastTimeUpdate
    {
        get => DateTimeConverter.ToDateTime(lastTimeUpdate);
        set => lastTimeUpdate = value.ToString();
    }

    private string fistTimePlayLevel = DateTime.Now.ToString();
    public DateTime FistTimePlayLevel
    {
        get => DateTimeConverter.ToDateTime(fistTimePlayLevel);
        set => fistTimePlayLevel = value.ToString();
    }

    [JsonProperty]
    private bool _isRemovedAds;
    [JsonIgnore]
    public bool isRemovedAds
    {
        get => _isRemovedAds;
        set
        {
            if (value != _isRemovedAds)
            {
                _isRemovedAds = value;
                SetUserProperty("is_removed_ads", _isRemovedAds);
                OnRemovedAdsChanged?.Invoke(_isRemovedAds);
            }
        }
    }
    [SerializeField, JsonProperty]
    protected bool _isVip = false;
    [JsonIgnore]
    public bool isVIP
    {
        get => _isVip;
        set
        {
            if (value != _isVip)
            {
                _isVip = value;
                SetUserProperty("is_VIP", _isVip);
                OnVIPChanged?.Invoke(_isVip);
            }
        }
    }

    [Header("Money")]
    [SerializeField, JsonProperty]
    private int coin = 0;
    [JsonIgnore]
    public int totalCoin
    {
        get => coin;
        set
        {
            if (coin < 2000000000)
            {
                if (coin != value)
                {
                    int changed = 0;
                    if (coin > value)
                    {
                        changed = coin - value;
                        totalCoinSpend += changed;
                    }
                    else
                    {
                        changed = value - coin;
                        totalCoinEarn += changed;
                    }

                    coin = value;
                    OnCoinChanged?.Invoke(changed, coin);
                }
            }
            else
            {
                UIToast.ShowError("Don't do that!");
                coin = 100;
                totalCoinEarn = 0;
                totalCoinSpend = 0;
            }
        }
    }
    public int totalCoinEarn = 0;
    public int totalCoinSpend = 0;

    [SerializeField, JsonProperty] 
    private int diamond;
    [JsonIgnore]
    public int totalDiamond
    {
        get => diamond;
        set
        {
            if (diamond != value)
            {
                int changed = 0;
                if (diamond > value)
                {
                    changed = diamond - value;
                    totalDiamondSpend += changed;
                }
                else
                {
                    changed = value - diamond;
                    totalDiamondEarn += changed;
                }

                diamond = value;
                OnDiamondChanged?.Invoke(changed, diamond);
            }
        }
    }
    public int totalDiamondEarn = 0;
    public int totalDiamondSpend = 0;

    public delegate void MoneyChangedDelegate(int change, int current);
    public static event MoneyChangedDelegate OnCoinChanged;
    public static event MoneyChangedDelegate OnDiamondChanged;
}

[Serializable]
public class UserAnalysic : UserBase
{
    [Header("Analysic")]
    [JsonProperty]
    private int versionInstall;
    [JsonIgnore]
    public int VersionInstall
    {
        get => versionInstall;
        set
        {
            if (versionInstall != value)
            {
                versionInstall = value;
            }
        }
    }
    [JsonProperty]
    private int versionCurrent;
    [JsonIgnore]
    public int VersionCurrent
    {
        get => versionCurrent;
        set
        {
            if (versionCurrent != value)
            {
                versionCurrent = value;
            }
        }
    }
    [JsonProperty]
    private int session = 0;
    [JsonIgnore]
    public int Session
    {
        get => session;
        set
        {
            if (session != value && value > 0)
            {
                session = value;
            }
        }
    }
    [JsonProperty]
    private long totalPlay = 0;
    [JsonIgnore]
    public long TotalPlay
    {
        get => totalPlay;
        set
        {
            if (totalPlay != value && value > 0)
            {
                totalPlay = value;
            }
        }
    }
    [JsonProperty]
    private int totalWin = 0;
    [JsonIgnore]
    public int TotalWin
    {
        get => totalWin;
        set
        {
            if (totalWin != value && value > 0)
            {
                totalWin = value;
            }
        }
    }
    [JsonProperty]
    private int winStreak = 0;
    [JsonIgnore]
    public int WinStreak
    {
        get => winStreak;
        set
        {
            if (winStreak != value)
            {
                winStreak = value;
            }
        }
    }
    [JsonProperty]
    private int loseStreak = 0;
    [JsonIgnore]
    public int LoseStreak
    {
        get => loseStreak;
        set
        {
            if (loseStreak != value)
            {
                loseStreak = value;
            }
        }
    }
    [JsonProperty]
    private long totalTimePlay = 0;
    [JsonIgnore]
    public long TotalTimePlay
    {
        get => totalTimePlay;
        set
        {
            if (totalTimePlay != value && value > 0)
            {
                totalTimePlay = value;
            }
        }
    }

    [Header("Ads")]
    [JsonProperty]
    private long totalAdInterstitial = 0;
    [JsonIgnore]
    public long TotalAdInterstitial
    {
        get => totalAdInterstitial;
        set
        {
            if (totalAdInterstitial != value && value > 0)
            {
                totalAdInterstitial = value;
            }
        }
    }
    [JsonProperty]
    private long totalAdRewarded = 0;
    [JsonIgnore]
    public long TotalAdRewarded
    {
        get => totalAdRewarded;
        set
        {
            if (totalAdRewarded != value && value > 0)
            {
                totalAdRewarded = value;
            }
        }
    }

    public void SetUserProperty(string title, object value)
    {
        try
        {
#if USE_FIREBASE
            Base.FirebaseManager.SetUser(title.ToLower(), value.ToString());
#endif
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    [JsonProperty]
    private string firstTimeOpenApp = new DateTime(1999, 1, 1).ToString();
    [JsonIgnore]
    public DateTime FirstTimeOpenApp
    {
        get => DateTimeConverter.ToDateTime(firstTimeOpenApp);
        set => firstTimeOpenApp = value.ToString();
    }
}

[Serializable]
public class UserBase
{
    [Header("Base")]
    public string id;
    public string email;
    public string name;
}
