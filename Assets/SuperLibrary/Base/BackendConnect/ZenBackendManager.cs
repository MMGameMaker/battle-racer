
using Best.HTTP;
using Best.HTTP.Request.Upload;
using Best.HTTP.Request.Upload.Forms;
using Cysharp.Threading.Tasks;
using MyBox;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class ZenBackendManager : MonoBehaviour
{
    public static async UniTask<string> GetTokenAccessJsonAsync(object param)
    {
        var base64encode = ZenUtls.Base64Encode(JsonConvert.SerializeObject(param));
        var utcs = CreateHttpRequestForTokenAndConfig(ZenUtls.URLToken, "tokenaccess", HTTPMethods.Post, out HTTPRequest request);

        // Define the form data
        var formFields = new Dictionary<string, string>
        {
            { "client_id", "rhinova-telegram-client" },
            { "grant_type", "delegation" },
            { "subject_token_type", "TelegramBot" },
            { "subject_token", base64encode },
            { "scope", "rhinova:api openid profile" },
        };

        var form = new UrlEncodedStream();

        // Convert form fields to URL-encoded format and add to the request
        foreach (var field in formFields)
        {
            form.AddField(field.Key, field.Value);
        }

        request.UploadSettings.UploadStream = form;
        // Set the Content-Type header
        request.SetHeader("Content-Type", "application/x-www-form-urlencoded");

        // Send the request
        await request.Send();
        await request.ToUniTask();
        if (request.Exception != null)
            utcs.TrySetResult(string.Empty);

        return await utcs.Task;
    }
    public static async UniTask<string> GetSessionKeyJsonAsync(string accessToken)
    {
        var url = $"{ZenUtls.URLApi}/api/sessions";
        var utcs = CreateHttpRequestForSession(url, "sessionkey", HTTPMethods.Post, out HTTPRequest request);
        request.SetHeader("Authorization", "Bearer " + accessToken);

        await request.Send();
        await request.ToUniTask();
        if (request.Exception != null)
            utcs.TrySetResult(string.Empty);

        return await utcs.Task;
    }
    public static async UniTask<string> ActionOnGameAsync(string accessToken, GameActionTypeEnum gameActionTypeEnum, object dto)
    {
        var url = $"{ZenUtls.URLApi}/api/gamedata/actions";
        var utcs = CreateHttpRequest(url, gameActionTypeEnum.ToString(), HTTPMethods.Post, out HTTPRequest request);
        if (string.IsNullOrEmpty(accessToken))
        {
            utcs.TrySetResult(string.Empty);
            return await utcs.Task;
        }
        else
        {
            request.SetHeader("Content-Type", "application/json");
            request.SetHeader("Authorization", "Bearer " + accessToken);

            var decryptedRequestDTO = new ApiDecryptedRequestDTO
            {
                type = gameActionTypeEnum,
                data = dto == null ? "" : JsonUtility.ToJson(dto)
            };
            var encryptedRequestDTO = new ApiEncryptedRequestDTO
            {
                data = ZenUtls.ObjectToBase64(decryptedRequestDTO)
            };
            request.UploadSettings.UploadStream = new JSonDataStream<ApiEncryptedRequestDTO>(encryptedRequestDTO);

            await request.Send();
            await request.ToUniTask();
            if (request.Exception != null)
                utcs.TrySetResult(string.Empty);

            return await utcs.Task;
        }
    }
    public static async UniTask<string> GetUserKVJson(string accessToken, List<string> keys)
    {
        var url = $"{ZenUtls.URLApi}/api/kv?";
        foreach (var key in keys)
            url += $"keys={key}&";
        Debug.Log("GetKeyValue Url:" + url);
        var utcs = CreateHttpRequest(url, "", HTTPMethods.Get, out HTTPRequest request);
        request.SetHeader("Authorization", "Bearer " + accessToken);

        await request.Send();
        await request.ToUniTask();
        if (request.Exception != null)
            utcs.TrySetResult(string.Empty);

        return await utcs.Task;
    }
    public static async void Log(string eventStr, string eventData = "")
    {
        var check = await ZenBackendManager.ActionOnGameAsync(ZenUtls.AccessToken,
            GameActionTypeEnum.ClientLogs,
            new LogEventDTO
            {
                message = eventStr,
                data = eventData
            });
        ZenUtls.Log($"Log -> '{eventStr}' -> '{eventData}' -> {check}");
    }


    private static UniTaskCompletionSource<string> CreateHttpRequest(string url, string meta, HTTPMethods httpMethod, out HTTPRequest request)
    {
        var utcs = new UniTaskCompletionSource<string>();
        void OnResponseFinish(HTTPRequest req, HTTPResponse res)
        {
            if (res.IsSuccess)
                utcs.TrySetResult(res.DataAsText);
            else
            {
                Debug.Log($"OnRequestError {req.CurrentUri.AbsoluteUri} -> {meta} -> {res.StatusCode} - {res.DataAsText}");
                utcs.TrySetResult(res.DataAsText);
            }
        }
        request = new HTTPRequest(new Uri(url), httpMethod, OnResponseFinish);
        request.AddHeader("X-Client-Version", ZenUtls.StringToBase64(UIManager.BundleVersion.ToString()));
        request.AddHeader("X-Client-SessionId", ZenUtls.SessionKey);
        return utcs;
    }
    private static UniTaskCompletionSource<string> CreateHttpRequestForTokenAndConfig(string url, string meta, HTTPMethods httpMethod, out HTTPRequest request)
    {
        var utcs = new UniTaskCompletionSource<string>();
        void OnResponseFinish(HTTPRequest req, HTTPResponse res)
        {
            if (res.IsSuccess)
                utcs.TrySetResult(res.DataAsText);
            else
            {
                Debug.Log($"OnRequestError {req.CurrentUri.AbsoluteUri} -> {meta} -> {res.StatusCode} - {res.DataAsText}");
                utcs.TrySetResult(res.DataAsText);
            }
        }
        request = new HTTPRequest(new Uri(url), httpMethod, OnResponseFinish);
        return utcs;
    }
    private static UniTaskCompletionSource<string> CreateHttpRequestForSession(string url, string meta, HTTPMethods httpMethod, out HTTPRequest request)
    {
        var utcs = new UniTaskCompletionSource<string>();
        void OnResponseFinish(HTTPRequest req, HTTPResponse res)
        {
            if (res.IsSuccess)
                utcs.TrySetResult(res.DataAsText);
            else
            {
                Debug.Log($"OnRequestError {req.CurrentUri.AbsoluteUri} -> {meta} -> {res.StatusCode} - {res.DataAsText}");
                utcs.TrySetResult(res.DataAsText);
            }
        }
        request = new HTTPRequest(new Uri(url), httpMethod, OnResponseFinish);
        request.AddHeader("X-Client-Version", ZenUtls.StringToBase64(UIManager.BundleVersion.ToString()));
        return utcs;
    }


    [SerializeField] List<string> testDataKeys;
    [SerializeField] KVDataObjec<string> testSaveValue;
    [ButtonMethod]
    private async void Test_SaveKvData()
    {
        var testKV = new UpdateKVDT()
        {
            pairs = new List<KVDataDTO>() { new KVDataDTO() { key = testDataKeys[0], value = JsonConvert.SerializeObject(testSaveValue) } }
        };

        var checkJSON = await ZenBackendManager.ActionOnGameAsync(ZenUtls.AccessToken
            , GameActionTypeEnum.UpdateKV, testKV);
        var check = JsonConvert.DeserializeObject<LogEventDTO>(checkJSON);
        if (!string.IsNullOrEmpty(check.error))
        {
            Debug.Log("Save kv error");
        }
    }
    [ButtonMethod]
    private async void Test_GetKVData()
    {
        var response = await GetUserKVJson(ZenUtls.AccessToken, testDataKeys);
        ZenUtls.Log($"Ins_KV Data -> {response}");
        var encripJson = JsonUtility.FromJson<GetKVDataDTOListAPIResponse>(response);
        ZenUtls.Log($"Ins_KV_Encrypted -> {JsonConvert.SerializeObject(encripJson)}");
        ZenUtls.Base64XorToObject<List<GetKVDataDTO>>(encripJson.data);
    }
}
