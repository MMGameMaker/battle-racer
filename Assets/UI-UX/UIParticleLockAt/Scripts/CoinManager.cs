using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    [SerializeField]
    private UITextNumber number = null;
    public static UITextNumber Number { get => instance?.number; }

    [SerializeField] ParticleLockAt particle = null;
    public static ParticleLockAt Particle { get => instance?.particle; }

    [SerializeField] UIAnimation anim;

    public Transform defaultTarget;

    public static int totalCoin
    {
        get => DataManager.UserData.totalCoin;
        private set => DataManager.UserData.totalCoin = value;
    }

    public static int CoinByAds => DataManager.GameConfig.goldByAds;

    public static CoinManager instance;

    private void OnDisable()
    {
        DOTween.Kill(instance.gameObject);
    }

    private void Awake()
    {
        instance = this;
        DataManager.OnLoaded += DataManager_OnLoaded;
    }

    private void Start()
    {
    }

    private void DataManager_OnLoaded(GameData gameData)
    {
        Number.DOAnimation(0, totalCoin, 0);
    }

    public static void Add(int numb, Transform fromTrans = null, Transform toTrans = null, float delay = 0)
    {
        var current = totalCoin;
        totalCoin += numb;
        if (Number != null)
        {
            if (numb > 0)
            {
                if (fromTrans)
                {
                    Particle.Emit(Mathf.Clamp(numb + 1, 0, 10), fromTrans, toTrans ?? instance.defaultTarget);
                }
                Number.DOAnimation(current, totalCoin, 0.5f, delay);
            }
            else
            {
                Number.DOAnimation(current, totalCoin, 0);
            }
        }
    }

    public static void Show()
    {
        instance.anim.Show(() =>
        {
        });
    }

    public static void Hide()
    {
        instance.anim.Hide(() =>
        {
        });
    }

    public static void GetByAds(Transform transform, string placement, Action<AdEvent> status = null)
    {
#if USE_IRON || USE_MAX || USE_ADMOB
        Base.Ads.AdsManager.ShowVideoReward((ae, at) =>
        {
            if (ae == AdEvent.ShowSuccess)
            {
                Add(CoinByAds, transform);
                Base.Ads.AdsManager.ShowNotice(ae);
            }
            else
            {
                Base.Ads.AdsManager.ShowNotice(ae);
            }
            status?.Invoke(ae);
        }, placement, "coin");
#endif
    }

    public void Test(int numb)
    {
        Add(numb);
    }


#if UNITY_EDITOR
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Test(100000);
        }
    }
#endif
}
