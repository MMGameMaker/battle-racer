using Base;
using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;

public class AutoRecycle : MonoBehaviour
{
    public float ScheduledOffTime = 5;
    public bool recycleInInitState = false;
    private Coroutine OffRoutine;
    private void Awake()
    {
        GameStateManager.OnStateChanged += GameStateManager_OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= GameStateManager_OnGameStateChanged;
    }

    private async UniTask GameStateManager_OnGameStateChanged(GameState current, GameState last, object data = null)
    {
        if (recycleInInitState && (current == GameState.Init || current == GameState.Idle))
        {
            if (gameObject != null && gameObject.activeSelf)
            {
                if (OffRoutine != null)
                {
                    StopCoroutine(OffRoutine);
                }
                gameObject.Recycle();
            }
        }
    }

    private void OnEnable()
    {
        if (OffRoutine != null)
        {
            StopCoroutine(OffRoutine);
        }

        OffRoutine = StartCoroutine(_ScheduledOff());
    }
    IEnumerator _ScheduledOff()
    {
        if (ScheduledOffTime <= 0)
            yield return null;
        else
            yield return new WaitForSeconds(ScheduledOffTime);
        gameObject.Recycle();
    }
}
