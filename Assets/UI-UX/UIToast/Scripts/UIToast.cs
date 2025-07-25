using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using static UIManager;

public class UIToast : MonoBehaviour
{
    [SerializeField]
    private RectTransform mainTransform = null;
    [SerializeField]
    private RectTransform contentTransform = null;
    [SerializeField]
    private RectTransform contentStartPos = null;
    [SerializeField]
    private CanvasGroup contents = null;
    [SerializeField]
    private RectTransform contentsRect = null;

    [SerializeField]
    private Button closeButton = null;
    [SerializeField]
    private TextMeshProUGUI tmpMessage = null;

    //[SerializeField]
    //private int maxLengthToSplit = 36;
    [SerializeField]
    private GameObject deActive = null;

    [Header("Options")]
    [SerializeField]
    private Image iconImage = null;
    [SerializeField]
    private Sprite iconLoading = null;
    [Space(10)]
    [SerializeField]
    private Sprite iconNotification = null;
    public static Sprite IconNotification => instance?.iconNotification;
    [SerializeField]
    private string soundNotification = null;
    [Space(10)]
    [SerializeField]
    private Sprite iconError = null;
    public static Sprite IconError => instance?.iconError;
    [SerializeField]
    private string soundError = null;
    [SerializeField]
    [Space(10)]
    private Sprite iconUnlock = null;
    public static Sprite IconUnlock => instance?.iconUnlock;
    [SerializeField]
    private string soundUnlock = null;
    [SerializeField]
    [Space(10)]
    private Sprite iconTip = null;
    public static Sprite IconTip => instance?.iconTip;

    private static float elapsedTime = 0;
    public static ToastType toastType = ToastType.Loading;
    public static UIAnimStatus Status = UIAnimStatus.IsHide;

