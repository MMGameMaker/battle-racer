using DG.Tweening;
using MyBox;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupAchievement : MonoBehaviour
{

    public static PopupAchievement instance;
    public Transform startTf;
    [SerializeField] UIAnimation anim;

    [Header("Achievement")]
    [SerializeField] RectTransform content;
    [SerializeField] AchievementCard card;
    [SerializeField] List<AchievementCard> achievementCards = new List<AchievementCard>();
    [SerializeField] Image totalStarFill;
    [SerializeField] float elapsedTime, minutes, seconds, hours;
    [SerializeField] TMP_Text timeTxt;
    [SerializeField] Button achievementBtn;
    [SerializeField] Button closeBtn;
    [SerializeField] TMP_Text starCurrentValue;
    [SerializeField] AchievementGiftBox achievementGiftBox;
    [SerializeField] List<AchievementGiftBox> achievementGiftBoxs = new List<AchievementGiftBox>();
    [SerializeField] RectTransform[] giftBoxTfs;
    [SerializeField] RectTransform giftBoxContentTf;
    [SerializeField] CanvasGroup achievementCanvasGroup;
    [SerializeField] ScrollRect scrollRect;

    private int totalAchievementStar = 100;

    [Header("Reward")]
    [SerializeField] AchievementRewardCard achievementRewardCard;
    [SerializeField] Button x2RewardBtn;
    [SerializeField] Button claimRewardBtn;
    [SerializeField] string[] boxSkins;
    [SerializeField] SkeletonGraphic boxSkeletonGraphic;
    [SerializeField] RectTransform achievementRewardContentTf;
    [SerializeField] RectTransform achievementRewardSpawnTf;
    [SerializeField] RectTransform achievementRewardEndTf;
    [SerializeField] CanvasGroup rewardCanvasGroup;
    private string loopAnimationName = "loop";
    private string openAnimationName = "open";

    private void Awake()
    {
        instance = this;
        card.CreatePool(18);
        achievementGiftBox.CreatePool(1);
        achievementRewardCard.CreatePool(1);
    }

    private void OnDisable()
    {
        DOTween.Kill(gameObject);
    }

    private void Start()
    {
        //achievementBtn.onClick.RemoveAllListeners();
        //achievementBtn.onClick.AddListener(() =>
        //{
        //    SoundManager.Play(SoundHelper.ButtonClick);
        //    Show();
        //});
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            SoundManager.Play(SoundHelper.ButtonClick);
            Hide();
        });

        x2RewardBtn.onClick.RemoveAllListeners();
        x2RewardBtn.onClick.AddListener(() =>
        {
            Hide();
        });

        claimRewardBtn.onClick.RemoveAllListeners();
        claimRewardBtn.onClick.AddListener(() =>
        {
            SoundManager.Play(SoundHelper.ButtonClick);
            Hide();
        });
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        minutes = Mathf.FloorToInt(elapsedTime / 60);
        seconds = Mathf.FloorToInt(elapsedTime % 60);
        hours = Mathf.FloorToInt(elapsedTime / 3600);
        timeTxt.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    [ButtonMethod]
    public void Show()
    {
        anim.Show(() =>
        {
            closeBtn.transform.SetScale(0);
            achievementCanvasGroup.alpha = 1;
            rewardCanvasGroup.alpha = 0;
            achievementCanvasGroup.blocksRaycasts = true;
            rewardCanvasGroup.blocksRaycasts = false;
            starCurrentValue.text = AchievementDataManager.achivementUserData.starAchievementUser.ToString();
            totalStarFill.fillAmount = AchievementDataManager.achivementUserData.starAchievementUser * 1f / totalAchievementStar * 1f; ;
            SpawnAchievementCard();
            SpawnGiftBox();
        });
    }

    [ButtonMethod]
    public void Hide()
    {
        anim.Hide(() =>
        {
            card.RecycleAll();
            achievementCards.Clear();
            achievementRewardCard.RecycleAll();
            achievementGiftBox.RecycleAll();
            achievementGiftBoxs.Clear();
        });
    }

    [ButtonMethod]
    public void ShowReward(int id)
    {
        ShowCloseButton(0);
        achievementCanvasGroup.DOFade(0, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            card.RecycleAll();
            achievementCards.Clear();
            rewardCanvasGroup.alpha = 1;
            rewardCanvasGroup.blocksRaycasts = true;
            x2RewardBtn.transform.SetScale(0);
            claimRewardBtn.transform.SetScale(0);

            boxSkeletonGraphic.initialSkinName = boxSkins[id];
            boxSkeletonGraphic.Initialize(true);

            IEnumerator YieldShowReward()
            {
                boxSkeletonGraphic.AnimationState.SetAnimation(0, openAnimationName, false).Complete += BoxShowRewardComplete;
                yield return new WaitForSeconds(1f);
                var card = achievementRewardCard.Spawn(achievementRewardContentTf, achievementRewardSpawnTf.localPosition);
                card.FillData(AchievementDataManager.instance.achievmentRewardDetails[id]);
                card.transform.SetScale(0.75f);
                card.transform.DOScale(1.025f, 0.1f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    card.transform.DOScale(0.85f, 0.1f).SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        card.transform.DOScale(1, 0.1f).SetEase(Ease.InOutSine).OnComplete(() =>
                        {
                            x2RewardBtn.transform.DOScale(1, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
                            claimRewardBtn.transform.DOScale(1, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
                            ShowCloseButton();
                        }).SetId(gameObject);
                    }).SetId(gameObject);
                }).SetId(gameObject);
                card.transform.DOLocalMove(achievementRewardEndTf.localPosition, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
            }

            StartCoroutine(YieldShowReward());
        }).SetId(gameObject);
    }

    private void BoxShowRewardComplete(Spine.TrackEntry trackEntry)
    {
        boxSkeletonGraphic.AnimationState.SetAnimation(0, loopAnimationName, true);
    }

    private void SpawnAchievementCard()
    {
        IEnumerator YieldSpawnAchivementCard()
        {
            for (int i = 0; i < AchievementDataManager.AchievementDataAssets.list.Count; i++)
            {
                var data = AchievementDataManager.AchievementDataAssets.list[i];
                if (data.isClaimed)
                {
                    var cardClaimed = card.Spawn(content);
                    cardClaimed.starPs.SetTargetTransform = startTf;
                    cardClaimed.FillData(data);
                    achievementCards.Add(cardClaimed);
                    cardClaimed.gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < AchievementDataManager.AchievementDataAssets.list.Count; i++)
            {
                var data = AchievementDataManager.AchievementDataAssets.list[i];
                if (!data.isUnlocked && !data.isClaimed)
                {
                    var cardLock = card.Spawn(content);
                    cardLock.FillData(data);
                    achievementCards.Add(cardLock);
                    cardLock.gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < AchievementDataManager.AchievementDataAssets.list.Count; i++)
            {
                var data = AchievementDataManager.AchievementDataAssets.list[i];
                if (data.isUnlocked && !data.isClaimed)
                {
                    var cardUnlock = card.Spawn(content);
                    cardUnlock.FillData(data);
                    achievementCards.Add(cardUnlock);
                    cardUnlock.gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < achievementCards.Count; i++)
            {
                achievementCards[i].id = i;
                achievementCards[i].gameObject.SetActive(true);
                achievementCards[i].transform.localScale = Vector3.one * 0.75f;
                achievementCards[i].transform.DOScale(Vector3.one * 1.05f, 0.1f).OnComplete(() =>
                {
                    achievementCards[i].transform.DOScale(Vector3.one, 0.1f).SetId(gameObject);
                }).SetId(gameObject);
                yield return new WaitForSeconds(0.15f);

                ScrollToBottom();

            }

            yield return new WaitForSeconds(0.1f);

            ShowButton(closeBtn.transform);

            for (int i = 0; i < achievementGiftBoxs.Count; i++)
            {
                ShowButton(achievementGiftBoxs[i].transform);
            }
        }

        StartCoroutine(YieldSpawnAchivementCard());
    }

    public void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    [ButtonMethod]
    public void ResetPositionAchievementCard(int id)
    {
        StartCoroutine(YieldResetPositionAchievementCard());
        IEnumerator YieldResetPositionAchievementCard()
        {

            for (int i = id + 1; i < achievementCards.Count; i++)
            {
                achievementCards[i].MoveStepTwo(achievementCards[i - 1].transform);
            }

            yield return new WaitForSeconds(0.6f);

            for (int i = 0; i < achievementCards.Count; i++)
            {
                achievementCards[i].MoveStepThree();
            }

            achievementCards[id].gameObject.SetActive(false);

            achievementCards.RemoveAt(id);

            yield return new WaitForSeconds(0.15f);

            ShowCloseButton();
        }
    }

    public void FillData(int star)
    {
        StartCoroutine(YieldFillProgreeBar());
        AchievementDataManager.achivementUserData.starAchievementUser += star;
        IEnumerator YieldFillProgreeBar()
        {
            yield return new WaitForSeconds(0.85f);

            for (int i = 0; i < star; i++)
            {
                totalStarFill.fillAmount = (AchievementDataManager.achivementUserData.starAchievementUser * 1f) / totalAchievementStar * 1f;
                yield return new WaitForSeconds(0.1f);
            }

            starCurrentValue.text = AchievementDataManager.achivementUserData.starAchievementUser.ToString();

            CheckStatusGiftBox();

            AchievementDataManager.Save();
        }
    }



    public void SpawnGiftBox()
    {
        for (int i = 0; i < AchievementDataManager.instance.achievmentRewardDetails.Count; i++)
        {
            var achieGiftBox = achievementGiftBox.Spawn(giftBoxContentTf, giftBoxTfs[i].localPosition);
            achieGiftBox.transform.SetScale(0);
            achievementGiftBoxs.Add(achieGiftBox);
            achieGiftBox.FillData(HasClaimedAchievement(i) ? StatuBox.isClaimed :
            AchievementDataManager.achivementUserData.starAchievementUser >=
            AchievementDataManager.GetStarClaimReward(i) ? StatuBox.isEnable : StatuBox.isDisable,
            AchievementDataManager.instance.achievmentRewardDetails[i],
            i);
        }
    }

    public void CheckStatusGiftBox()
    {
        for (int i = 0; i < achievementGiftBoxs.Count; i++)
        {
            achievementGiftBoxs[i].CheckStatus(HasClaimedAchievement(i) ? StatuBox.isClaimed :
            AchievementDataManager.achivementUserData.starAchievementUser >=
            AchievementDataManager.GetStarClaimReward(i) ? StatuBox.isEnable : StatuBox.isDisable);
        }
    }

    private bool HasClaimedAchievement(int index)
    {
        switch (index)
        {
            case 0:
                return AchievementDataManager.achivementUserData.isClaimedPopupAchievement1;
            case 1:
                return AchievementDataManager.achivementUserData.isClaimedPopupAchievement2;
            case 2:
                return AchievementDataManager.achivementUserData.isClaimedPopupAchievement3;
        }
        return false;
    }

    public void ShowCloseButton(float fade = 1, float time = 0.25f, float delayTime = 0)
    {
        closeBtn.transform.DOScale(fade, time).SetEase(Ease.InOutSine).SetDelay(delayTime).SetId(gameObject);
    }

    public void ShowButton(Transform obj)
    {
        obj.DOScale(1.1f, 0.15f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            obj.DOScale(0.9f, 0.15f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                obj.DOScale(1, 0.1f).SetEase(Ease.InOutSine).SetId(gameObject);
            }).SetId(gameObject);
        }).SetId(gameObject);
    }

    [Header("Test")]
    [SerializeField] int testStars = 10;
    [ButtonMethod]
    public void TestAddStar()
    {
        FillData(testStars);
    }

    [ButtonMethod]
    public void TestClaimAchievement()
    {
        AchievementDataManager.AchievementDataAssets.SetData(TypeAchievement.Crab);
        AchievementDataManager.Save();
    }
}
