using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementDataManager : MonoBehaviour
{
    [SerializeField]
    protected AchievementDataAssets achievementDataAssets = null;

    public static AchievementDataAssets AchievementDataAssets { get; private set; }

    public static AchievementUserData achivementUserData { get; private set; }

    public static AchievementDataManager instance { get; set; }

    public delegate void LoadedDelegate(AchievementUserData achievementUserData);

    public static event LoadedDelegate OnLoaded;

    public List<AchievmentRewardDetail> achievmentRewardDetails = new List<AchievmentRewardDetail>();

    [SerializeField] int[] ConfigStarClaimRewards;

    private void Awake()
    {
        instance = this;
    }

    public static IEnumerator DoLoad()
    {
        if (instance)
        {
            var elapsedTime = 0f;
            if (achivementUserData == null)
                Load();
            else
                Debug.LogWarning("achivementUserData not NULL");

            while (achivementUserData == null)
            {
                if (elapsedTime < 5)
                {
                    Debug.LogWarning("achivementUserData load " + elapsedTime.ToString("0.0"));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
        }
    }

    public static void Load()
    {
        var tempData = new AchievementUserData();

        if (AchievementDataAssets == null)
        {
            AchievementDataAssets = ScriptableObject.CreateInstance("AchievementDataAssets") as AchievementDataAssets;
            foreach (var i in instance.achievementDataAssets.list)
                AchievementDataAssets.list.Add(i);
        }
        else
            Debug.Log("AchievementDataAssets is not NULL");

        AchievementUserData loadData = FileExtend.LoadData<AchievementUserData>("AchievementUserData") as AchievementUserData;

        if (loadData != null)
        {
            if (loadData.achievementSaveDatas != null && loadData.achievementSaveDatas.Any())
            {
                AchievementDataAssets.ConvertToData(loadData.achievementSaveDatas);
            }
        }

        achivementUserData = tempData;
        if (achivementUserData != null && loadData != null)
            achivementUserData = loadData;

        OnLoaded?.Invoke(achivementUserData);
    }

    public static void Save()
    {
        {
            if (instance && achivementUserData != null)
            {
                var time = DateTime.Now;
                achivementUserData.achievementSaveDatas = AchievementDataAssets.itemSaveList;
                FileExtend.SaveData<AchievementUserData>("AchievementUserData", achivementUserData);
                Debug.Log("AchievementUserData in " + (DateTime.Now - time).TotalMilliseconds + "ms");
            }
        }
    }

    public static int GetStarClaimReward(int index)
    {
        if (index < instance.ConfigStarClaimRewards.Length)
            return instance.ConfigStarClaimRewards[index];
        else
            return 0;
    }

    [ButtonMethod]
    public void ResetDataAchievement()
    {
        achievementDataAssets.ResetData();
        var path = FileExtend.FileNameToPath("AchievementUserData.gd");
        FileExtend.DeleteFile(path);
    }
}

[Serializable]
public class AchievementUserData
{
    private int starAchievement;

    public int starAchievementUser
    {
        get => starAchievement;
        set
        {
            if (starAchievement != value)
            {
                int changed = 0;
                if (starAchievement > value)
                {
                    changed = starAchievement - value;
                    starAchievementUserSpend += changed;
                }
                else
                {
                    changed = value - starAchievement;
                    starAchievementUserEarn += changed;
                }

                starAchievement = value;
                OnstarAchievementUserChanged?.Invoke(changed, starAchievement);
            }
        }
    }

    public int starAchievementUserEarn = 0;
    public int starAchievementUserSpend = 0;
    public static event CurrentValueChangedDelegate OnstarAchievementUserChanged;
    public delegate void CurrentValueChangedDelegate(int change, int current);

    public bool isClaimedPopupAchievement1 = false;
    public bool isClaimedPopupAchievement2 = false;
    public bool isClaimedPopupAchievement3 = false;
    public List<AchievementSaveData> achievementSaveDatas = new List<AchievementSaveData>();
}

[Serializable]
public class AchievmentRewardDetail
{
    [Header("Reward")]
    public string nameReward;
    public Sprite iconReward;
    public float amountReward;
    public Sprite iconBox;
}


