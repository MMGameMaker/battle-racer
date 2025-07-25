using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;
using System;
using UnityEngine.SceneManagement;
using MyBox;

public class GameCoreManager : GameManagerBase<GameCoreManager>
{
    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    protected override void Start()
    {
        base.Start();
    }



    protected override void LoadMain(object data)
    {
        base.LoadMain(data);
    }

    public override void IdleGame(object data)
    {
        Time.timeScale = 1;
        Debug.Log("Game Core goto IdleGame");
    }

    public override void InitGame(object data)
    {
        Debug.Log("Game Core goto InitGame");
    }

    public override void LoadGame(object data)
    {
    }

    public override void NextGame(object data)
    {
        Debug.Log("Game Core goto NextGame");
    }


    float timeScaleAtPause = 0;
    public override void PauseGame(object data)
    {
        Debug.Log("Game Core goto PauseGame");
        timeScaleAtPause = Time.timeScale;
        Time.timeScale = 0;
    }

    public override void PlayGame(object data)
    {
        Debug.Log("Game Core goto PlayGame");
    }

    public override void RestartGame(object data)
    {
        Debug.Log("Game Core goto RestartGame");
    }

    public override void ResumeGame(object data)
    {
        Debug.Log("Game Core goto ResumeGame");
        Time.timeScale = 1;
    }

    protected override void CompleteGame(object data)
    {
        Debug.Log("Game Core goto CompleteGame");
    }

    protected override void GameOver(object data)
    {
        Debug.Log("Game Core goto GameOver");
    }

    protected override void ReadyGame(object data)
    {
        Debug.Log("Game Core goto ReadyGame");
    }

    protected override void RebornCheckPointGame(object data)
    {
        Debug.Log("Game Core goto RebornCheckPointGame");
    }

    protected override void RebornContinueGame(object data)
    {
        Debug.Log("Game Core goto RebornContinueGame");
    }

    protected override void WaitingGameComplete(object data)
    {
        if (GameStateManager.CurrentState == GameState.WaitComplete)
            GameStateManager.Complete(null);
    }

    protected override void WaitingGameOver(object data)
    {
        Debug.Log("Game Core goto WaitingGameOver");
    }


    [ButtonMethod]
    public void EditorWin()
    {
        GameStateManager.WaitComplete(null);
    }
    [ButtonMethod]
    public void EditorLose()
    {
        GameStateManager.WaitGameOver(null);
    }
}