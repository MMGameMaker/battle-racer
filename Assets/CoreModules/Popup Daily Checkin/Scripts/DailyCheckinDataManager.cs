using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DailyCheckinDataManager : MonoBehaviour
{
    [SerializeField] protected DailyGiftDataAsset dailyGiftDataAsset = null;

    public static DailyGiftDataAsset DailyGiftDataAsset { get; private set; }

    public static DailyCheckinUserData dailyCheckinUserData { get; private set; }
    private static DailyCheckinDataManager instance { get; set; }

    public delegate void LoadedDelegate(DailyCheckinUserData dailyCheckinUserData);

    public static event LoadedDelegate OnLoaded;

    private void Awake()
    {
        instance = this;
    }

    public static IEnumerator DoLoad()
    {
        if (instance)
        {
            var elapsedTime = 0f;
            if (dailyCheckinUserData == null)
                Load();
            else
                Debug.LogWarning("dailyCheckinUserData not NULL");

            while (dailyCheckinUserData == null)
            {
                if (elapsedTime < 5)
                {
                    Debug.LogWarning("dailyGiftDataAsset load " + elapsedTime.ToString("0.0"));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
        }
    }

    public static void Load()
    {
        var tempData = new DailyCheckinUserData();

        if (DailyGiftDataAsset == null)
        {
            DailyGiftDataAsset = ScriptableObject.CreateInstance("DailyGiftDataAsset") as DailyGiftDataAsset;
            foreach (var i in instance.dailyGiftDataAsset.list)
                DailyGiftDataAsset.list.Add(i);
        }
        else
            Debug.Log("DailyGiftDataAsset is not NULL");

        DailyCheckinUserData loadData = FileExtend.LoadData<DailyCheckinUserData>("DailyCheckinUserData") as DailyCheckinUserData;

        if (loadData != null)
        {
            if (loadData.dailyGiftSaveData != null && loadData.dailyGiftSaveData.Any())
            {
                DailyGiftDataAsset.ConvertToData(loadData.dailyGiftSaveData);
            }
        }

        dailyCheckinUserData = tempData;
        if (dailyCheckinUserData != null && loadData != null)
            dailyCheckinUserData = loadData;

        OnLoaded?.Invoke(dailyCheckinUserData);
    }

    public static void Save()
    {
        {
            if (instance && dailyCheckinUserData != null)
            {
                var time = DateTime.Now;
                dailyCheckinUserData.dailyGiftSaveData = DailyGiftDataAsset.itemSaveList;
                dailyCheckinUserData.LastTimeCheckinUpdate = DateTime.Now;
                FileExtend.SaveData<DailyCheckinUserData>("DailyCheckinUserData", dailyCheckinUserData);
                Debug.Log("DailyCheckinUserData in " + (DateTime.Now - time).TotalMilliseconds + "ms");
            }
        }
    }

    [ButtonMethod]
    private void ClearData()
    {
        var path = FileExtend.FileNameToPath("DailyCheckinUserData.gd");
        FileExtend.DeleteFile(path);
        Debug.Log("Reset Daily Checkin Data");
        dailyGiftDataAsset.ResetData();
    }
}

[Serializable]
public class DailyCheckinUserData
{
    private string lastTimeCheckinUpdate = new DateTime(1999, 1, 1).ToString();

    public DateTime LastTimeCheckinUpdate
    {
        get => DateTimeConverter.ToDateTime(lastTimeCheckinUpdate);
        set => lastTimeCheckinUpdate = value.ToString();
    }

    public int totalTimeClaimed = 0;

    public List<DailyGiftSaveData> dailyGiftSaveData = new List<DailyGiftSaveData>();
}
