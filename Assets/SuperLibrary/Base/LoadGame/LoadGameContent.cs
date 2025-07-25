using System.Collections;
using UnityEngine;

public class LoadGameContent : MonoBehaviour
{
    private static LoadGameContent instance { get; set; }

    private void Awake()
    {
        instance = this;
    }

    public static void PrepairDataToPlay()
    {
        instance.StartCoroutine(instance.DoPrepairDataToPlay());
    }

    private IEnumerator DoPrepairDataToPlay()
    {
        UILoadGame.Init(true, null);

        yield return SceneHelper.DoLoadSceneAsync("3_Battle");

        while (!SceneHelper.isLoaded)
            yield return null;

        GameStateManager.Init(null);

        while (GameStateManager.CurrentState == GameState.LoadGame && UILoadGame.currentProcess < 1)
        {
            UILoadGame.Process();
            yield return null;
        }

        GameStateManager.Ready(null);

        UIToast.Hide();
    }

    public void ShowError(FileStatus status)
    {
        string note = "";

        if (status == FileStatus.TimeOut || status == FileStatus.NoInternet)
            note = LocalizedManager.Key("base_DownloadFirstTime") + "\n" + "\n";
        if (status == FileStatus.TimeOut)
        {
            note += LocalizedManager.Key("base_DownloadTimeOut");
        }
        else if (status == FileStatus.NoInternet)
        {
            note += LocalizedManager.Key("base_PleaseCheckYourInternetConnection");
        }
        else
        {
            note += LocalizedManager.Key("base_SomethingWrongs") + "\n ERROR #" + status;
        }
    }
}