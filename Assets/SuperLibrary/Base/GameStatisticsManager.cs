using System;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameStatisticsManager : MonoBehaviour
{
    #region GameState
    private UserData userData => DataManager.UserData;

    private void Awake()
    {
        GameStateManager.OnStateChanged += OnGameStateChanged;
    }

    private async UniTask OnGameStateChanged(GameState current, GameState last, object data)
    {
        if (current != GameState.LoadMain)
        {
            switch (current)
            {
                case GameState.Idle:
                    DebugMode.UpdateWinLose();
                    break;
                case GameState.Init:
                    UIManager.DelScreenShot();
                    break;
                case GameState.Play:
                    userData.TotalPlay++;
                    break;
                case GameState.RebornCheckPoint:
                    break;
                case GameState.RebornContinue:
                    break;
                case GameState.Restart:
                    break;
                case GameState.WaitGameOver:
                    DebugMode.UpdateWinLose();
                    break;
                case GameState.WaitComplete:
                    userData.WinStreak++;
                    userData.level++;
                    Debug.Log("Increase Level".ToUpper());
                    DebugMode.UpdateWinLose();
                    break;
                case GameState.Complete:
                    break;
                case GameState.GameOver:
                    break;
            }
        }
    }
    #endregion

    #region RewardInGame
    public static int goldEarn = 10;
    public static int gemEarn = 0;
    #endregion
}
