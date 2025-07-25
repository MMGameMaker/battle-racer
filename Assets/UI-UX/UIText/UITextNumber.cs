using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UITextNumber : MonoBehaviour
{
    [SerializeField]
    public Text uiText = null;
    public string soundCount = "sfx_score_loop";
    public string soundCompleted = "sfx_score_stop";
    [SerializeField]
    public string Text
    {
        set
        {
            if (uiText)
            {
                uiText.text = value;
            }
        }
    }

    private void Awake()
    {
        if (uiText == null)
            uiText = GetComponent<Text>();
    }

    public void DOAnimation(int startValue, int endValue, float timeAnimation = 0.5f, float delayTime = 0f, string fomat = "{0}", TweenCallback onDone = null)
    {
        uiText.DOText(startValue, endValue, timeAnimation, delayTime, fomat,
            (s) =>
            {
                SoundManager.Play(soundCount);
            },
            () =>
            {
                SoundManager.Play(soundCompleted);
            });
    }
}
