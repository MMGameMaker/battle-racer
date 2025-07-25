using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using MyBox;

[CreateAssetMenu(fileName = "AchievementDataAssets", menuName = "DataAsset/AchievementDataAssets")]
public class AchievementDataAssets : BaseAsset<AchievementData>
{

    private int indexData = -1;
    private AchievementSaveData achievementSaveData = null;

    public new List<AchievementSaveData> itemSaveList
    {
        get => list.Select(x => new AchievementSaveData
        {
            id = x.id,
            index = x.index,
            currentAchievement = x.currentAchievement,
            indexAchevement = x.indexAchevement,
            isUnlocked = x.isUnlocked,
            isSelected = x.isSelected,
            isClaimed = x.isClaimed,
            count = x.count,
            unlockPay = x.unlockPay,
        }).ToList();
    }

    public void ConvertToData(List<AchievementSaveData> saveData)
    {
        foreach (var i in saveData)
        {
            var temp = list.FirstOrDefault(x => x.index == i.index);
            if (temp != null)
            {
                temp.count = i.count;
                temp.currentAchievement = i.currentAchievement;
                temp.indexAchevement = i.indexAchevement;
                temp.unlockPay = i.unlockPay;
                temp.isUnlocked = i.isUnlocked;
                temp.isClaimed = i.isClaimed;
                if (i.isSelected)
                    temp.isSelected = i.isSelected;
            }
        }
    }

    public int FindDataAchievement(TypeAchievement typeAchievement)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].typeAchievement == typeAchievement)
                return i;
        }
        return -1;
    }


    public void SetData(TypeAchievement type)
    {
        indexData = FindDataAchievement(type);
        achievementSaveData = list[indexData];
        if (!achievementSaveData.isClaimed && !achievementSaveData.isUnlocked)
        {
            if (achievementSaveData.currentAchievement < list[indexData].totalAchievements[achievementSaveData.indexAchevement])
                achievementSaveData.currentAchievement += 1;

            if (achievementSaveData.currentAchievement >= list[indexData].totalAchievements[achievementSaveData.indexAchevement])
            {
                PopupAchievementNotice.instance.Show(type);
                achievementSaveData.isClaimed = true;
            }
        }
    }

    public AchievementData GetData(TypeAchievement type)
    {
        indexData = FindDataAchievement(type);
        return list[indexData];
    }

    public override void ResetData()
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].isClaimed = false;
            list[i].isUnlocked = false;
            list[i].currentAchievement = 0;
            list[i].indexAchevement = 0;
        }
        base.ResetData();
    }

    [ButtonMethod]
    private void SetIndex()
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].index = i;
        }
    }
}

[Serializable]
public class AchievementData : AchievementSaveData
{
    [Header("Info")]
    public Sprite iconAchievement;
    public string[] nameAchievements;
    public int[] totalAchievements;
    public int[] rewards;
    public string[] conditionAchievements;
    public TypeAchievement typeAchievement;
}

[Serializable]
public class AchievementSaveData : SaveData
{
    [Header("Data")]
    public int indexAchevement = 0;
    public int currentAchievement = 0;
    public bool isClaimed;
}

[Serializable]
public enum TypeAchievement
{
    Crab,
    Shrimp,
    Cockles,
    Turtles,
    Star,
    Match,
    Fan,
    Vacuum,
    Shuffle,
    NeverGiveUp,
    NewbieWinner,
    LuckAddict,
    FirstInvestment,
    BountyHunter,
    LoyalKitty,
    HitX3,
    HitX4,
    HitX5,
    IceBreak
}
