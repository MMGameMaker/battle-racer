using Base;
using Base.Ads;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using Spine.Unity;

public class GameUIManager : GameManagerBase<GameUIManager>
{
    [SerializeField]
    private float waitTimeForLoadAd = 1;

    [SerializeField]
    private UIAnimation splashScreen = null;

    [SerializeField]
    private UIMainScreen mainScreen = null;
    public static UIMainScreen MainScreen => instance?.mainScreen;

    [SerializeField]
    private UIInGame inGameScreen = null;
    [SerializeField]
    private UIPause pauseScreen = null;

    [SerializeField]
    private UIGameOver gameOverScreen = null;
    public static UIGameOver GameOverScreen => instance?.gameOverScreen;

    private DateTime startLoadTime = DateTime.Now;

    private GameConfig gameConfig => DataManager.GameConfig;
    private UserData userData => DataManager.UserData;

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        DOTween.Kill(gameObject);
    }

    protected override void Awake()
    {
        base.Awake();
        startLoadTime = DateTime.Now;
#if UNITY_EDITOR
        DOTween.useSafeMode = false;
#endif
    }

    protected override void Start()
    {
        Application.targetFrameRate = 60;
        base.Start();
        StartCoroutine(LoadGameData());
        StartCoroutine(LoadDailyCheckinData());
        StartCoroutine(LoadAchievementData());
    }

    public IEnumerator LoadGameData()
    {
        yield return DataManager.DoLoad();
        //UIToast.ShowLoading("Loading... please wait!!");

        while (userData == null)
        {
            DebugMode.Log("Load game data...");
            yield return null;
        }

        SoundManager.LoadAllSounds();

        yield return AdsManager.DOInit();

#if USE_FIREBASE
        var remote = new GameConfig();
        var defaultRemoteConfig = new Dictionary<string, object>
        {
            {"suggestUpdateVersion" , gameConfig.suggestUpdateVersion },
            {"minLevelToShowInter", gameConfig.minLevelToShowInter },
        };

        yield return FirebaseManager.DoCheckStatus(defaultRemoteConfig);

        var cacheExpirationHours = 12;
#if UNITY_EDITOR
        cacheExpirationHours = 0;
#endif
        yield return FirebaseManager.DoFetchRemoteData((status) =>
        {
            if (status == FirebaseStatus.Success && userData != null && gameConfig != null)
            {
                gameConfig.suggestUpdateVersion = FirebaseManager.RemoteGetValueInt("suggestUpdateVersion");
                gameConfig.minLevelToShowInter = FirebaseManager.RemoteGetValueInt("seafood_minLevelToShowInter");
                if (gameConfig.minLevelToShowInter <= 3)
                    gameConfig.minLevelToShowInter = 4;
            }

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("DoFetchRemoteData_" + status.ToString());
                DebugMode.Log(JsonUtility.ToJson(gameConfig));
            });
        }, cacheExpirationHours);
#endif

        if (userData.VersionInstall == 0)
        {
            userData.VersionInstall = UIManager.BundleVersion;
#if USE_FIREBASE
            FirebaseManager.SetUser("Type", "New");
#endif
        }
        else if (userData.VersionInstall != UIManager.BundleVersion)
        {
#if USE_FIREBASE
            FirebaseManager.SetUser("Type", "Update");
#endif
        }
        userData.VersionCurrent = UIManager.BundleVersion;

        GameStateManager.LoadMain(null);

#if USE_MAXOPEN
        bool readyCheck = false;
        var wait = new WaitForSeconds(1);
        for (float elapsed = 0; elapsed < waitTimeForLoadAd; elapsed++)
        {
            if (MaxHelper.OpenIsReady)
            {
                readyCheck = true;
                break;
            }
            yield return wait;
        }
        if (!readyCheck)
            Debug.Log($"{MaxHelper.TAG} TimeOutWait OpenIsReady -> {MaxHelper.OpenIsReady}");
        AdsManager.ShowAdOpen();
