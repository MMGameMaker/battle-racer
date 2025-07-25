using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIPerfectToast : MonoBehaviour
{
    public static UIPerfectToast instance;
    [SerializeField]
    private UIAnimType type = UIAnimType.Zoom;
    [SerializeField]
    private float maxTranY = 100;
    [SerializeField]
    private Text textMes = null;
    [SerializeField]
    private RectTransform textMesRectTransform = null;
    [SerializeField]
    private Color textColor = new Color32(255, 25, 10, 255);
    private static Vector3 scaleMax = new Vector3(1f, 1f, 1f);
    private static Vector3 scaleRe = new Vector3(0.55f, 0.55f, 0.55f);
    private static Vector3 scaleMin = new Vector3(0.50f, 0.50f, 0.50f);

    private static bool isAnimation = false;
    private static bool waitAnimationDone = false;

    private static float timeIn = 0.25f;
    private static float timeDelay = 0.25f;

    private float beginTranY;

    private void Awake()
    {
        if (textMes == null)
        {
            Debug.LogError("[PerfectToast] textMes NULL");
            return;
        }
        if (textMesRectTransform == null)
        {
            Debug.LogError("[PerfectToast] textMesRectTransform NULL");
            return;
        }

        textColor = textMes.color;
        textMes.text = "";
        beginTranY = textMesRectTransform.anchoredPosition.y;
        instance = this;
    }

    public void Show(string mes)
    {
        Show(mes, 1.5f);
    }

    public void Show(string mes, float timeAutoHide, bool waitAnimation = false, Color color = new Color())
    {
        if (isAnimation && waitAnimationDone)
            return;

        timeIn = timeAutoHide * 0.25f;
        timeDelay = timeAutoHide * 0.75f;
        waitAnimationDone = waitAnimation;
        textMes.text = mes;
        if (color == new Color())
            color = textColor;
        textMes.color = color;
        textMes.DOKill(true);
        textMesRectTransform.DOKill(true);
        if (isAnimation == false)
        {
            textMes.SetAlpha(0);
            if (type == UIAnimType.Scale)
                DoScaleAnimation(scaleMax, timeIn, timeDelay);
            else
                DoZoomAnimation(-500, timeIn, timeDelay);
        }
        else
        {
            textMes.SetAlpha(0);
            if (type == UIAnimType.Scale)
                DoScaleAnimation(scaleRe, timeIn, timeDelay);
            else
                DoZoomAnimation(-150, timeIn, timeDelay);
        }
    }

    private void DoScaleAnimation(Vector3 start, float timeInAnimation, float timeDelayToHideAnimation)
    {
        isAnimation = true;
        textMesRectTransform.SetLocalPosition(Vector3.zero);
        textMesRectTransform.SetScale(start);
        textMes.DOFade(0.95f, timeInAnimation);
        textMesRectTransform.DOScale(scaleMin, timeInAnimation).OnComplete(() =>
        {
            textMes.DOFade(0, timeInAnimation).SetDelay(timeDelayToHideAnimation);
            if (maxTranY != 0)
            {
                textMesRectTransform.DOAnchorPosY(maxTranY, timeInAnimation).SetDelay(timeDelayToHideAnimation).OnComplete(() =>
                {
                    isAnimation = false;
                });
            }
            else
            {
                textMesRectTransform.DOScale(Vector3.zero, timeInAnimation).SetDelay(timeDelayToHideAnimation).OnComplete(() =>
                {
                    isAnimation = false;
                });
            }
        });
    }

    private void DoZoomAnimation(float start, float timeInAnimation, float timeDelayToHideAnimation)
    {
        textMesRectTransform.DOAnchorPosY(beginTranY, 0);
        textMesRectTransform.SetLocalZ(start);
        textMes.DOFade(1, timeInAnimation);
        textMesRectTransform.DOLocalMoveZ(0, timeInAnimation).OnComplete(() =>
        {
            textMes.DOFade(0, timeInAnimation).SetDelay(timeDelayToHideAnimation);
            textMesRectTransform.DOLocalMoveZ(-start, timeInAnimation).SetDelay(timeDelayToHideAnimation).OnComplete(() =>
            {
                isAnimation = false;
            });
        });
    }
}
