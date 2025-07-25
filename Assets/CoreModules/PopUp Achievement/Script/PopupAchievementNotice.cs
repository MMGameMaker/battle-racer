using TMPro;
using MyBox;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PopupAchievementNotice : MonoBehaviour
{
    public static PopupAchievementNotice instance;
    public Image noticeImg;
    public Image mainIconImg;
    public TMP_Text nameTxt;
    public TMP_Text detailTxt;
    public Image circleImg;
    public Image subImg;

    private AchievementData achievementData;

    [SerializeField] UIAnimation anim;

    private void Awake()
    {
        instance = this;
    }

    [ButtonMethod]
    public void Show(TypeAchievement type)
    {
        DOVirtual.DelayedCall(1f, () =>
        {
            anim.Show(() =>
            {
                FillData(type);
            });
        }).SetId(gameObject);
    }

    [ButtonMethod]
    public void Hide()
    {
        nameTxt.gameObject.SetActive(false);
        detailTxt.gameObject.SetActive(false);
        mainIconImg.DOFade(0, 0.5f).OnComplete(() =>
        {
            noticeImg.transform.DOScaleX(0, 0.5f).OnComplete(() =>
            {
                anim.Hide();
            }).SetId(gameObject);
        }).SetId(gameObject);
        circleImg.DOFade(0, 0.5f).SetId(gameObject);
        subImg.DOFade(0, 0.5f).SetId(gameObject);
    }

    private void FillData(TypeAchievement t)
    {
        noticeImg.transform.SetScaleX(0);
        circleImg.SetAlpha(0);
        mainIconImg.SetAlpha(0);
        subImg.SetAlpha(0);
        nameTxt.gameObject.SetActive(false);
        detailTxt.gameObject.SetActive(false);
        achievementData = AchievementDataManager.AchievementDataAssets.GetData(t);
        mainIconImg.sprite = achievementData.iconAchievement;
        nameTxt.text = $"{achievementData.nameAchievements[achievementData.indexAchevement]}";
        detailTxt.text = $"{achievementData.conditionAchievements[achievementData.indexAchevement]}";

        noticeImg.transform.DOScaleX(1, 0.5f).OnComplete(() =>
        {
            circleImg.DOFade(1, 0.5f).OnComplete(() =>
            {
                nameTxt.gameObject.SetActive(true);
                detailTxt.gameObject.SetActive(true);
                mainIconImg.DOFade(1, 0.5f).OnComplete(() =>
                {
                    DOVirtual.DelayedCall(1.5f, () =>
                    {
                        Hide();
                    }).SetId(gameObject);
                }).SetId(gameObject);
            }).SetId(gameObject);
            subImg.DOFade(1, 0.5f).SetId(gameObject);
        }).SetId(gameObject);
    }
}
