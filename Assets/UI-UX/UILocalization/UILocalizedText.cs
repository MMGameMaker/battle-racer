using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UILocalizedText : MonoBehaviour
{
    private Text uiText;
    public TextStyle textStyle = TextStyle.Normal;
    [SerializeField]
    private string key;
    public string Key
    {
        get
        {
            return key;
        }

        set
        {
            if (key != value)
            {
                key = value;
                LocalizedManager.UpdateText(this);
            }
        }
    }

    void Awake()
    {
        uiText = GetComponent<Text>();
    }

    void Start()
    {
        if (!LocalizedManager.uiTextList.Contains(this))
            LocalizedManager.uiTextList.Add(this);
    }

    void OnEnable()
    {
        if (!LocalizedManager.uiTextList.Contains(this))
        {
            LocalizedManager.uiTextList.Add(this);
            LocalizedManager.UpdateText(this);
        }
    }

    public string Text
    {
        set
        {
            if (uiText && value != uiText.text)
            {
                switch (textStyle)
                {
                    case TextStyle.Normal:
                        uiText.text = value;
                        break;
                    case TextStyle.ToLower:
                        uiText.text = value.ToLower();
                        break;
                    case TextStyle.ToUpper:
                        uiText.text = value.ToLower();
                        break;
                }
            }
        }
    }

    public enum TextStyle
    {
        Normal = 0,
        ToLower = 1,
        ToUpper = 2
    }
}

