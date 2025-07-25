using DG.Tweening;
using MyBox;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private bool loadAllSoundsAtStart = true;
    [SerializeField]
    [Tooltip("Add sound to Resources/Sounds")]
    private string soundPath = "Sounds";

    [SerializeField]
    private string soundIntro = "sfx_intro";

    [SerializeField]
    private float soundVolume = 0.65f;

    [SerializeField]
    private Toggle soundToggle = null;
    private static Dictionary<string, AudioClip> allSounds = new Dictionary<string, AudioClip>();

    [SerializeField]
    public AudioSource soundSource = null;

    [SerializeField]
    private int soundObjTempList = 10;

    #region Base
    [SerializeField]
    private Transform parrentTransform = null;
    public static SoundManager instance { get; set; }

    public static bool IsOn => instance?.soundToggle != null ? instance.soundToggle.isOn : true;

    public static string TAG
    {
        get
        {
            if (instance != null)
                return "[" + instance.GetType().Name + "] ";
            return "";
        }
    }

    public void Awake()
    {
        instance = this;
        parrentTransform = transform;
        InitSoundObjTempList();
    }

    private void OnDestroy()
    {
        DOTween.Kill(instance.gameObject);
    }
    #endregion

    private void Start()
    {
        if (loadAllSoundsAtStart)
        {
            LoadAllSounds();
        }

        if (soundToggle)
            soundToggle.onValueChanged.AddListener(ToggleSound);
        else
            Debug.LogWarning(TAG + " soundToggle NULL");

        if (!string.IsNullOrEmpty(soundIntro) && (soundToggle && soundToggle.isOn || soundToggle == null) && MusicManager.IsOn)
            Play(soundIntro);
    }

    public void ToggleSound(bool isOn)
    {
        if (isOn)
            Play(SoundHelper.ButtonClick);
    }

    public static void LoadAllSounds(string path = "")
    {
        if (string.IsNullOrEmpty(path) && instance)
            path = instance.soundPath;

        var resources = Resources.LoadAll<AudioClip>(path);
        if (resources != null)
        {
            foreach (var i in resources)
            {
                if (!allSounds.ContainsKey(i.name))
                    allSounds.Add(i.name, i as AudioClip);
            }
            resources = null;
        }
        else
        {
            Debug.LogError("[LoadAllSounds] " + path + " is not correct!?");
        }
    }

    public void UIPlay(string fileName)
    {
        Play(fileName);
    }

    public static void Play(string fileName, float delayTime = 0)
    {
        DOVirtual.DelayedCall(delayTime, () =>
        {
            if (instance && !string.IsNullOrEmpty(fileName) && allSounds != null && (instance.soundToggle == null || (instance.soundToggle && instance.soundToggle.isOn)))
            {
                if (!allSounds.ContainsKey(fileName))
                {
                    var sound = Resources.Load<AudioClip>(instance.soundPath + "/" + fileName);
                    if (sound != null && !allSounds.ContainsKey(fileName))
                        allSounds.Add(fileName, sound);
                }

                if (allSounds.ContainsKey(fileName))
                    PlayTemp(allSounds[fileName]);
                else
                    Debug.LogWarning(TAG + " There is no sound file with the name [" + fileName + "] in any of the Resources folders.\n Check that the spelling of the fileName (without the extension) is correct or if the file exists in under a Resources folder");
            }
        }).SetId(instance.gameObject);
    }

    private static void PlayClipAt(AudioClip clip, bool setPos = false, Vector3 pos = new Vector3())
    {
        var tempGO = new SoundObj("TempAudio - " + clip.name, instance?.parrentTransform);
        if (setPos)
            tempGO.gameObject.transform.position = pos;
        tempGO.aSource.volume = instance.soundVolume;
        tempGO.aSource.PlayOneShot(clip);
        Destroy(tempGO.gameObject, clip.length);
    }

    private static List<SoundObj> soundObjList = new List<SoundObj>();

    public void InitSoundObjTempList()
    {
        for (int i = 0; i < soundObjTempList; i++)
        {
            soundObjList.Add(new SoundObj("SoundObj", parrentTransform));
        }
    }

    public static void PlayTemp(AudioClip clip, bool setPos = false, Vector3 pos = new Vector3())
    {
        instance.soundSource.PlayOneShot(clip);
        //var check = soundObjList.FirstOrDefault(x => x.aSource.clip == clip && x.aSource.isPlaying == false);
        //if (check == null)
        //    check = soundObjList.FirstOrDefault(x => x.aSource.isPlaying == false);
        //if (check != null)
        //{
        //    if (setPos)
        //        check.gameObject.transform.position = pos;
        //    check.aSource.volume = instance.soundVolume;
        //    check.aSource.PlayOneShot(clip);
        //}
        //else
        //{
        //    PlayClipAt(clip);
        //}
    }


    public void TestComboSound()
    {
        foreach (var i in allSounds)
            Play(i.Key);
    }

    public class SoundObj
    {
        public GameObject gameObject;
        public AudioSource aSource;

        public SoundObj(string name, Transform parent = null)
        {
            gameObject = new GameObject(name);
            gameObject.transform.parent = parent;
            aSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public static void StopAll()
    {
        foreach (var sound in soundObjList)
            sound.aSource.mute = true;
    }

    public static void PlayAll()
    {
        foreach (var sound in soundObjList)
            sound.aSource.Play();
    }
}
