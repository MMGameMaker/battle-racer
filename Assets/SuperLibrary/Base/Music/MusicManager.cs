using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSourceReal = null;
    [SerializeField]
    private AudioClip defaultClip = null;
    public static AudioSource AudioSourceReal { get => instance?.audioSourceReal; }

    public delegate void ClipChangedDelegate(string clipName);
    public static event ClipChangedDelegate OnClipChanged;

    [Header("Options - GameObjects")]
    [SerializeField]
    private UIToggle musicToggle = null;

    [SerializeField]
    private float maxVolume = 1f;
    public static float MaxVolume => instance != null ? instance.maxVolume : 1f;

    public static float latency { get; set; }

    private GameConfig gameConfig => DataManager.GameConfig;

    private static AudioClip audioClip = null;
    private static AudioClip AudioClip
    {
        get => audioClip;
        set
        {
            if (audioClip == null || (audioClip != value && audioClip.name != value.name))
            {
                audioClip = value;
                OnClipChanged?.Invoke(audioClip.name);
            }
        }
    }

    public static bool IsPlaying => AudioSourceReal.isPlaying;
    public static bool IsOn => instance?.musicToggle != null ? instance.musicToggle.isOn : true;

    public int currentSoundPlay = 0;

    private bool isStartGame;

    public static MusicManager instance = null;

    private void Awake()
    {
        instance = this;
        if (audioSourceReal == null)
            audioSourceReal = GetComponent<AudioSource>();

        this.RegisterListener((int)EventID.OnPlayMusic, SchedulePlayMusicHandle);
        GameStateManager.OnStateChanged += GameStateManager_OnStateChanged;
    }

    private void OnDestroy()
    {
        GameStateManager.OnStateChanged -= GameStateManager_OnStateChanged;
    }

    private async UniTask GameStateManager_OnStateChanged(GameState current, GameState last, object data)
    {
        if (current == GameState.Idle)
            isStartGame = false;
        else
            isStartGame = true;
    }

    private void Start()
    {
        if (musicToggle)
        {
            musicToggle.OnChangedAction((isOn) =>
            {
                if (isOn)
                    FadeIn(null, MaxVolume);
                else
                    FadeOut();
            });
        }
    }

    public void SchedulePlayMusicHandle(object obj)
    {
        string fileName = (string)obj;
        StartCoroutine(DoPlay(fileName, true));
    }

    public static IEnumerator DoPlay(string fileName, bool autoPlay, Action<bool> onDone = null)
    {
        yield return FileExtend.DOGetRes<AudioClip>((clib, status) =>
        {
            if (clib != null)
            {
                Init(onDone, clib, autoPlay);
            }
        }, "Musics/" + fileName);
    }

    public static void Init(Action<bool> actionOnDone, AudioClip clip, bool autoPlay = false, float fadeTime = 0.25f)
    {
        if (AudioClip == null)
        {
            AudioClip = clip;
            AudioSourceReal.clip = AudioClip;
            if (autoPlay)
                Play(actionOnDone);
            else
                actionOnDone?.Invoke(true);
        }
        else
        {
            if (AudioClip.name == clip.name)
            {
                if (autoPlay && !AudioSourceReal.isPlaying)
                    Play(actionOnDone);
                else
                    actionOnDone?.Invoke(true);
            }
            else
            {
                Stop(() =>
                {
                    Resources.UnloadUnusedAssets();
                    AudioClip.UnloadAudioData();
                    AudioClip = clip;
                    AudioSourceReal.clip = AudioClip;
                    if (autoPlay)
                        Play(actionOnDone);
                    else
                        actionOnDone?.Invoke(true);
                }, false, 0.25f);
            }
        }
    }

    public static void Play(Action<bool> actionOnDone, float time = 0, bool loop = true, float fadeTime = 0.5f, float audioSourceRealDelay = 0f)
    {
        DOTween.Kill("AudioSourceReal");
        if (audioSourceRealDelay > 0)
        {
            DOVirtual.DelayedCall(audioSourceRealDelay, () =>
            {
                AudioSourceReal.time = time;
                AudioSourceReal.loop = loop;
                AudioSourceReal.volume = 0;
                AudioSourceReal.pitch = 1;
                AudioSourceReal.Play();

                WaitAudioPlay(actionOnDone, time, fadeTime);
            }).SetId("AudioSourceReal");
        }
        else
        {
            AudioSourceReal.time = time;
            AudioSourceReal.loop = loop;
            AudioSourceReal.volume = 0;
            AudioSourceReal.pitch = 1;
            AudioSourceReal.Play();

            WaitAudioPlay(actionOnDone, time, fadeTime);
        }
    }

    public static void WaitAudioPlay(Action<bool> actionOnDone, float time, float fadeTime)
    {
        if (instance.WaitAudioPlayCoroutine != null)
        {
            instance.StopCoroutine(instance.WaitAudioPlayCoroutine);
        }
        instance.WaitAudioPlayCoroutine = instance.StartCoroutine(DoWaitAudioPlay(actionOnDone, time, fadeTime));
    }

    private Coroutine WaitAudioPlayCoroutine;
    private static IEnumerator DoWaitAudioPlay(Action<bool> actionOnDone, float time, float fadeTime)
    {
        var timeout = 5;
        var start = Time.time;
        var failed = false;

        while (!AudioSourceReal.isPlaying)
        {
            if (Time.time - start > timeout)
            {
                failed = true;
                break;
            }
            yield return null;
        }
        if (failed)
        {
            Debug.Log("Play Music failed!");
            actionOnDone?.Invoke(false);
            yield break;
        }
        start = Time.time;
        while (AudioSourceReal.time <= time + latency)
        {
            if (Time.time - start > timeout)
            {
                failed = true;
                break;
            }
            yield return null;
        }
        if (failed)
        {
            Debug.Log("Play Music -> wait latency failed!");
            actionOnDone?.Invoke(false);
            yield break;
        }
        actionOnDone?.Invoke(true);
        if (instance.musicToggle && instance.musicToggle.isOn)
            AudioSourceReal.DOFade(instance.maxVolume, fadeTime).SetId("AudioSourceReal");

        if (instance.isStartGame)
            instance.currentSoundPlay++;
    }

    public static void Pause()
    {
        if (instance == null)
            return;
        AudioSourceReal.Pause();
    }

    public static void UnPause(float fadeTime = 1f)
    {
        if (instance == null)
            return;
        AudioSourceReal.UnPause();
    }

    public static void Stop(Action actionOnDone, bool pitch = true, float fadeTime = 1.5f)
    {
        if (instance == null)
            return;
        if (fadeTime == 0 || AudioSourceReal.volume == 0)
        {
            AudioSourceReal.Stop();
            actionOnDone?.Invoke();
        }
        else
        {
            if (AudioSourceReal.isPlaying)
            {
                AudioSourceReal.DOFade(0, fadeTime)
                .SetId("AudioSourceReal")
                .OnUpdate(() =>
                {
                    if (pitch)
                    {
                        AudioSourceReal.pitch = AudioSourceReal.volume;
                    }
                })
                .OnComplete(() =>
                {
                    AudioSourceReal.pitch = 1;
                    AudioSourceReal.volume = MaxVolume;
                    AudioSourceReal.Stop();
                    actionOnDone?.Invoke();
                });
            }
            else
            {
                actionOnDone?.Invoke();
            }
        }
    }

    public static void FadeIn(Action actionOnDone = null, float volume = 1f, float fadeTime = 0.125f)
    {
        AudioSourceReal.DOKill();
        if (instance.musicToggle && instance.musicToggle.isOn)
            AudioSourceReal.DOFade(volume, fadeTime)
                .SetId("AudioSourceReal")
                .OnComplete(() =>
                {
                    actionOnDone?.Invoke();
                });
    }

    public static void FadeOut(Action actionOnDone = null, float fadeTime = 0.5f)
    {
        AudioSourceReal.DOKill();
        AudioSourceReal.DOFade(0, fadeTime)
            .SetId("AudioSourceReal")
            .OnComplete(() =>
            {
                actionOnDone?.Invoke();
            });
    }

    public static void SetPitch(float pitch, float timeAnim, TweenCallback actionOnDone = null, TweenCallback<float> onUpdate = null)
    {
        if (timeAnim <= 0)
        {
            AudioSourceReal.pitch = pitch;
        }
        else
        {
            AudioSourceReal.DOKill();
            AudioSourceReal.DOPitch(pitch, timeAnim)
                .SetId("AudioSourceReal")
                .SetUpdate(UpdateType.Normal, true)
                .OnUpdate(() =>
                {
                    if (instance.musicToggle && instance.musicToggle.isOn)
                        onUpdate?.Invoke(AudioSourceReal.volume);
                })
                .OnComplete(() => actionOnDone?.Invoke());
        }
    }

    public static IEnumerator Load(Action<AudioClip, FileStatus> actionOnDone, string fileId, string fileUrl = null, bool autoPlay = false, bool downloadToPlay = false)
    {
        if (string.IsNullOrEmpty(fileId))
        {
            Init(null, instance.defaultClip, true);
            yield break;
        }

        if (AudioClip != null && AudioClip.name.Equals(fileId))
        {
            if (autoPlay)
                Play(null);
            actionOnDone?.Invoke(AudioClip, FileStatus.Success);
            yield break;
        }

        string fileName = fileId + ".mp3";
        string filePath = FileExtend.FileNameToPath(fileName);

        yield return FileExtend.DOGetRes<AudioClip>((clip, status) =>
        {
            if (clip != null && status == FileStatus.Success)
            {
                clip.name = fileId;
                Init((onDone) =>
                {
                    if (onDone)
                        actionOnDone?.Invoke(clip, status);
                    else
                        actionOnDone?.Invoke(null, status);
                }, clip, autoPlay && GameStateManager.CurrentState == GameState.Idle);
            }
            else
            {
                actionOnDone?.Invoke(null, status);
            }
        }, "tracks/" + fileId);
    }
}
