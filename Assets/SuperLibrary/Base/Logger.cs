using System.Collections;
using System.Collections.Generic;
#if USE_FIREBASE
using Firebase.Crashlytics;
#endif
using UnityEngine;
using UnityEngine.CrashReportHandler;

public class Logger
{

    public static void d(string message)
    {
        Debug.Log(message);
        CrashlyticsLog(message);
    }

    public static void d(params object[] data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append(data[i].ToString());
            sb.Append(" ");
        }
        string message = sb.ToString();
        d(message);
    }

    public static void d(System.DateTime date)
    {
        d(date);
    }

    public static void e(string message)
    {
        Debug.LogError(message);
        CrashlyticsLog(message, true);
    }

    public static void e(params object[] data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append(data[i].ToString());
            sb.Append(" ");
        }
        string message = sb.ToString();
        e(message);
    }

    public static void w(string message)
    {
        Debug.LogWarning(message);
        CrashlyticsLog(message);
    }

    public static void w(params object[] data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append(data[i].ToString());
            sb.Append(" ");
        }
        string message = sb.ToString();
        w(message);
    }

    private static void CrashlyticsLog(string message, bool isError = false)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && USE_FIREBASE
        Crashlytics.Log(message);
#endif
    }

    public static void CrashlyticsSetKey(string key, string value)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && USE_FIREBASE
        if (Firebase.FirebaseApp.DefaultInstance != null)
            Crashlytics.SetCustomKey(key, value);
#endif

        CrashReportHandler.SetUserMetadata(key, value);
    }

    public static void CrashlyticsSetUserId(string identifier)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && USE_FIREBASE
        if (Firebase.FirebaseApp.DefaultInstance != null)
            Crashlytics.SetUserId(identifier);
#endif
    }

    public static void CrashlyticsException(System.Exception e)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && USE_FIREBASE
        if (Firebase.FirebaseApp.DefaultInstance != null)
            Crashlytics.LogException(e);
#endif
    }

    public static void d(string title, List<int> listInt)
    {
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        stringBuilder.Append(title).Append(": ");
        for (int i = 0; i < listInt.Count; i++)
        {
            stringBuilder.Append(listInt[i]).Append((i == listInt.Count - 1) ? "" : " - ");
        }

        d(stringBuilder.ToString());
    }

    public static void d(string title, int[] arrayInt)
    {
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        stringBuilder.Append(title).Append(": ");
        for (int i = 0; i < arrayInt.Length; i++)
        {
            stringBuilder.Append(arrayInt[i]);
            if (i < arrayInt.Length - 1)
                stringBuilder.Append(" - ");
        }

        d(stringBuilder.ToString());
    }
    public static void LogEditor(string head, string body)
    {
#if UNITY_EDITOR
        Debug.Log($"<color=#00ACFF>Editor ~~</color> {head}:==> {body}");
#endif
    }
}
