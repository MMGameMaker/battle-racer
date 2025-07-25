using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UITextEffectManager : MonoBehaviour
{
    [SerializeField]
    private Text text = null;
    [SerializeField]
    private int defaultFontSize = 28;
    [SerializeField]
    private float moveYOffset = 60;
    [SerializeField]
    private float timeAnimation = 0.25f;
    [SerializeField]
    private int initialPoolSize = 10;

    private void Start()
    {
        text.CreatePool(initialPoolSize);
        text.gameObject.SetActive(false);
    }

    public void DoAnimation(string content, Color color, Vector3 startPos, int defaultFontSize = 0)
    {
        DoAnimation(content, color, startPos, moveYOffset, timeAnimation, defaultFontSize);
    }

    public void DoAnimation(string content, Color color, Vector3 startPos, float moveYOffset, float timeAnimation = 0, int defaultFontSize = 0)
    {
        var temp = this.text.Spawn(transform);
        temp.fontSize = defaultFontSize == 0 ? this.defaultFontSize : defaultFontSize;
        temp.text = content;
        temp.color = color;
        temp.transform.SetPosition(startPos);
        temp.DOOffsetY(moveYOffset, timeAnimation, 0.2f);
        temp.DOScale(timeAnimation * 0.125f, 0, 1, 0.1f);
        temp.DOFadeInOut(timeAnimation * 0.125f, timeAnimation * 0.75f, timeAnimation * 0.125f, 0f, 1f, 0f, true, temp.Recycle);
    }
}

public static class GraphicExtend
{
    public static void DOFadeIn(this Graphic graphic, float timeAnimation = 0.5f, float delayTime = 0, float startValue = 0f, float endValue = 1f, Action actionOnDone = null)
    {
        graphic.DOFadeInOut(timeAnimation, 0f, 0f, startValue, endValue, delayTime, false, actionOnDone);
    }

    public static void DOFadeInOut(this Graphic graphic, float timeAnimationIn = 0.125f, float timeAppear = 0.75f, float timeAnimationOut = 0.125f, float startValue = 0, float endValue = 1, float delayTime = 0, bool reverse = true, Action actionOnDone = null)
    {
        graphic.SetAlpha(startValue);
        graphic.DOFade(endValue, timeAnimationIn)
            .SetDelay(delayTime)
            .OnComplete(() =>
            {
                if (reverse)
                {
                    graphic.DOFade(startValue, timeAnimationOut)
                    .SetDelay(timeAppear)
                    .OnComplete(() => actionOnDone?.Invoke());
                }
            });
    }

    public static void DOScaleInOut(this Graphic graphic, float timeAnimation = 0.5f, Action actionOnDone = null)
    {
        graphic.DOScaleInOut(timeAnimation * 0.125f, timeAnimation * 0.75f, timeAnimation * 0.125f, actionOnDone);
    }

    public static void DOScale(this Graphic graphic, float timeAnimation = 0.25f, float startValue = 0, float endValue = 1, float timeDelay = 0.1f, Action actionOnDone = null)
    {
        var rect = graphic.rectTransform;
        rect.SetScale(startValue);
        rect.DOScale(endValue, timeAnimation)
            .SetDelay(timeDelay)
            .OnComplete(() => actionOnDone?.Invoke());
    }

    public static void DOScaleInOut(this Graphic graphic, float timeIn = 0.125f, float timeAppear = 0.75f, float timeOut = 0.125f, Action actionOnDone = null)
    {
        var rect = graphic.rectTransform;
        rect.SetScale(0);
        rect.DOScale(1, timeIn)
            .OnComplete(() =>
            {
                rect.DOScale(0, timeOut)
                .SetDelay(timeAppear)
                .OnComplete(() => actionOnDone?.Invoke());
            });
    }

    public static void DoMoveY(this Graphic graphic, float posY = 60, float timeAnimation = 0.25f, Action actionOnDone = null)
    {
        graphic.rectTransform.DOAnchorPosY(posY, timeAnimation).OnComplete(() => actionOnDone?.Invoke());
    }

    public static void DOOffsetY(this Graphic graphic, float yOffset = 60, float timeAnimation = 0.25f, float timeDelay = 0.25f, Action actionOnDone = null)
    {
        var rect = graphic.rectTransform;
        rect.DOAnchorPosY(rect.anchoredPosition.y + yOffset, timeAnimation).OnComplete(() => actionOnDone?.Invoke()).SetDelay(timeDelay);
    }

    public static void SetAnchorPos(this Graphic graphic, Vector2 vector2)
    {
        graphic.rectTransform.anchoredPosition = vector2;
    }
}

public static class TextExtend
{
    /// <param name="startValue"></param>
    /// <param name="endValue"></param>
    /// <param name="delayTime"></param>
    /// <param name="timeAnimation">-1 = autoTime</param>
    /// <param name="onChanged">On value changed</param>
    /// <param name="onDone">On completed</param>
    public static void DOText(this Text uiText, int startValue, int endValue, float timeAnimation = -1, float delayTime = 0.1f, string format = "{0}", TweenCallback<int> onChanged = null, TweenCallback onDone = null)
    {
        int nextValue = endValue;
        int tempValue = startValue;
        DOTween.Kill(uiText.GetInstanceID());
        if (timeAnimation == -1f)
            timeAnimation = Mathf.Clamp(endValue * 0.01f, 0.25f, 1.5f);
        DOVirtual.Float(startValue, endValue, timeAnimation, (e) =>
        {
            tempValue = Mathf.FloorToInt(e);
            if (tempValue != nextValue)
            {
                nextValue = tempValue;
                uiText.text = string.Format(format, nextValue);
                onChanged?.Invoke(nextValue);
            }
        })
        .SetDelay(delayTime)
        .OnComplete(() =>
        {
            uiText.text = string.Format(format, endValue);
            //uiText.transform.DoScale();
            onDone?.Invoke();
        })
        .SetId(uiText.GetInstanceID());
    }

    public static void DOText(this Text uiText, string startValue, string endValue, float timeAnimation = 0.25f, float delayTime = 0.1f, string fomat = "{0}", TweenCallback onDone = null, ScrambleMode scrambleMode = ScrambleMode.Uppercase)
    {
        uiText.text = startValue;
        uiText.DOText(endValue, timeAnimation, false, scrambleMode)
            .SetDelay(delayTime)
            .OnComplete(() => onDone?.Invoke());
    }
}
