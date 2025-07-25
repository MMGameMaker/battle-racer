using UnityEngine;
using Cysharp.Threading.Tasks;
using Base;
using System;

public class GameStateManager : MonoBehaviour
{
    public static GameState CurrentState
    {
        get
        {
            return instance != null ? instance.current : GameState.None;
        }
        private set
        {
            if (instance)
            {
                instance.lastState = instance.current;
                if (instance.current != value)
                {
                    instance.current = value;
                    OnStateChanged?.Invoke(instance.current, instance.lastState, stateChangedData);
                }
            }
        }
    }
    public static bool isBusy { get; set; }
    public static GameState LastState => instance.lastState;
    private static GameStateManager instance { get; set; }

    [SerializeField]
    private GameState lastState = GameState.None;
    [SerializeField]
    private GameState current = GameState.None;

    public delegate UniTask StateDelegate(GameState current, GameState last, object data);
    public static event StateDelegate OnStateChanged;

    private static object stateChangedData = null;


    private void Awake()
    {
        instance = this;
    }

    private void OnApplicationPause(bool pause)
    {
        isBusy = pause;
    }

    private void OnApplicationFocus(bool focus)
    {
    }

    public static void None(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.None;
    }

    public static void LoadMain(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.LoadMain;
    }

    public static void Login(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.Login;
    }

    public static void LoadGame(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.LoadGame;
    }

    public static void Init(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.Init;
    }

    public static void Play(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.Play;
    }

    public static void Idle(object data, bool force = false)
    {
        if (force)
        {
            CurrentState = GameState.None;
        }
        stateChangedData = data;
        CurrentState = GameState.Idle;
    }

    public static void Pause(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.Pause;
    }

    public static void Restart(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.Restart;
    }

    public static void RebornCheckPoint(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.RebornCheckPoint;
    }

    public static void RebornContinue(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.RebornContinue;
    }

    public static void GameOver(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.GameOver;
    }

    public static void Complete(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.Complete;
    }

    public static void WaitGameOver(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.WaitGameOver;
    }

    public static void WaitComplete(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.WaitComplete;
    }

    public static void Ready(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.Ready;
    }

    public static void Next(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.Next;
    }
    public static void InShop(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.InShop;
    }
    public static void LuckyWheel(object data)
    {
        stateChangedData = data;
        CurrentState = GameState.LuckySpin;
    }
}

public enum GameState
{
    /// <summary>
    ///1. Open game -> NONE -> on done -> LOADMAIN (if need to hide game object)
    ///2. for some ui listener do something like spectrum or beat or efeect on main
    ///</summary>
    None,

    /// <summary>
    /// Load main on done -> LOGIN
    ///</summary>
    LoadMain,

    /// <summary>
    /// LOGIN on done -> Idle
    ///</summary>
    Login,

    /// <summary>
    ///1. Waiting for user click stage or button play + hide game object (on Main Screen)
    ///2. Back from game to main (kill game object + hide IngameScreen + show MainScreen)
    /// </summary>
    Idle,

    /// <summary>
    ///User click Play or Stage from UI -> LOADGAME 
    ///-> load game content, music, midi, mp3, on loading content completed -> INIT
    /// </summary>
    LoadGame,

    /// <summary>
    /// DONE -> load game content, init game object... on init game completed -> READY
    /// FAIL -> IDLE rollback to main screen
    /// </summary>
    Init,

    /// <summary>
    /// Loading and Init game done -> READY (game is loaded)
    ///1. waiting for user click "tap to start" -> START (game is start)
    ///2. if don't need "tap to start" -> START (game is start)
    /// </summary>
    Ready,

    /// <summary>
    /// if use click "tap to start" (last state = READY) -> game is start
    /// if last state = REBORNCONTINUE or REBORNCHECKPOINT -> game is start
    /// </summary>
    Play,

    /// <summary>
    /// if use click "pause button" or back hardware buton -> PAUSE (game is pause)
    /// </summary>
    Pause,

    /// <summary>
    /// if user die but can reborn continue -> REBORNCONTINUE (game is waiting acttion)
    /// if user click reborn Button -> START (game is play continue)
    /// if user click cancel Button -> GAMEOVER (game is over)
    /// </summary>
    RebornContinue,

    /// <summary>
    /// if user die but can reborn last checkPoint -> REBORNCHECKPOINT (game is waiting acttion)
    /// if user click reborn Button -> START (game is play continue)
    /// if user click cancel Button -> GAMEOVER (game is over)
    /// </summary>
    RebornCheckPoint,

    /// <summary>
    /// waiting for change stage or user die but wait animation done then check can CONTINUE or GAMEOVER
    /// </summary>
    WaitGameOver,

    /// <summary>
    /// if user CAN'T CONTINUE or END TIME count down on RebornScreen -> GAMEOVER 
    /// </summary>
    GameOver,

    /// <summary>
    /// waiting for change stage or user die but wait animation done then set COMPLETE
    /// </summary>
    WaitComplete,

    /// <summary>
    /// if user completed -> COMPLETED
    /// </summary>
    Complete,

    /// <summary>
    /// if user completed -> RESTART -> game is play -> loading game if need -> INIT
    /// </summary>
    Restart,

    /// <summary>
    /// if user completed -> NEXT -> loading game if need -> LOADING
    /// </summary>
    Next,
    /// <summary>
    /// Show Shop
    /// </summary>
    InShop,
    LuckySpin,
    DailyReward
}
