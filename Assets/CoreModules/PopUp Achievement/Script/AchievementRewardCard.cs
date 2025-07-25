using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AchievementRewardCard : MonoBehaviour
{
    [SerializeField] Text nameRewardTxt;
    [SerializeField] Image rewardImg;
    [SerializeField] Text amountRewardTxt;

    public void FillData(AchievmentRewardDetail data)
    {
        nameRewardTxt.text = $"{data.nameReward}";
        rewardImg.sprite = data.iconReward;
        amountRewardTxt.text = $"{data.amountReward}";
    }
}
