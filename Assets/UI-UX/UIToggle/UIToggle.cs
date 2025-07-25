using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIToggle : MonoBehaviour
{
    public Toggle toggle = null;
    [SerializeField]
    private UILocalizedText status = null;
    [SerializeField]
    private string keyOn = "On";
    [SerializeField]
    private string keyOff = "Off";
    [SerializeField]
    private bool autoSaveLoad = true;
    [SerializeField]
    private GameObject objOff;
    [SerializeField]
    private GameObject objOn;
    protected static UIToggle instance = null;

    public virtual void Awake()
    {
        instance = this;

        if (toggle == null)
            toggle = GetComponent<Toggle>();

        if (toggle != null)
        {
            if (autoSaveLoad)
                toggle.isOn = PlayerPrefs.GetInt(name, toggle.isOn ? 1 : 0) == 1 ? true : false;
        }
    }

    public virtual void Start()
    {
        toggle.onValueChanged.AddListener(OnValueChanged);
        UpdateStatus();
    }

    public virtual void OnChangedAction(UnityAction<bool> action)
    {
        if (toggle && action != null)
            toggle.onValueChanged.AddListener(action);
    }

    public virtual void OnValueChanged(bool isOn)
    {
        if (autoSaveLoad)
        {
            UpdateStatus();
            SoundManager.Play(SoundHelper.ButtonClick);
            PlayerPrefs.SetInt(name, isOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        Debug.Log(name + " " + isOn);
        if (name == "BtnSound")
        {
            this.PostEvent((int)EventID.OnSoundChange, PlayerPrefs.GetInt(name));
        }
        else if (name == "BtnMusic")
        {
            this.PostEvent((int)EventID.OnMusicChange, PlayerPrefs.GetInt(name));
        }
    }

    public bool isOn
    {
        get
        {
            return toggle.isOn;
        }
        set
        {
            if (toggle)
                toggle.isOn = value;
        }
    }

    private void Reset()
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (objOff)
        {
            objOff.SetActive(!isOn);
            objOn.SetActive(isOn);
        }
        if (status)
        {
            status.Key = isOn ? keyOn : keyOff;
        }
    }
}
