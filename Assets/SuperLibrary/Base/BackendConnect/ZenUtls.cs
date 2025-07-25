using Best.HTTP.JSON.LitJson;
using MyBox;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ZenUtls : MonoBehaviour
{
    public static bool IsTestnet => instance.isTestnet;
    public static string URLApi => instance.isTestnet ? instance.urlApiTest : instance.urlApiProd;
    public static string URLHub => instance.isTestnet ? instance.urlHubTest : instance.urlHubProd;
    public static string URLToken => instance.isTestnet ? instance.urlTokenTest : instance.urlTokenProd;
    public static string XORKey => instance.isTestnet ? instance.xorKeyTest : instance.xorKeyProd;
    public static string URLManifest => instance.isTestnet ? instance.urlManifestTest : instance.urlManifestProd;
    public static int IntervalForAutoGetToken => instance.intervalForAutoGetToken;
    
    public static string AccessToken
    {
        get => instance.accessToken;
        set => instance.accessToken = value;
    }
    public static string SessionKey
    {
        get => instance.sessionKey;
        set => instance.sessionKey = value;
    }

    public static void Log(string content)
    {
#if UNITY_EDITOR
        Debug.Log(content);
#else
        if (instance.isTestnet)
            Debug.Log(content);
#endif
    }

    [Header("Product")]
    [SerializeField] string urlTokenProd = "https://id-dev.nakame.social/connect/token";
    [SerializeField] string urlApiProd = "https://dev-lethal.cairo.farm";
    [SerializeField] string urlHubProd = "https://dev-lethal.cairo.farm";
    [SerializeField] string xorKeyProd = "Ud00I7brYNB0801bLz";
    [SerializeField] string urlManifestProd = "https://play-lethal-dev.cairo.farm/tonconnect-manifest.json";

    [Header("Test")]
    [SerializeField] string urlTokenTest = "https://id-dev.nakame.social/connect/token";
    [SerializeField] string urlApiTest = "https://dev-lethal.cairo.farm";
    [SerializeField] string urlHubTest = "https://dev-lethal.cairo.farm";
    [SerializeField] string xorKeyTest = "Ud00I7brYNB0801bLz";
    [SerializeField] string urlManifestTest = "https://zendius-signin-demo.firebaseapp.com/tonconnect-manifest.json";

    [Header("Other")]
    [SerializeField] string contentBot = "Hey there! Ever tried Cat And Aliens? 🐱💰 It's a blast! Wanna team up and take for treasures together?";
    [SerializeField] int intervalForAutoGetToken = 3000;
    [Header("Debug")]
    [SerializeField] bool isTestnet = false;
    [SerializeField] bool isTestRunLocal = false;
    
    [SerializeField, ReadOnly] string accessToken = "";
    [SerializeField, ReadOnly] string sessionKey = "";

    private static ZenUtls instance;

    private void Awake()
    {
        instance = this;
    }
    public static T Base64XorToObject<T>(string plaintext)
    {
        try
        {
            var base64DecodedBytes = System.Convert.FromBase64String(plaintext);
            var xByte = XorData(base64DecodedBytes, XORKey);
            byte[] trimedByte = xByte.Skip(8).ToArray();
            var jsonData = System.Text.Encoding.UTF8.GetString(trimedByte);
            Log($"Base64Xor To JsonData: {jsonData}");
            if (string.IsNullOrEmpty(jsonData) || jsonData.Equals("[]") || jsonData.Equals("{}"))
                return default(T);
            else
                return JsonConvert.DeserializeObject<T>(jsonData);
        }
        catch (Exception ex)
        {
            Log("Base64XorToObject -> Error: " + ex.Message);
            return default(T);
        }
    }
    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }
    public static string Base64ToJson(string base64EncodedData)
    {
        var base64DecodedBytes = System.Convert.FromBase64String(base64EncodedData);
        var xByte = XorData(base64DecodedBytes, XORKey);
        var json = System.Text.Encoding.UTF8.GetString(xByte);
        return json;
    }
    public static string ObjectToBase64(object data)
    {
        var json = JsonUtility.ToJson(data);
        var dByte = System.Text.Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(XorData(dByte, XORKey));
    }
    public static string StringToBase64(string data)
    {
        var dByte = System.Text.Encoding.UTF8.GetBytes(data);
        return Convert.ToBase64String(XorData(dByte, XORKey));
    }
    public static T Base64ToObject<T>(string base64)
    {
        var json = Base64ToJson(base64);
        return JsonUtility.FromJson<T>(json);
    }
    public static T BytesToObject<T>(byte[] bytes)
    {
        var xByte = XorData(bytes, XORKey);
        return JsonUtility.FromJson<T>(System.Text.Encoding.UTF8.GetString(xByte));
    }
    public static byte[] XorData(byte[] data, string xorKEY)
    {
        byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(xorKEY);
        byte[] result = new byte[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
        }
        return result;
    }
}

[System.Serializable]
public class ApiEncryptedResponseDTO
{
    public string data;
    public string error;
    public string message;
}
[System.Serializable]
public class SignalRespDTO
{
    public SignalRRespTypeEnum type;
    public string data;
}
[System.Serializable]
public class AccessTokenDTO
{
    public string access_token;
    public int expires_in;
    public string token_type;
    public string scope;
}
[System.Serializable]
public class SessionKeyDTO
{
    public string data;
    public string error;
    public string message;
}
[System.Serializable]
public class ApiDecryptedRequestDTO
{
    public GameActionTypeEnum type;
    public string data;
}
[System.Serializable]
public class ApiEncryptedRequestDTO
{
    public string data;
    public string error;
    public string message;
}

[System.Serializable]
public enum GameActionTypeEnum
{
    SyncData = 0,
    UpdateKV = 1,
    ClientLogs = 5000
}
[System.Serializable]
public enum SignalRRespTypeEnum
{
    RefreshConfig = 0,
}
[Serializable]
public class LogEventDTO
{
    public string message;
    public string error;
    public string data;
}
