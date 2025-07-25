using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LuckySpinContent : MonoBehaviour
{
    [SerializeField] Image rewardImg;
    [SerializeField] Text amountTxt;
    [SerializeField] Image mainImg;
    [SerializeField] Image subImg;

    public CanvasGroup canvasGroup;

    public void FillData(LuckySpinReward data)
    {
        rewardImg.sprite = data.rewardSpriteIcon;
        amountTxt.text = data.rewardAmount.ToString();
        mainImg.color = data.colorMain;
        subImg.color = data.colorSub;
    }

}
