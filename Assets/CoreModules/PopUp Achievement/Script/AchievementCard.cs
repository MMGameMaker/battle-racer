using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using MyBox;
using TMPro;

public class AchievementCard : MonoBehaviour
{
    public int id;
    public ParticleLockAt starPs;
    public ParticleSystem starBurstPs;

    [SerializeField] Button claimBtn;
    [SerializeField] Button moveBtn;
    [SerializeField] RectTransform leftPointTf;
    [SerializeField] RectTransform rightPointTf;
    [SerializeField] RectTransform childTf;
    [SerializeField] Image background;
    [SerializeField] Image mainIcon;
    [SerializeField] TMP_Text mainTxt;
    [SerializeField] Image rewardIcon;
    [SerializeField] TMP_Text rewardTxt;
    [SerializeField] TMP_Text progressTxt;
    [SerializeField] Image progressFill;
    [SerializeField] Image frameIcon;
    [SerializeField] GameObject tickGo;
    [SerializeField] GameObject youGotRewardGo;
    [SerializeField] Sprite bgComplete;
    [SerializeField] Sprite bgDone;
    [SerializeField] Sprite bgUncomplete;
    [SerializeField] Sprite frameComplete;
    [SerializeField] Sprite frameDone;
    [SerializeField] Sprite frameUnDone;
    [SerializeField] TMP_Text claimBtnTxt;

    [SerializeField] Image mainBtnImg;
    [SerializeField] Sprite claimSp;
    [SerializeField] Sprite notClaimSp;

    private AchievementData achievementData;
    private int star;
    private bool isClaimed;

    private void OnEnable()
    {
        isClaimed = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        claimBtn.onClick.RemoveAllListeners();
        claimBtn.onClick.AddListener(() =>
        {
            SoundManager.Play(SoundHelper.ButtonClick);
            OnClaimed();
        });
        moveBtn.onClick.RemoveAllListeners();
        moveBtn.onClick.AddListener(() => { OnMove(); });
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ButtonMethod]
    public void MoveStepOne()
    {
        childTf.transform.DOLocalMove(leftPointTf.localPosition, 0.25f).OnComplete(() =>
        {
            childTf.transform.DOLocalMove(rightPointTf.localPosition, 0.25f).OnComplete(() =>
            {
                PopupAchievement.instance.ResetPositionAchievementCard(id);
            }).SetId(gameObject);
        }).SetId(gameObject);
    }

    [ButtonMethod]
    public void MoveStepTwo(Transform tf)
    {
        var posY = Mathf.Abs(tf.localPosition.y - transform.localPosition.y);
        childTf.transform.DOLocalMoveY(posY, 0.5f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            id -= 1;
        }).SetId(gameObject);
    }

    public void MoveStepThree()
    {
        childTf.anchoredPosition = Vector2.zero;
    }

    public void FillData(AchievementData data)
    {
        achievementData = data;
        mainIcon.sprite = achievementData.iconAchievement;
        star = achievementData.rewards[achievementData.isUnlocked ? achievementData.indexAchevement - 1 : achievementData.indexAchevement];
        rewardTxt.text = $"{star}";
        frameIcon.sprite = frameUnDone;
        if (achievementData.isClaimed || achievementData.isUnlocked)
        {
            childTf.GetComponent<Image>().sprite = achievementData.isUnlocked ? bgDone : achievementData.isClaimed ? bgComplete : bgUncomplete;
            frameIcon.sprite = achievementData.isUnlocked ? frameDone : frameComplete;
            mainTxt.text = $"{achievementData.conditionAchievements[achievementData.isUnlocked ? achievementData.indexAchevement - 1 : achievementData.indexAchevement]}";
            progressFill.fillAmount = 1;
            var check = achievementData.totalAchievements[achievementData.isUnlocked ? achievementData.indexAchevement - 1 : achievementData.indexAchevement];
            progressTxt.text = $"{check}/{check}";
        }
        else
        {

            childTf.GetComponent<Image>().sprite = bgUncomplete;
            mainTxt.text = $"{achievementData.conditionAchievements[achievementData.indexAchevement]}";
            progressFill.fillAmount = achievementData.currentAchievement * 1f / achievementData.totalAchievements[achievementData.indexAchevement] * 1f;
            progressTxt.text = $"{achievementData.currentAchievement}/{achievementData.totalAchievements[achievementData.indexAchevement]}";
        }

        claimBtn.gameObject.SetActive(true);
        var checkInteract = !achievementData.isUnlocked && achievementData.isClaimed;
        claimBtn.interactable = checkInteract;
        mainBtnImg.sprite = checkInteract ? claimSp : notClaimSp;
        claimBtnTxt.text = !achievementData.isUnlocked && achievementData.isClaimed ? "Claim" : "Move";
        claimBtn.gameObject.SetActive(!achievementData.isUnlocked);
        tickGo.SetActive(achievementData.isUnlocked);
        youGotRewardGo.SetActive(achievementData.isUnlocked);
        rewardIcon.gameObject.SetActive(!achievementData.isUnlocked);
        rewardTxt.gameObject.SetActive(!achievementData.isUnlocked);
    }

    [ButtonMethod]
    private void OnClaimed()
    {
        if (isClaimed) return;
        isClaimed = true;
        PopupAchievement.instance.ShowCloseButton(0);
        starBurstPs.Play();
        DOVirtual.DelayedCall(0.65f, () =>
        {

            starPs.EmitLocalPosition(5, rewardIcon.transform, PopupAchievement.instance.startTf);
            PopupAchievement.instance.FillData(star);
            DOVirtual.DelayedCall(1.25f, () =>
            {
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    achievementData.indexAchevement += 1;
                    if (achievementData.indexAchevement >= achievementData.totalAchievements.Length)
                    {
                        achievementData.isClaimed = false;
                        achievementData.isUnlocked = true;
                    }
                    else
                        achievementData.isClaimed = false;

                    AchievementDataManager.Save();
                    DOVirtual.DelayedCall(0.1f, () =>
                    {
                        MoveStepOne();
                    }).SetId(gameObject);
                }).SetId(gameObject);
            }).SetId(gameObject);

        }).SetId(gameObject);
    }

    private void OnMove()
    {
        PopupAchievement.instance.Hide();
    }
}
