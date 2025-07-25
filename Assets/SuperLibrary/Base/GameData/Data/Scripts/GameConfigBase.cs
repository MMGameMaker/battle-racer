using System;
using UnityEngine;

[Serializable]
public class GameConfigBase
{
    public int suggestUpdateVersion = 0;
    [Header("Ads Config")]
    [SerializeField]
    protected int _timePlayToShowAd = 15;
    public int timePlayToShowAd
    {
        get
        {
            return _timePlayToShowAd;
        }
        set
        {
            _timePlayToShowAd = value;
        }
    }

    /// <summary>
    /// On show any ads success --> increase or decrease timePlayToShowAd.
    /// Ex:
    /// firsTime timePlayToShowAd = 15s
    /// timePlayToShowAdRecuce = -5s
    /// --> show ads success = 2
    /// nextTime timePlayToShowAd = 15 + (2 * -5) = 5s
    /// </summary>

    [SerializeField]
    protected int _timePlayToShowAdReduce = -5;
    public int timePlayToShowAdReduce
    {
        get
        {
            return _timePlayToShowAdReduce;
        }
        set
        {
            _timePlayToShowAdReduce = value;
        }
    }

    [SerializeField]
    protected float _timeToWaitOpenAd = 5;
    public float timeToWaitOpenAd
    {
        get
        {
            return _timeToWaitOpenAd;
        }
        set
        {
            _timeToWaitOpenAd = value;
        }
    }
    [SerializeField]
    protected int _minLevelToShowInter = 1;
    public int minLevelToShowInter
    {
        get
        {
            return _minLevelToShowInter;
        }
        set
        {
            _minLevelToShowInter = value;
        }
    }
    [SerializeField]
    protected int _timePlayToShowOpenAd = 5;
    public int timePlayToShowOpenAd
    {
        get
        {
            return _timePlayToShowOpenAd;
        }
        set
        {
            _timePlayToShowOpenAd = value;
        }
    }

    [Header("Ads Extra")]

    [SerializeField]
    private bool _adInterOnComplete = true;
    public bool adInterOnComplete
    {
        get
        {
            return _adInterOnComplete;
        }
        set => _adInterOnComplete = value;
    }

    [Header("Revice")]
    [SerializeField]
    private int _reviceCountDown = 5;
    public int reviceCountDown
    {
        get
        {
            if (_reviceCountDown < 0)
                _reviceCountDown = 5;
            return _reviceCountDown;
        }
        set => _reviceCountDown = value;
    }

    [SerializeField]
    private int _reviceCountMax = 3;
    public int reviceCountMax
    {
        get
        {
            if (_reviceCountMax < 0)
                _reviceCountMax = 0;
            return _reviceCountMax;
        }
        set => _reviceCountMax = value;
    }

    [SerializeField]
    private int _reviceNoThankCountDown = 2;
    public int reviceNoThankCountDown
    {
        get
        {
            if (_reviceNoThankCountDown < 0)
                _reviceNoThankCountDown = 2;
            return _reviceNoThankCountDown;
        }
        set => _reviceNoThankCountDown = value;
    }

    [SerializeField]
    private int _reviceFree = 1;
    public int reviceFree
    {
        get
        {
            return _reviceFree;
        }
        set => _reviceFree = value;
    }

    [SerializeField]
    private GameState _reviceType = GameState.None;
    public GameState reviceType
    {
        get
        {
            return _reviceType;
        }
        set => _reviceType = value;
    }
}