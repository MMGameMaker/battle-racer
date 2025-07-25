using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class LocalizedManager : MonoBehaviour
{
    [SerializeField]
    private bool dontDestroyOnLoad = false;
    [SerializeField]
    private bool loadAtStart = true;

    public static Text currentName;
    public static string stringCode { get; set; }

    private static LocalizationCode current;
    public static LocalizationCode Current
    {
        get
        {
            if (current == null && languages != null)
                current = languages.FirstOrDefault(x => x.code == stringCode);
            return current;
        }
        set
        {
            if (current != value)
                current = value;
        }
    }

    public static List<LocalizationCode> languages = new List<LocalizationCode>
    {
        new LocalizationCode("en-UK", "English"),
        new LocalizationCode("de-DE", "Deutsch"),
        new LocalizationCode("es-ES", "Español"),
        new LocalizationCode("fr-FR", "Français"),
        new LocalizationCode("id-ID", "Indonesian"),
        new LocalizationCode("ja-JP", "日本"),
        new LocalizationCode("ko-KR", "한국어"),
        new LocalizationCode("pt-BR", "Português"),
        new LocalizationCode("ru-RU", "Русский"),
        new LocalizationCode("vi-VN", "Tiếng Việt"),
        new LocalizationCode("zh-Hans", "中国"),
        new LocalizationCode("zh-Hant", "中國")
    };
    public static Dictionary<string, string> localizedData = new Dictionary<string, string>();
    public static List<UILocalizedText> uiTextList = new List<UILocalizedText>();

    public delegate void SetLocalizedEvent(bool status);
    public static event SetLocalizedEvent onSetLocalized;

    private static LocalizedManager instance { get; set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        if (loadAtStart)
        {
            stringCode = PlayerPrefs.GetString("localize", "");
            SetLocalized(stringCode);
        }
    }

    public static void SetLocalized(string code = null)
    {
        if (!string.IsNullOrEmpty(code))
        {
            stringCode = code;
            PlayerPrefs.SetString("localize", stringCode);
            PlayerPrefs.Save();
        }
        else
        {
            var language = Application.systemLanguage;
            string systemCode = "en-UK";
            switch (language)
            {
                case SystemLanguage.German:
                    systemCode = "de-DE";
                    break;
                case SystemLanguage.Spanish:
                    systemCode = "es-ES";
                    break;
                case SystemLanguage.French:
                    systemCode = "fr-FR";
                    break;
                case SystemLanguage.Indonesian:
                    systemCode = "id-ID";
                    break;
                case SystemLanguage.Japanese:
                    systemCode = "ja-JP";
                    break;
                case SystemLanguage.Korean:
                    systemCode = "ko-KR";
                    break;
                case SystemLanguage.Portuguese:
                    systemCode = "pt-BR";
                    break;
                case SystemLanguage.Russian:
                    systemCode = "ru-RU";
                    break;
                case SystemLanguage.Vietnamese:
                    systemCode = "vi-VN";
                    break;
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    systemCode = "zh-Hans";
                    break;
                case SystemLanguage.ChineseTraditional:
                    systemCode = "zh-Hant";
                    break;
                default:
                    systemCode = "en-UK";
                    break;
            }

            PlayerPrefs.SetString("localize", systemCode);
            PlayerPrefs.Save();
        }

        LoadLocalized(stringCode);
    }

    private static void LoadLocalized(string fileName = "")
    {
        if (string.IsNullOrEmpty(fileName))
            fileName = stringCode = PlayerPrefs.GetString("localize", "en-UK");

        localizedData = new Dictionary<string, string>();
        var asset = Resources.Load<TextAsset>(fileName);
        if (asset != null)
        {
            string content = asset.ToString();
            var loadedData = JsonUtility.FromJson<LocalizationData>(content);
            for (int i = 0; i < loadedData.items.Length; i++)
            {
                try
                {
                    if (!localizedData.ContainsKey(loadedData.items[i].key))
                        localizedData.Add(loadedData.items[i].key, loadedData.items[i].value);
                    else
                        Debug.LogError(loadedData.items[i].key + " is EXIT");
                }
                catch (Exception ex)
                {
                    Debug.LogError("[LocalizationHelper] SetLocalized: " + ex.Message);
                }
            }

            onSetLocalized?.Invoke(true);
        }
        else
        {
            Debug.LogWarning("Cannot find file! " + fileName + ".json");
            PlayerPrefs.SetString("localize", "en-UK");
            PlayerPrefs.Save();

            if (fileName != "en-UK")
            {
                SetLocalized("en-UK");
                onSetLocalized?.Invoke(false);
            }
            else
            {
                Debug.LogError("!!! Cannot find file! " + fileName + ".json in Resources folder");
            }
        }

        UpdateAllText();
    }

    public static void UpdateText(UILocalizedText ui)
    {
        if (!string.IsNullOrEmpty(ui.Key))
        {
            var keyString = Key(ui.Key, ui.gameObject);
            ui.Text = keyString;
        }
    }

    public static string Key(string key, GameObject go = null)
    {
        string result = "";
        if (instance != null && localizedData != null && localizedData.Any())
        {
            if (localizedData.ContainsKey(key))
            {
                result = localizedData[key.Trim()];
            }
            else
            {
                result = key.Replace("base_", "");
                if (go)
                    Debug.LogError("[LocalizationHelper] " + result + " not found" + " " + stringCode + " " + go.name);
                else
                    Debug.LogError("[LocalizationHelper] " + result + " not found" + " " + stringCode);
            }
            return result;
        }
        return "...";
    }

    public static void NextLanguage(int next = 1)
    {
        if (languages != null && Current != null)
        {
            int nextIndex = languages.IndexOf(Current) + next;
            if (nextIndex >= languages.Count)
                nextIndex = 0;
            else if (nextIndex < 0)
                nextIndex = languages.Count - 1;

            Current = languages[nextIndex];
            SetLocalized(Current.code);
        }
    }

    private static void UpdateAllText()
    {
        if (instance != null && currentName != null)
            currentName.text = Current.name;

        foreach (var i in uiTextList)
            UpdateText(i);
    }
}

#region Properties
[Serializable]
public class LocalizationData
{
    public LocalizationItem[] items;
}

[Serializable]
public class LocalizationItem
{
    public string key;
    public string value;
}

[Serializable]
public class LocalizationCode
{
    public string code;
    public string name;

    public LocalizationCode(string _code, string _name)
    {
        code = _code;
        name = _name;
    }
}
#endregion