using DG.Tweening;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
public abstract class GameManagerBase<T> : MonoBehaviour where T : GameManagerBase<T>
{
    public static readonly string GameTweenId = "GAME_TWEEN_ID";
    protected static T instance;

    protected virtual void Awake()
    {
        instance = (T)this;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    protected virtual void Start()
    {
        GameStateManager.OnStateChanged += StateManager_OnStateChanged;
    }

    private async UniTask StateManager_OnStateChanged(GameState current, GameState last, object data)
    {
        switch (current)
        {
            case GameState.None:
                break;
            case GameState.Idle:
                IdleGame(data);
                break;
            case GameState.LoadMain:
                LoadMain(data);
                break;
            case GameState.Login:
                Login(data);
                break;
            case GameState.LoadGame:
                LoadGame(data);
                break;
            case GameState.Init:
                InitGame(data);
                break;
            case GameState.Play:
                if (last == GameState.Pause)
                    ResumeGame(data);
                else
                    PlayGame(data);
                break;
            case GameState.Pause:
                PauseGame(data);
                break;
            case GameState.Restart:
                RestartGame(data);
                break;
            case GameState.Ready:
                ReadyGame(data);
                break;
            case GameState.WaitGameOver:
                WaitingGameOver(data);
                break;
            case GameState.WaitComplete:
                WaitingGameComplete(data);
                break;
            case GameState.RebornContinue:
                RebornContinueGame(data);
                break;
            case GameState.RebornCheckPoint:
                RebornCheckPointGame(data);
                break;
            case GameState.GameOver:
                GameOver(data);
                break;
            case GameState.Complete:
                CompleteGame(data);
                break;
            case GameState.Next:
                NextGame(data);
                break;
            case GameState.InShop:
                GoToShop(data);
                break;
            case GameState.LuckySpin:
                LuckyWheel(data);
                break;
        }
    }

    public abstract void IdleGame(object data);
    public abstract void LoadGame(object data);
    public abstract void InitGame(object data);
    public abstract void PlayGame(object data);
    public abstract void PauseGame(object data);
    public abstract void ResumeGame(object data);
    public abstract void RestartGame(object data);
    public abstract void NextGame(object data);
    protected abstract void WaitingGameOver(object data);
    protected abstract void WaitingGameComplete(object data);
    protected abstract void GameOver(object data);
    protected abstract void CompleteGame(object data);
    protected abstract void ReadyGame(object data);

    protected virtual void RebornContinueGame(object data) { }
    protected virtual void RebornCheckPointGame(object data) { }
    protected virtual void LoadMain(object data)
    {

    }
    protected virtual void GoToShop(object data)
    {

    }
    protected virtual void LuckyWheel(object data)
    { }
    protected virtual void Login(object data)
    {

    }
}