#endif
        if (gameConfig.suggestUpdateVersion > userData.VersionCurrent)
        {
        }
        else
        {
            GameStateManager.Idle(null);
            yield return new WaitForSeconds(0.5f);
            splashScreen?.Hide();
        }

        int loadGameIn = (int)(DateTime.Now - startLoadTime).TotalSeconds;
        Debug.Log("loadGameIn: " + loadGameIn + "s");
    }

    public IEnumerator LoadDailyCheckinData()
    {
        yield return DailyCheckinDataManager.DoLoad();
    }

    public IEnumerator LoadAchievementData()
    {
        yield return AchievementDataManager.DoLoad();
    }

    protected override void LoadMain(object data)
    {
        base.LoadMain(data);
    }

    public override void IdleGame(object data)
    {
        SceneHelper.DoLoadScene("2_Idle");
        if (GameStateManager.LastState != GameState.LoadMain)
        {
            UILoadGame.Init(true, null);
            StartCoroutine(WaitForLoading(() =>
            {
                mainScreen.Show();
            }, 0.25f));
        }
        else
        {
            mainScreen.Show();
        }
    }

    public override void LoadGame(object data)
    {
        gameOverScreen.Hide();
        Time.timeScale = 1;
        LoadGameContent.PrepairDataToPlay();
    }

    public override void InitGame(object data)
    {
        foreach (var i in UIManager.listPopup)
            i.Hide();
        DOTween.Kill(this);
    }

    public IEnumerator WaitForLoading(Action onComplete, float time = 0)
    {
        if (time > 0)
            yield return new WaitForSeconds(time);

        while (UILoadGame.currentProcess < 1)
        {
            UILoadGame.Process();
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        onComplete?.Invoke();
        yield return new WaitForSeconds(0.15f);
        UILoadGame.Hide();
    }

    public override void PlayGame(object data)
    {
        mainScreen.Hide();
        inGameScreen.Show();
        MusicManager.UnPause();
    }

    public override void PauseGame(object data)
    {
        MusicManager.Pause();
        inGameScreen.Hide();
        pauseScreen.Show();
    }

    protected override void GameOver(object data)
    {
        inGameScreen.Hide();
        gameOverScreen.Show(GameState.GameOver, data);
    }

    protected override void CompleteGame(object data)
    {
        inGameScreen.Hide();
        gameOverScreen.Show(GameState.Complete, data);
    }

    protected override void ReadyGame(object data)
    {
        mainScreen.Hide();
        StartCoroutine(WaitForLoading(() =>
        {
            inGameScreen.Show();
            DOVirtual.DelayedCall(0.1f, () =>
            {
                GameStateManager.Play(null);
            }).SetId(gameObject);
        }, 0.25f));
    }

    public override void ResumeGame(object data)
    {
        inGameScreen.Show();
        pauseScreen.Hide();
    }

    public override void RestartGame(object data)
    {
        MusicManager.UnPause();
        GameStateManager.LoadGame(null);
    }

    public override void NextGame(object data)
    {
        gameOverScreen.Hide();
    }

    protected override void WaitingGameOver(object data)
    {
        inGameScreen.Hide();
        MusicManager.Pause();
        if (GameStateManager.CurrentState == GameState.WaitGameOver)
            GameStateManager.GameOver(data);
    }

    protected override void WaitingGameComplete(object data)
    {
        MusicManager.Pause();
    }

    protected override void RebornContinueGame(object data)
    {
        inGameScreen.Hide();
        gameOverScreen.Show(GameState.RebornContinue, data);
    }

    protected override void RebornCheckPointGame(object data)
    {
        gameOverScreen.Hide();
        GameStateManager.Init(null);
        StartCoroutine(WaitToAutoPlay());
    }

    IEnumerator WaitToAutoPlay()
    {
        var wait01s = new WaitForSeconds(0.1f);
        var wait05s = new WaitForSeconds(1.5f);
        while (GameStateManager.CurrentState != GameState.Ready)
            yield return wait01s;
        yield return wait05s;
        GameStateManager.Play(null);
        inGameScreen.Show();
    }
}