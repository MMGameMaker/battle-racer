using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
internal class DebugMode : MonoBehaviour
{
    [Header("Debug Mode")]
    [SerializeField]
    private Toggle debugModeToggle = null;
    [SerializeField]
    private Button buttonDebugModeButton = null;

    private static bool isDebugMode;
    public static bool IsDebugMode
    {
        get => isDebugMode;
        set
        {
            isDebugMode = value;
            OnDebugModeChanged?.Invoke(isDebugMode);

            if (instance)
            {
                if (isDebugMode)
                    instance.debugModeIsOnEvent?.Invoke();
                else
                    instance.debugModeIsOffEvent?.Invoke();
                instance.debugModeToggle.isOn = isDebugMode;
                instance.debugModeToggle.gameObject.SetActive(isDebugMode);
                instance.DebugChanged(IsDebugMode);
                PlayerPrefs.SetInt(instance.name, isDebugMode ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }

    public delegate void DebugModeChangedDelegate(bool isOn);
    public static event DebugModeChangedDelegate OnDebugModeChanged;

    [Serializable]
    private class DebugModeEvent : UnityEvent { }
    [SerializeField]
    private DebugModeEvent debugModeIsOnEvent = new DebugModeEvent();
    [SerializeField]
    private DebugModeEvent debugModeIsOffEvent = new DebugModeEvent();

    [SerializeField]
    private List<GameObject> debugGroup = null;

    [SerializeField]
    private Button sendDebugButton = null;

    [Header("Log Panel")]
    [SerializeField]
    private Toggle showLogPanelToggle = null;
    [SerializeField]
    private GameObject logPanel = null;
    [SerializeField]
    private Text textLog = null;
    [SerializeField]
    private Text textWinLose = null;

    [Header("Options")]
    [Range(20, 100)]
    [SerializeField]
    private int eventsDebugShow = 20;
    public Color normalColor = new Color(255, 255, 255, 255);
    public Color errorColor = new Color(200, 0, 0, 255);

    private int showDebugModeCount;
    private static string datePatt = @"hh:mm:ss tt";
    private static IList<string> events = new List<string>();

    [SerializeField] InputField bgmVolumneInput;

    private static DebugMode instance = null;


    private void Awake()
    {
        instance = this;

        if (showLogPanelToggle)
            showLogPanelToggle.onValueChanged.AddListener(ShowLogPanel);
        else
            Debug.LogWarning("[UILog] showLogPanel is NULL");

        if (sendDebugButton)
            sendDebugButton.onClick.AddListener(SendDebugMode);
        else
            Debug.LogWarning("[UILog] sendDebugButton is NULL");

        if (buttonDebugModeButton)
            buttonDebugModeButton.onClick.AddListener(ShowDebugMode);
        else
            Debug.LogWarning("[UILog] buttonDebugModeButton is NULL");

        if (debugModeToggle)
        {
            IsDebugMode = PlayerPrefs.GetInt(name, 0) == 1 ? true : false;
            DebugChanged(IsDebugMode);
            debugModeToggle.gameObject.SetActive(IsDebugMode);
            debugModeToggle.onValueChanged.AddListener((isOn) => IsDebugMode = isOn);
        }
    }

    private void ShowDebugMode()
    {
#if UNITY_EDITOR
        showDebugModeCount = 1;
#endif
        showDebugModeCount++;
        if (showDebugModeCount >= 100000)
        {
            UIToast.ShowLoading("Debug mode is show!");
            IsDebugMode = true;
            UpdateWinLose();
        }
    }

    #region Static
    public static void LogError(object log)
    {
        Log(log, true);
    }

    public static void Log(object log, bool isError = false)
    {
        try
        {
            if (instance != null)
            {
                var result = log.ToString();
                var stackFrame = new System.Diagnostics.StackFrame(1, true);
                var timeString = System.DateTime.Now.ToString(datePatt);
                if (events.Count >= instance.eventsDebugShow)
                    events.Remove(events.LastOrDefault());

                if (isError)
                {
                    events.Insert(0, "--------------------" + "\n");
                    events.Insert(0, timeString + " - [ERROR] - " + result + "\n");
                    events.Insert(0, "--------------------" + "\n");
                }
                else
                {
                    events.Insert(0, timeString + " - " + result + "\n");
                }

                if (isError)
                    Debug.LogError(log.ToString());
                else
                    Debug.Log(log.ToString());

                instance.SetLog(false);
            }
            else
            {
                if (isError)
                    Debug.LogError(log.ToString());
                else
                    Debug.Log(log.ToString());
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[UILog] : " + ex.Message);
        }
    }
    public static void UpdateWinLose()
    {
        instance.textWinLose.text = $"WS:{DataManager.UserData.WinStreak}  LS:{DataManager.UserData.LoseStreak}";
    }
    #endregion

    private void ShowLogPanel(bool isOn)
    {
        logPanel.SetActive(isOn);
        SetLog(!isOn);
    }

    public void SetLog(bool clear)
    {
        if (clear)
        {
            events = new List<string>();
            textLog.text = "";
        }
        else
        {
            textLog.text = logData;
        }
    }

    private string logData
    {
        get
        {
            var logData = "";
            foreach (var i in events)
                logData += i;
            return logData;
        }
    }

    private void DebugChanged(bool isOn)
    {
        foreach (var i in debugGroup)
            if (i) i.SetActive(isOn);
    }

    private void SendDebugMode()
    {
        UIManager.Share("LogData", "LogData", logData);
    }

    public static bool IsAuto = false;
    public void SwitchAutoPlay()
    {
        IsAuto = !IsAuto;
        //GameCoreManager.IsAuto = IsAuto;
        UIToast.ShowNotice("Auto play is turned " + (IsAuto ? "ON" : "OFF") + "!");
    }

    public void Ins_CompleteLevel()
    {
        if (GameStateManager.CurrentState == GameState.Play)
            GameStateManager.WaitComplete(null);
    }

    public void Ins_ParseVolume()
    {
        MusicManager.AudioSourceReal.volume = (float)Convert.ToDouble(bgmVolumneInput.text);
    }
}