    private static UIToast instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        closeButton.onClick.AddListener(() => Hide());
        deActive.SetActive(false);
        contentTransform.gameObject.SetActive(false);
        mainTransform.anchoredPosition = startAnchoredPosition2D;
    }

    public static void ShowNotice(string mes)
    {
        if (instance)
            instance.Show(mes, ToastType.Notification);
        else
            Debug.LogWarning("[UIToast]  instance NULL");
    }

    public static void ShowError(string mes)
    {
        if (instance)
            instance.Show(mes, ToastType.Error, 5f);
        else
            Debug.LogWarning("[UIToast]  instance NULL");
    }

    public static void ShowUnlock(string mes, Sprite sprite = null)
    {
        if (instance)
            instance.Show(mes, ToastType.Unlock, 5f, sprite);
        else
            Debug.LogWarning("[UIToast]  instance NULL");
    }

    public static void ShowLoading(string mes, float timeAutoHide = 3f, Sprite icon = null)
    {
        if (instance)
            instance.Show(mes, ToastType.Loading, timeAutoHide, icon);
        else
            Debug.LogWarning("[UIToast]  instance NULL");
    }

    public static void Show(string mes, Sprite icon, ToastType type, float timeAutoHide)
    {
        if (instance)
            instance.Show(mes, type, timeAutoHide, icon);
        else
            Debug.LogWarning("[UIToast]  instance NULL");
    }
    public static void Show(string mes, ToastType type, float timeAutoHide)
    {
        if (instance)
            instance.Show(mes, type, timeAutoHide);
        else
            Debug.LogWarning("[UIToast]  instance NULL");
    }

    public void Show(string mes)
    {
        Show(mes, ToastType.Notification);
    }

    private void Show(string mes = "", ToastType type = ToastType.Loading, float timeAutoHide = 3f, Sprite icon = null, string soundEffect = "")
    {
        toastType = type;
        //if (!string.IsNullOrEmpty(mes) && mes.Length > maxLengthToSplit)
        //{
        //    if (mes.Contains("...! "))
        //        mes = mes.Replace("...! ", "...!\n");
        //    else if (mes.Contains("... "))
        //        mes = mes.Replace("... ", "...\n");
        //    else if (mes.Contains(". "))
        //        mes = mes.Replace(". ", ".\n");
        //    else if (mes.Contains("!? "))
        //        mes = mes.Replace("!? ", "!?\n");
        //    else if (mes.Contains("! "))
        //        mes = mes.Replace("! ", "!\n");
        //}

        elapsedTime = timeAutoHide;

        if (iconImage && icon == null)
        {
            switch (toastType)
            {
                case ToastType.Loading:
                    if (iconLoading)
                        iconImage.sprite = iconLoading;
                    break;
                case ToastType.Notification:
                    if (iconNotification)
                        iconImage.sprite = iconNotification;
                    if (!string.IsNullOrEmpty(soundNotification))
                        soundEffect = soundNotification;
                    break;
                case ToastType.Error:
                    if (iconError)
                        iconImage.sprite = iconError;
                    if (!string.IsNullOrEmpty(soundError))
                        soundEffect = soundError;
                    break;
                case ToastType.Unlock:
                    if (iconUnlock)
                        iconImage.sprite = iconUnlock;
                    if (!string.IsNullOrEmpty(soundUnlock))
                        soundEffect = soundUnlock;
                    break;
            }
        }

        else
        {
            iconImage.sprite = icon;
        }

        if (!string.IsNullOrEmpty(soundEffect))
            SoundManager.Play(soundEffect);

        if (Status == UIAnimStatus.IsShow && tmpMessage.text == mes.Trim())
        {
            Debug.LogWarning("Sample mes... return");
            return;
        }

        StopAllCoroutines();

        if (Status == UIAnimStatus.IsHide)
        {
            Status = UIAnimStatus.IsAnimationShow;

            contentTransform.gameObject.SetActive(true);
            contentTransform.DOKill(true);

            tmpMessage.text = mes.Trim();
            tmpMessage.gameObject.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(tmpMessage.rectTransform);

            contentTransform.anchoredPosition = contentStartPos.anchoredPosition;
            contentTransform.DOAnchorPos(startAnchoredPosition2D, 0.125f)
                .SetDelay(0.05f)
                .OnStart(() =>
                {
                    contents.DOFade(!string.IsNullOrEmpty(mes) ? 1 : 0, 0);
                    tmpMessage.gameObject.SetActive(true);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(contentsRect);
                })
                .OnComplete(() =>
                {
                    tmpMessage.gameObject.SetActive(true);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(contentsRect);
                    Status = UIAnimStatus.IsShow;
                });
        }
        else
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentsRect);
            contents.DOKill(true);
            contents.DOFade(0, 0.15f)
                .OnComplete(() =>
                {
                    tmpMessage.text = mes.Trim();
                    tmpMessage.gameObject.SetActive(false);
                    contents.DOFade(!string.IsNullOrEmpty(mes) ? 1 : 0, 0.15f)
                    .OnPlay(() =>
                    {
                        tmpMessage.gameObject.SetActive(true);
                        LayoutRebuilder.ForceRebuildLayoutImmediate(contentsRect);
                    })
                    .OnComplete(() =>
                    {
                        tmpMessage.gameObject.SetActive(true);
                        LayoutRebuilder.ForceRebuildLayoutImmediate(contentsRect);
                    });
                });
        }

        StartCoroutine(AutoHide());
        deActive.SetActive(toastType == ToastType.Loading);
    }

    public void Close()
    {
        if (toastType != ToastType.Loading)
            Hide();
    }

    public static void Hide(bool active = true)
    {
        elapsedTime = 0;
        instance.StartCoroutine(instance?.AutoHide(active));
    }

    private IEnumerator AutoHide(bool active = true)
    {
        while (elapsedTime > 0)
        {
            elapsedTime -= Time.deltaTime;
            yield return null;
        }
        deActive.SetActive(!active);
        contentTransform.DOAnchorPos(contentStartPos.anchoredPosition, 0.125f)
            .OnComplete(() =>
            {
                Status = UIAnimStatus.IsHide;
                contentTransform.gameObject.SetActive(false);
            });
    }

    private int indexTest = 0;
    public void Test()
    {
        string[] stringList = new string[]
        {
            "With music we are the one!",
            "Let's the music!",
            "Let's the music!",
            "",
            "Got music!?",
            "Touching heaven on frequency!",
            "Oops... Don't do that! Touching...!"
        };
        ShowNotice(stringList[indexTest]);

        indexTest++;
        if (indexTest >= stringList.Length)
            indexTest = 0;
    }
}

public enum ToastType
{
    Loading,
    Notification,
    Error,
    Unlock
}