using System;
using UnityEngine;

[Serializable]
public class GameConfig : GameConfigBase
{
    [Header("ADs")]
    public bool adUseBackup = false;
    public bool adUseOpenBackup = false;
    public bool adUseNative = false;
    public bool adUseInPlay = false;
    public bool forceInterToReward = false;
    public bool forceRewardToInter = false;
    public bool forceInterEverywhere = false;
    public float timeToWaitOpenInter = 4.5f;

    public float timePlayToShowAds = 30;
    public float timePlayReduceToShowAds = 15;
    public float timePlayToShowAdsBreak = 30;
    public int adRewardNotToInter = 1;
    public int adInterNotToReward = 1;
    public int adInterViewToReward = 5;
    public float removeAdsCost = 5.99f;
    public bool isAdsByPass = false;

    public float timeCappingInterAfterRv = 90;

    #region MONEY
    [Header("Money")]
    [SerializeField]
    private int _goldByAds = 150;
    public int goldByAds
    {
        get
        {
            if (_goldByAds <= 0)
                _goldByAds = 150;
            return _goldByAds;
        }
        set
        {
            if (value != _goldByAds)
                _goldByAds = value;
        }
    }
    #endregion
}

[Serializable]
public enum RebornBy
{
    Free,
    Gold,
    Gem,
    Ads
}