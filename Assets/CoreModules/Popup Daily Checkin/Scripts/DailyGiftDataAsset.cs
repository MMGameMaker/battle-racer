using MyBox;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DailyGiftDataAsset", menuName = "DataAsset/DailyGiftDataAsset")]
public class DailyGiftDataAsset : BaseAsset<DailyGiftData>
{

    [SerializeField] Sprite[] thumbnailDailys;

    private int totalDayCheckin = 7;

    public new List<DailyGiftSaveData> itemSaveList
    {
        get => list.Select(x => new DailyGiftSaveData
        {
            id = x.id,
            index = x.index,
            isUnlocked = x.isUnlocked,
            isSelected = x.isSelected,
            isDailyClaimed = x.isDailyClaimed,
            count = x.count,
            unlockPay = x.unlockPay,
        }).ToList();
    }

    public void ConvertToData(List<DailyGiftSaveData> saveData)
    {
        foreach (var i in saveData)
        {
            var temp = list.FirstOrDefault(x => x.index == i.index);
            if (temp != null)
            {
                temp.count = i.count;
                temp.unlockPay = i.unlockPay;
                temp.isUnlocked = i.isUnlocked;
                temp.isDailyClaimed = i.isDailyClaimed;
            }
        }
    }

    public override void ResetData()
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].isDailyClaimed = false;
        }
        base.ResetData();
    }

    [ButtonMethod]
    private void CreateData()
    {
        list.Clear();
        for (int i = 0; i < totalDayCheckin; i++)
        {
            var day = new DailyGiftData();
            list.Add(day);
        }
    }

    [ButtonMethod]
    private void FillData()
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].name = $"Day {i+1}";
            list[i].id = $"Day {i+1}";
            list[i].index = i;
            list[i].iconDailyGift = thumbnailDailys[i];
        }
    }
}

[Serializable]
public class DailyGiftData : DailyGiftSaveData
{
    [Header("Info")]
    public Sprite iconDailyGift;
    public int amount;
}

[Serializable]
public class DailyGiftSaveData : SaveData
{
    [Header("Data")]
    public bool isDailyClaimed;
}
