using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "LuckySpinAsset", menuName = "DataAsset/LuckySpinDataAsset")]
public class LuckySpinAsset : ScriptableObject
{
    public LuckySpinReward[] luckySpinRewards;

    [ButtonMethod]
    public void FillIndex()
    {
#if UNITY_EDITOR
        for (int i = 0; i > luckySpinRewards.Length; i++)
            luckySpinRewards[i].index = i;
        EditorUtility.SetDirty(this);
#endif
    }
}

[Serializable]
public class LuckySpinReward
{
    public int index;
    public int rewardAmount;
    public Color colorMain;
    public Sprite rewardSpriteIcon;
    public string nameBooster;
    public Color colorSub;
    public bool isSpecial;
}

