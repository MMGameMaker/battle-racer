using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using DG.Tweening;

public class PopupDailyCheckin : MonoBehaviour
{

    public static PopupDailyCheckin instance;

    [Header("Daily")]
    [SerializeField] DailyCheckin dailyCheckin;
    [SerializeField] Button closeBtn;
    [SerializeField] UIAnimation anim;
    [SerializeField] RectTransform[] dailyTfs;
    [SerializeField] RectTransform contentTf;
    [SerializeField] Image fadeImg;
    [SerializeField] RectTransform boardTf;
    [SerializeField] RectTransform boardStartTf;
    [SerializeField] RectTransform boardEndTf;

    [Header("Reward")]
    [SerializeField] Image rewardIcon;
    [SerializeField] Text nameRewardTxt;
    [SerializeField] Text amountRewardTxt;
    [SerializeField] Button claimBtn;
    [SerializeField] Button claimX2Btn;
    [SerializeField] Image congratulationImg;
    [SerializeField] Image glowImg;
    [SerializeField] Image lightImg;
    [SerializeField] Image adsImg;

    private List<DailyCheckin> dailyCheckins = new List<DailyCheckin>();

    private void Awake()
    {
        instance = this;
        dailyCheckin.CreatePool(1);
    }

    // Start is called before the first frame update
    void Start()
    {
        closeBtn?.onClick.RemoveAllListeners();
        closeBtn?.onClick.AddListener(() =>
        {
            Hide(0);
        });
        anim.OnShowCompleted.RemoveAllListeners();
        anim.OnShowCompleted.AddListener(() =>
        {
            Ins_FillData();
        });

        claimBtn?.onClick.RemoveAllListeners();
        claimBtn?.onClick.AddListener(() =>
        {
            HideReward();
        });

        claimX2Btn?.onClick.RemoveAllListeners();
        claimX2Btn?.onClick.AddListener(() =>
        {
            HideReward();
        });
    }

    [ButtonMethod]
    public void Show()
    {
        anim.Show(() =>
        {
            rewardIcon.SetAlpha(0);
            nameRewardTxt.transform.SetScale(0);
            amountRewardTxt.transform.SetScale(0);
            claimBtn.transform.SetScale(0);
            claimX2Btn.transform.SetScale(0);

            glowImg.SetAlpha(0);
            lightImg.SetAlpha(0);
            congratulationImg.transform.SetScale(0);

            fadeImg.SetAlpha(0);
            closeBtn.transform.SetScale(0);
            boardTf.position = boardStartTf.position;
            fadeImg.DOFade(1, 0.5f).SetId(gameObject).OnComplete(() =>
            {
                boardTf.DOMove(boardEndTf.position, 0.25f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    StartCoroutine(YieldSpawDailyCheckin());
                }).SetId(gameObject);
            }).SetId(gameObject);

            IEnumerator YieldSpawDailyCheckin()
            {
                for (int i = 0; i < dailyCheckins.Count; i++)
                {
                    dailyCheckins[i].Show();
                    yield return new WaitForSeconds(0.1f);
                }

                yield return new WaitForSeconds(0.3f);

                closeBtn.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutSine).SetId(gameObject);
            }
        });
    }

    public void Hide(int index)
    {
        closeBtn.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutSine).SetId(gameObject);
        boardTf.DOMove(boardStartTf.position, 0.25f).SetEase(Ease.InOutSine).SetDelay(0.25f).OnComplete(() =>
        {
            switch (index)
            {
                case 0:
                    fadeImg.DOFade(0, 0.5f).SetId(gameObject).OnComplete(() =>
                    {
                        anim.Hide(() =>
                        {
                            dailyCheckin.RecycleAll();
                            dailyCheckins.Clear();
                        });
                    }).SetId(gameObject);
                    break;
                case 1:
                    dailyCheckin.RecycleAll();
                    dailyCheckins.Clear();
                    ShowReward();
                    break;
            }

        }).SetId(gameObject);
    }

    public void Ins_FillData()
    {
        for (int i = 0; i < dailyTfs.Length; i++)
        {
            var daily = dailyCheckin.Spawn(contentTf, dailyTfs[i].localPosition);
            daily.FillData(DailyCheckinDataManager.DailyGiftDataAsset.list[i]);
            daily.transform.SetScale(0);
            dailyCheckins.Add(daily);
        }
    }

    private void ShowReward()
    {
        rewardIcon.SetAlpha(0);
        nameRewardTxt.transform.SetScale(0);
        amountRewardTxt.transform.SetScale(0);
        claimBtn.transform.SetScale(0);
        claimX2Btn.transform.SetScale(0);
        glowImg.transform.SetScale(1);
        glowImg.SetAlpha(0);
        lightImg.SetAlpha(0);
        lightImg.transform.localRotation = Quaternion.Euler(Vector3.zero);

        rewardIcon.sprite = DailyCheckinDataManager.DailyGiftDataAsset.list[DailyCheckinDataManager.dailyCheckinUserData.totalTimeClaimed].iconDailyGift;
        nameRewardTxt.text = DailyCheckinDataManager.DailyGiftDataAsset.list[DailyCheckinDataManager.dailyCheckinUserData.totalTimeClaimed].name;
        amountRewardTxt.text = DailyCheckinDataManager.DailyGiftDataAsset.list[DailyCheckinDataManager.dailyCheckinUserData.totalTimeClaimed].amount.ToString();

        ShowDg(congratulationImg.transform);

        glowImg.DOKill();
        glowImg.DOFade(1 , 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
        glowImg.transform.DOScale(1.01f, 0.75f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetId(gameObject);

        lightImg.DOKill();
        lightImg.DOFade(1, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
        lightImg.transform.DOLocalRotate(new Vector3(0, 0, -1), 0.1f).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear).SetId(gameObject);

        rewardIcon.DOFade(1, 0.25f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            ShowDg(nameRewardTxt.transform);
            ShowDg(amountRewardTxt.transform);

            ShowDg(claimBtn.transform, 1, 0.25f, 0.25f);
            ShowDg(claimX2Btn.transform, 1, 0.25f, 0.25f);

            adsImg.DOKill();
            adsImg.transform.SetScale(1);
            adsImg.transform.DOScale(1.025f, 0.75f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetId(gameObject);
        }).SetId(gameObject);
    }

    private void HideReward()
    {
        ShowDg(nameRewardTxt.transform, 0);
        ShowDg(amountRewardTxt.transform, 0);

        ShowDg(claimBtn.transform, 0);
        ShowDg(claimX2Btn.transform, 0);

        ShowDg(congratulationImg.transform, 0);

        glowImg.DOFade(0, 0.2f).SetEase(Ease.InOutSine).SetId(gameObject);
        lightImg.DOFade(0, 0.2f).SetEase(Ease.InOutSine).SetId(gameObject);

        rewardIcon.DOFade(0, 0.25f).SetEase(Ease.InOutSine).SetDelay(0.25f).OnComplete(() =>
        {
            fadeImg.DOFade(0, 0.5f).SetId(gameObject).OnComplete(() =>
            {
                anim.Hide(() =>
                {
                });
            }).SetId(gameObject);
        }).SetId(gameObject);
    }

    private void ShowDg(Transform obj, float To = 1, float time = 0.25f, float delayTime = 0)
    {
        obj.transform.DOScale(To, time).SetEase(Ease.InOutSine).SetDelay(delayTime).SetId(gameObject);
    }
}
