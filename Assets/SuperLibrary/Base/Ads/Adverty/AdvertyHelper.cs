using System;
using UnityEngine;
using System.Collections;
using Base;
using System.Collections.Generic;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
#if USE_ADVERTY
using Adverty;
#endif

public class AdvertyHelper : MonoBehaviour
{
    [SerializeField] protected bool autoInit = false;
    [SerializeField] protected string androidKey = "";
    [SerializeField] protected string iphoneKey = "";
    [SerializeField] protected bool isLazyLoadAllowed = true;


    public Toggle hideAdInGameToggle = null;
    public static bool IsHideAdInGame = false;
    public delegate void OnDelegateChanged(bool value);
    public static OnDelegateChanged OnHideAdInGameChanged;

    protected static string apiKey = "YOUR_KEY";

    protected static string TAG = "[ADVERTY] ";

    protected static Camera gameCamera;

    protected static bool IsInit = false;
    protected static AdvertyHelper instance = null;


    private void Awake()
    {
        try
        {
            if (instance != null)
                Destroy(gameObject);
            if (instance == null)
                instance = this;
            DontDestroyOnLoad(gameObject);

            if (hideAdInGameToggle)
            {
                hideAdInGameToggle.isOn = IsHideAdInGame;
                OnHideAdInGameChanged?.Invoke(IsHideAdInGame);
                hideAdInGameToggle.onValueChanged.AddListener((isOn) =>
                {
                    IsHideAdInGame = isOn;
                    OnHideAdInGameChanged?.Invoke(IsHideAdInGame);
                });
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(TAG + "Exception: " + ex.Message);
        }
    }

#if USE_ADVERTY
    private static void OnUnitActivated(BaseUnit obj)
    {
        LogEvent("Activated", obj);
    }

    private static void OnUnitActivationFailed(BaseUnit obj)
    {
        LogEvent("ActivationFailed", obj);
    }

    private static void OnUnitDeactivated(BaseUnit obj)
    {
        LogEvent("Deactivated", obj);
    }

    private static void OnUnitViewed(BaseUnit obj)
    {
        LogEvent("Viewed", obj);
    }

    public static void LogEvent(string eventName, BaseUnit obj)
    {
        if (obj != null && !string.IsNullOrEmpty(obj.name))
        {
            FirebaseManager.LogEvent("ad_inplay_" + eventName.ToLower(), new Dictionary<string, object>
            {
                { "id", obj.Id },
                { "item", obj.name },
                { "level_id", DataManager.StageDatas != null && DataManager.StageDatas.currentLevel != null ? DataManager.StageDatas.currentLevel.id : "unknown" }
            });
        }
    }
#endif

    private async UniTask OnGameStateChanged(GameState current, GameState last, object data = null)
    {
        if (current == GameState.Ready || current == GameState.Idle)
            UpdateCamera();
    }

    private void Start()
    {
        if (autoInit)
            StartCoroutine(DOInit());
    }

    public static IEnumerator DOInit()
    {
        if (instance == null || IsInit)
            yield break;

        while (instance.autoInit && (DataManager.UserData == null || GameStateManager.CurrentState == GameState.None))
            yield return null;

        if (DataManager.GameConfig != null && DataManager.GameConfig.adUseInPlay == false)
            yield break;

        GameStateManager.OnStateChanged += instance.OnGameStateChanged;

#if USE_ADVERTY
        AdvertyEvents.UnitViewed += OnUnitViewed;
        AdvertyEvents.UnitActivated += OnUnitActivated;
        AdvertyEvents.UnitActivationFailed += OnUnitActivationFailed;
        AdvertyEvents.UnitDeactivated += OnUnitDeactivated;

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
            apiKey = instance.iphoneKey;
        else
            apiKey = instance.androidKey;

        Debug.Log(TAG + "Init SDK: " + apiKey);

        AdvertySettings.SandboxMode = DebugMode.IsOn;
        DebugMode.OnChanged += (isOn) => AdvertySettings.SandboxMode = isOn;
        AdvertySettings.IsLazyLoadAllowed = instance.isLazyLoadAllowed;

        AdvertySettings.Mode platform = AdvertySettings.Mode.Mobile; //define target platform (Mobile, VR, AR)
        bool restrictUserDataCollection = false; //do you disallow collect extra user data?

        //Adverty.UserData userData = new Adverty.UserData(AgeSegment.Adult, Gender.Male); // define user as adult male
        Adverty.UserData userData = new Adverty.UserData(AgeSegment.Unknown, Gender.Unknown); //define user data (user and gender are unknown)

        AdvertySDK.Init(apiKey, platform, restrictUserDataCollection, userData);

        Debug.Log(TAG + "Init SDK DONE IsAPIKeyValid " + AdvertySettings.IsAPIKeyValid + " IsLazyLoadAllowed " + AdvertySettings.IsLazyLoadAllowed + " SandboxMode " + AdvertySettings.SandboxMode);

        UpdateCamera();

        IsInit = true;
#endif
    }

    public static void UpdateCamera(Camera camera = null)
    {
#if USE_ADVERTY
        if (instance == null)
            return;

        if (camera != null)
            gameCamera = camera;

        if (gameCamera == null)
            gameCamera = Camera.main;

        if (gameCamera != null)
        {
            AdvertySettings.SetMainCamera(gameCamera);
            Debug.Log(TAG + "SetMainCamera DONE");
        }
        else
            Debug.LogWarning(TAG + "SetMainCamera NULL");
#endif
    }
}
