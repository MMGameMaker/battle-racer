using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementGiftBox : MonoBehaviour
{
    [SerializeField] Image imageOn, imageOff, imageClaimed, iconBox;
    [SerializeField] GameObject notice, tick;
    [SerializeField] Button claimBtn;

    private int index;

    private void Start()
    {
        claimBtn.onClick.RemoveAllListeners();
        claimBtn.onClick.AddListener(() =>
        {
            PopupAchievement.instance.ShowReward(index);
            ClaimData(index);
        });
    }

    public void FillData(StatuBox status, AchievmentRewardDetail detail, int id)
    {
        CheckStatus(status);
        index = id;
        iconBox.sprite = detail.iconBox;
    }

    public void CheckStatus(StatuBox status)
    {
        switch (status)
        {
            case StatuBox.isDisable:
                claimBtn.interactable = false;
                imageOff.enabled = true;
                imageClaimed.enabled = false;
                notice.SetActive(false);
                tick.SetActive(false);
                break;
            case StatuBox.isEnable:
                claimBtn.interactable = true;
                imageOff.enabled = false;
                imageClaimed.enabled = false;
                notice.SetActive(true);
                tick.SetActive(false);
                break;
            case StatuBox.isClaimed:
                claimBtn.interactable = false;
                imageOff.enabled = true;
                imageClaimed.enabled = true;
                notice.SetActive(false);
                tick.SetActive(true);
                break;
        }
    }

    public void ClaimData(int index)
    {
        switch (index)
        {
            case 0:
                AchievementDataManager.achivementUserData.isClaimedPopupAchievement1 = true;
                break;
            case 1:
                AchievementDataManager.achivementUserData.isClaimedPopupAchievement2 = true;
                break;
            case 2:
                AchievementDataManager.achivementUserData.isClaimedPopupAchievement3 = true;
                break;
        }

        AchievementDataManager.Save();
    }
}

public enum StatuBox
{
    isEnable,
    isDisable,
    isClaimed,
}
