using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class LevelManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ButtonMethod]
    private void Victory()
    {
        GameStateManager.WaitComplete(null);
    }

    [ButtonMethod]
    private void Lose()
    {
        GameStateManager.WaitGameOver(null);
    }

    [ButtonMethod]
    private void Reborn()
    {
        GameStateManager.RebornContinue(null);
    }
}
