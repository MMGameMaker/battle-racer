using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;
public class SceneHelper : MonoBehaviour
{
    [System.Serializable]
    public class SceneData
    {
        public int sceneIndex;
        public string sceneName;
    }
    [SerializeField] List<SceneData> sceneDatas = new List<SceneData>();
    public static IEnumerator DoLoadSceneAsync(string sceneName)
    {
        yield return instance.LoadScene(sceneName);
    }
    public static void DoLoadScene(string sceneName)
    {
        instance.StartCoroutine(DoLoadSceneAsync(sceneName));
    }

    private static SceneHelper instance;
    public static bool isLoaded { get; private set; }

    private void Awake()
    {
        instance = this;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
    }
    private void SceneManager_sceneUnloaded(Scene scene)
    {
        Debug.Log("unloaded: " + scene.name);
        isLoaded = false;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        Debug.Log("loaded: " + scene.name);
        isLoaded = false;
        StartCoroutine(WaitForLOad());
    }

    private IEnumerator WaitForLOad()
    {
        yield return null;
        isLoaded = true;
    }

    private async UniTask LoadScene(string sceneName)
    {
        if (SceneManager.sceneCount > 1)
        {
            var scene = SceneManager.GetSceneAt(1);
            await SceneManager.UnloadSceneAsync(scene);
        }

        await UniTask.Yield();
        await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        await UniTask.Yield();
        if (SceneManager.sceneCount > 1)
            SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
    }
}
