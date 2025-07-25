using System;
using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

[ExecuteInEditMode]
public class DataManager : MonoBehaviour
{
    #region STATIC
    public static GameConfig GameConfig => instance.configAsset.gameConfig;
    public static UserData UserData
    {
        get { return gameData?.user; }
    }
    public static GameData gameData { get; private set; }
    private static DataManager instance { get; set; }
    #endregion

    [Space(10)]
    [Header("Default Data")]
    [SerializeField]
    protected ConfigAsset configAsset = null;

    [Header("GameData auto SAVE LOAD")]
    [SerializeField]
    protected bool saveOnPause = true;
    [SerializeField]
    protected bool saveOnQuit = true;

    public delegate void LoadedDelegate(GameData gameData);
    public static event LoadedDelegate OnLoaded;

    #region BASE
    private void Awake()
    {
        instance = this;
    }

    public static void Save(bool saveCloud = false)
    {
        if (instance && gameData != null && gameData.user != null)
        {
            DoSave();

            if (saveCloud)
            {
                //Save cloud in here;
                Debug.Log("Save cloud is not implement!");
            }
        }
    }

    private static async UniTask DoSave()
    {
        gameData.user.LastTimeUpdate = DateTime.Now;
        await UniTask.Yield();
        var stringData = JsonConvert.SerializeObject(gameData);
        await UniTask.Yield();
        PlayerPrefs.SetString("ZenGameData", stringData);
        Debug.Log("SaveData");
    }

    public static IEnumerator DoLoad()
    {
        if (instance)
        {
            var elapsedTime = 0f;
            if (gameData == null)
                Load();
            else
                Debug.LogWarning("GameData not NULL");

            while (gameData == null)
            {
                if (elapsedTime < 5)
                {
                    Debug.LogWarning("GameData load " + elapsedTime.ToString("0.0"));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
        }
    }

    public static void Load()
    {
        var time = DateTime.Now;
        if (instance)
        {
            //Create default
            var tempData = new GameData();

            //Load gamedata
            GameData loadData = null;

            string stringData = PlayerPrefs.GetString("ZenGameData", string.Empty);
            Debug.Log($"string data: {stringData}");
            if (string.IsNullOrEmpty(stringData))
                loadData = FileExtend.LoadData<GameData>("GameData") as GameData;
            else
                try
                {
                    loadData = JsonConvert.DeserializeObject<GameData>(stringData);
                }
                catch (Exception E)
                {
                    Debug.Log($"Conver string data error: {E.Message}");
                }
            
            if (loadData != null)
            {
                if (loadData.user != null)
                {
                    tempData.user = loadData.user;
                    if ((DateTime.Now - tempData.user.LastTimeUpdate).TotalSeconds >= 15 * 60)
                        tempData.user.Session++;
                    if (tempData.user.VersionInstall == 0)
                        tempData.user.VersionInstall = UIManager.BundleVersion;
                    tempData.user.VersionCurrent = UIManager.BundleVersion;
                }
            }
            else
            {
                tempData.user.Session++;
                Debug.Log("CreateData in " + (DateTime.Now - time).TotalMilliseconds + "ms");
                if (tempData.user.FirstTimeOpenApp == new DateTime(1999, 1, 1))
                    tempData.user.FirstTimeOpenApp = DateTime.Now;
            }
            gameData = tempData;
        }
        else
        {
            throw new Exception("Data Manager instance is NULL. Maybe it hasn't been created.");
        }
        OnLoaded?.Invoke(gameData);
    }

    public void Reset()
    {
        var path = FileExtend.FileNameToPath("GameData.gd");
        FileExtend.DeleteFile(path);
        PlayerPrefs.DeleteAll();
        Debug.Log("Reset game data");
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause && !GameStateManager.isBusy && saveOnPause)
            Save(false);
    }
    private void OnApplicationQuit()
    {
        if (saveOnQuit)
            Save(true);
    }
    public void ResetAndUpdateData()
    {
        try
        {
            Reset();
            Debug.Log("Reset and Update data to BUILD!!!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Please update and save DATA before build!!!");
            Debug.LogException(ex);
        }
    }
    #endregion

    #region KEY_VALUE
    private static bool useXor => !string.IsNullOrEmpty(ZenUtls.XORKey);
    public static async void SaveKVData(UpdateKVDT kvData)
    {
        try
        {
            foreach (var kv in kvData.pairs)
            {
                var encryp = useXor ? ZenUtls.StringToBase64(kv.value) : kv.value;
                PlayerPrefs.SetString($"{kv.key}", encryp);
            }
            PlayerPrefs.Save();

#if USE_CLOUD
            var checkJSON = await ZenBackendManager.ActionOnGameAsync(ZenUtls.AccessToken
            , GameActionTypeEnum.UpdateKV, kvData);
            var check = JsonConvert.DeserializeObject<LogEventDTO>(checkJSON);
            if (!string.IsNullOrEmpty(check.error))
            {
                return;
            }
            ZenBackendManager.Log($"KV_DataSync_{checkJSON.Contains("true")}", JsonConvert.SerializeObject(kvData));
#endif
        }
        catch (Exception ex)
        {

        }
    }
    public static async UniTask<List<GetKVDataDTO>> GetKVData(List<string> dataKeys)
    {
        List<GetKVDataDTO> result = new();

        foreach (var kv in dataKeys)
        {
            var localDatum = PlayerPrefs.GetString(kv.ToString(), string.Empty);
            result.Add(new GetKVDataDTO()
            {
                key = kv,
                value = localDatum
            });
        }
#if USE_CLOUD
        try
        {
            var encriptedDataJson = await ZenBackendManager.GetUserKVJson(ZenUtls.AccessToken, dataKeys);

            ZenUtls.Log($"Ins_KV Data -> {encriptedDataJson}");
            var encripJson = JsonUtility.FromJson<GetKVDataDTOListAPIResponse>(encriptedDataJson);
            ZenUtls.Log($"Ins_KV_Encrypted -> {JsonConvert.SerializeObject(encripJson)}");
            if (encripJson == null || encripJson.data == null || !string.IsNullOrEmpty(encripJson.error))
            {
                var msg = string.IsNullOrEmpty(encripJson.message) ? "Server is maintaining." : encripJson.message;
                return null;
            }
            result =  ZenUtls.Base64XorToObject<List<GetKVDataDTO>>(encripJson.data);
        }
        catch (Exception ex)
        {
            ZenUtls.Log("Cannot get kv data -> " + ex.Message);
        }
#endif
        return result;
    }
    #endregion
}

[Serializable]
public class UpdateKVDT
{
    public List<KVDataDTO> pairs = new List<KVDataDTO>();
}
[Serializable]
public class KVDataDTO
{
    public string key;
    public string value;
}
[Serializable]
public class GetKVDataDTOListAPIResponse
{
    public string message;
    public string error;
    public string data;
}
[Serializable]
public class GetKVDataDTO
{
    public string key;
    public object value;
}
[Serializable]
public class KVDataObjec<T>
{
    public T data;
}