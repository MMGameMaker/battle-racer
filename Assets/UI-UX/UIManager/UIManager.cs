using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

[DisallowMultipleComponent]
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Tooltip("The container that has all the ui elements")]
    private RectTransform uiContainer;
    public static RectTransform UIContainer
    {
        get
        {
            if (instance && instance.uiContainer == null)
                instance.uiContainer = FindObjectOfType<UIManager>()?.GetComponent<RectTransform>();
            return instance.uiContainer;
        }
        set
        {
            instance.uiContainer = value;
        }
    }

    [Header("Version Info")]
    [SerializeField] 
    string SDKKey = "o1IltPaBtL1UWpdzrQyS7AQCxsJt/Dpg8pf4LLIXWUI6NUPdRoDVrvivmOmtLe/V";
    [SerializeField]
    private int bundleVersion = 100000;
    public static int BundleVersion => instance.bundleVersion;
    [SerializeField]
    private List<Text> versionBuildTexts = new List<Text>();

    [SerializeField]
    private string appIdDROI = "com.zendios.demo";
    [SerializeField]
    private string appIdIOS = "123456789";
    public static string AppId
    {
        get
        {
#if UNITY_ANDROID
            return instance.appIdDROI;
#elif UNITY_IOS
            return instance.appIdIOS;
#else
            return "";
#endif
        }
    }

    public static string appVersion => Application.version;
    public static string appVerstionDetail => "ver " + appVersion + " build " + BundleVersion;

    private string urlAndroid => "http://play.google.com/store/apps/details?id=" + Application.identifier;
    public static string UrlAndroid => instance.urlAndroid;
    private string urlIOS => "http://apps.apple.com/app/id" + appIdIOS;
    public static string shareUrl = "http://www.google.com.vn";
    private static string screenshotFileName => Application.productName.Replace(" ", "_").ToLower() + ".png";

    [Header("Feedback")]
    [SerializeField]
    private string email = "";
    [SerializeField]
    private string emailCC = "";

    [Header("Screenshot")]
    public Button takeScreenshot = null;

    [Header("Hide GameObject in Platform")]
    [SerializeField]
    private List<GameObject> androidList = new List<GameObject>();
    [SerializeField]
    private List<GameObject> iOSList = new List<GameObject>();

    #region UIAnimation
    private static UIAnimation curScreen;
    public static UIAnimation CurScreen
    {
        get
        {
            return curScreen;
        }
        set
        {
            if (value != curScreen)
                curScreen = value;
        }
    }
    public static List<UIAnimation> listScreen { get; set; }

    private static UIAnimation curPopup;
    public static UIAnimation CurPopup
    {
        get
        {
            return curPopup;
        }
        set
        {
            if (value != curPopup)
                curPopup = value;
        }
    }
    public static List<UIAnimation> listPopup { get; set; }
    #endregion

    #region Screen Size
    public static Vector2 startAnchoredPosition2D = Vector2.zero;
    public delegate void ScreenSizeChangedDelegate();
    public static event ScreenSizeChangedDelegate OnSizeChanged;

    public delegate void OrientationChangedDelegate(Orientation currentOrientation);
    public static event OrientationChangedDelegate OnOrientationChanged = null;
    #endregion

    private void Awake()
    {
        instance = this;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        DebugMode.OnDebugModeChanged += DebugMode_OnDebugModeChanged;
        InitUIAnimation();
        InitSDK();
    }

    private void InitSDK()
    {
        UnityNetworkHelper.Init(SDKKey);
    }

    private void InitUIAnimation()
    {
        UIAnimation.startAnchoredPosition2D = startAnchoredPosition2D;
        UIAnimation.DoStartCoroutine = (c) => StartCoroutine(c);
        UIAnimation.CheckScreenExist = (n) => listScreen.Any(x => x.Navigation == n && x.Status == UIAnimStatus.IsAnimationShow);
        UIAnimation.CheckPopupExist = (n) => listPopup.Any(x => x.Navigation == n && x.Status == UIAnimStatus.IsAnimationShow);
        UIAnimation.AddNewScreen = (screen) => {
            if (!listScreen.Contains(screen))
                listScreen.Add(screen);
            CurScreen = screen;
        };
        UIAnimation.AddNewPopup = (popup) =>
        {
            if (!listPopup.Contains(popup))
                listPopup.Add(popup);
            CurPopup = popup;
        };

        UIAnimation.SlideAnim = (rect, toPos, time, delay, onStart, onDone, ease) => {
            rect.DOAnchorPos(toPos, time, false)
                .SetDelay(delay)
                .SetEase(ease)
                .SetUpdate(UpdateType.Normal, true)
                .OnComplete(() =>
                {
                    onDone?.Invoke();
                });
        };

        UIAnimation.FadeCanvasAnim = (canvas, time, delay, end, onStart, onDone, ease) => {
            canvas.DOFade(end, time)
                .SetDelay(delay)
                .SetEase(ease)
                .SetUpdate(UpdateType.Normal, true)
                .OnStart(() =>
                {
                    onStart?.Invoke();
                })
                .OnComplete(() =>
                {
                    onDone?.Invoke();
                });
        };
        UIAnimation.FadeImageAnim = (img, time, delay, end, onStart, onDone, ease) => {
            if (img == null)
            {
                Debug.LogError("[UIAnimation] DoScale: " + img.name + " NULL");
                return;
            }

            img.DOKill();
            img.DOFade(end, time)
                .OnStart(() =>
                {
                    onStart?.Invoke();
                })
                .OnComplete(() =>
                {
                    onDone?.Invoke();
                })
                .SetEase(ease)
                .SetDelay(delay);
        };
        UIAnimation.SoundPlay = (sound) => {
            Debug.LogWarning($"Play sound '{sound}'");
        };
    }
    private void DebugMode_OnDebugModeChanged(bool isOn)
    {
        if (isOn)
            takeScreenshot?.gameObject.SetActive(true);
    }

    private void Start()
    {
        DOTween.Init();

        listScreen = new List<UIAnimation>();
        listPopup = new List<UIAnimation>();

        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (Application.platform == RuntimePlatform.Android)
        {
            foreach (var i in androidList)
                i.SetActive(false);
            shareUrl = urlAndroid;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            foreach (var i in iOSList)
                i.SetActive(false);
            shareUrl = urlIOS;
        }

        foreach (var i in versionBuildTexts)
            if (i) i.text = appVerstionDetail;

        if (takeScreenshot)
        {
            takeScreenshot.gameObject.SetActive(false);
            takeScreenshot.onClick.AddListener(TakeScreenshotAndShare);
        }

        StartCoroutine(GetScreenSize());
        StartCoroutine(GetOrientation());
    }

    #region Methods for UpdateUiContainer, Goback
    public static void ResetAllUIAnimaion(Action actionOnDone)
    {
        foreach (var i in listPopup)
            i.Hide();

        foreach (var i in listScreen)
            i.Hide();

        actionOnDone?.Invoke();
    }
    #endregion

    #region IEnumerators - GetScreenSize, GetOrientation

    public static bool firstPass = true;
    public static UIScreenRect uiScreenRect;

    [Tooltip("Scale gameObject to match UI")]
    [Header("ScaleRatio")]
    [SerializeField]
    private List<Transform> scaleRatioTransform = null;

    public static float scaleRatio
    {
        get
        {
            if (screenRatio <= 1)
                return 1.22f;
            else if (screenRatio <= 1.34)
                return 1.12f;
            else if (screenRatio <= 1.67)
                return 1.02f;
            else if (screenRatio <= 1.78)
                return 1.00f;
            else if (screenRatio <= 2.00)
                return 0.96f;
            else if (screenRatio <= 2.06)
                return 0.95f;
            else
                return 1;
        }
    }

    public static float screenRatio = 1.78f;

    [Serializable]
    public class UIScreenRect
    {
        public Vector2 size = Vector2.zero;
        public Vector2 position = Vector2.zero;
    }

    public enum Position
    {
        ParentPosition,
        LocalPosition,
        TopScreenEdge,
        RightScreenEdge,
        BottomScreenEdge,
        LeftScreenEdge,
    }

    public static Vector2 GetPosition(RectTransform rectTransform, Position position)
    {
        try
        {
            RectTransform parent = rectTransform.parent.GetComponent<RectTransform>();  //We need to do this check because when we Instantiate a notification we need to use the uiContainer if the parent is null.
            if (parent == null)
                parent = UIContainer;

            Vector3 targetPosition = startAnchoredPosition2D;

            Canvas tempCanvas = rectTransform.GetComponent<Canvas>();
            Canvas rootCanvas = null;

            if (tempCanvas == null) //this might be a button or an UIElement that does not have a Canvas component (this should not happen)
            {
                rootCanvas = rectTransform.root.GetComponentInChildren<Canvas>();
            }
            else
            {
                rootCanvas = tempCanvas.rootCanvas;
            }

            Rect rootCanvasRect = rootCanvas.GetComponent<RectTransform>().rect;
            float xOffset = rootCanvasRect.width / 2 + rectTransform.rect.width * rectTransform.pivot.x;
            float yOffset = rootCanvasRect.height / 2 + rectTransform.rect.height * rectTransform.pivot.y;

            var positionAdjustment = Vector3.zero;
            var positionFrom = Vector3.zero;

            switch (position)
            {
                case Position.ParentPosition:
                    if (parent == null)
                        return targetPosition;

                    targetPosition = new Vector2(parent.anchoredPosition.x + positionAdjustment.x,
                                                 parent.anchoredPosition.y + positionAdjustment.y);
                    break;

                case Position.LocalPosition:
                    if (parent == null)
                        return targetPosition;

                    targetPosition = new Vector2(positionFrom.x + positionAdjustment.x,
                                                 positionFrom.y + positionAdjustment.y);
                    break;

                case Position.TopScreenEdge:
                    targetPosition = new Vector2(positionAdjustment.x + UIManager.startAnchoredPosition2D.x,
                                                 positionAdjustment.y + yOffset);
                    break;

                case Position.RightScreenEdge:
                    targetPosition = new Vector2(positionAdjustment.x + xOffset,
                                                 positionAdjustment.y + UIManager.startAnchoredPosition2D.y);
                    break;

                case Position.BottomScreenEdge:
                    targetPosition = new Vector2(positionAdjustment.x + UIManager.startAnchoredPosition2D.x,
                                                 positionAdjustment.y - yOffset);
                    break;

                case Position.LeftScreenEdge:
                    targetPosition = new Vector2(positionAdjustment.x - xOffset,
                                                 positionAdjustment.y + UIManager.startAnchoredPosition2D.y);
                    break;

                //case Position.TopLeft:
                //    targetPosition = new Vector2(positionAdjustment.x - xOffset,
                //                                 positionAdjustment.y + yOffset);
                //    break;

                //case Position.TopCenter:
                //    targetPosition = new Vector2(positionAdjustment.x,
                //                                 positionAdjustment.y + yOffset);
                //    break;

                //case Position.TopRight:
                //    targetPosition = new Vector2(positionAdjustment.x + xOffset,
                //                                 positionAdjustment.y + yOffset);
                //    break;

                //case Position.MiddleLeft:
                //    targetPosition = new Vector2(positionAdjustment.x - xOffset,
                //                                 positionAdjustment.y);
                //    break;

                //case Position.MiddleCenter:
                //    targetPosition = new Vector2(positionAdjustment.x,
                //                                 positionAdjustment.y);
                //    break;

                //case Position.MiddleRight:
                //    targetPosition = new Vector2(positionAdjustment.x + xOffset,
                //                                 positionAdjustment.y);
                //    break;

                //case Position.BottomLeft:
                //    targetPosition = new Vector2(positionAdjustment.x - xOffset,
                //                                 positionAdjustment.y - yOffset);
                //    break;

                //case Position.BottomCenter:
                //    targetPosition = new Vector2(positionAdjustment.x,
                //                                 positionAdjustment.y - yOffset);
                //    break;

                //case Position.BottomRight:
                //    targetPosition = new Vector2(positionAdjustment.x + xOffset,
                //                                 positionAdjustment.y - yOffset);
                //    break;

                default:
                    Debug.LogWarning("[UIAnimaion] This should not happen! DoMoveIn in UIAnimator went to the default setting!");
                    break;
            }

            //Debug.Log("[UIAnimaion] GetPosition: " + targetPosition);
            return targetPosition;
        }
        catch (Exception ex)
        {
            Debug.LogError("[UIAnimaion] GetPosition: " + rectTransform.name + " " + ex.Message + "\n" + ex.StackTrace);
            return new Vector3();
        }
    }

    private IEnumerator GetScreenSize()
    {
        int infiniteLoopBreak = 0;

        while (firstPass)
        {
            yield return new WaitForEndOfFrame();
            UpdateUIScreenRect();

            if (firstPass)  //this check is needed since in the first frame of the application the uiScreenRect is (0,0); only from the second frame can we get the screen size values
                firstPass = false;

            infiniteLoopBreak++;
            if (infiniteLoopBreak > 1000)
                break;
        }
    }

    public static void UpdateUIScreenRect()
    {
        uiScreenRect = new UIScreenRect();
        uiScreenRect.size = instance.uiRect.size;
        uiScreenRect.position = instance.uiRect.position;
        screenRatio = uiScreenRect.size.y / uiScreenRect.size.x;
        if (instance?.scaleRatioTransform != null)
        {
            foreach (var i in instance.scaleRatioTransform)
                i.SetScale(new Vector3(scaleRatio, scaleRatio, scaleRatio));
        }
        //DebugMode.Log("ScreenRatio: " + screenRatio);
    }

    public static Orientation currentOrientation = Orientation.Unknown;

    IEnumerator GetOrientation()
    {
        if (currentOrientation != Orientation.Unknown)
            Debug.Log("DeviceOrientation_" + currentOrientation);

        int infiniteLoopBreak = 0;

        while (currentOrientation == Orientation.Unknown)
        {
            CheckDeviceOrientation();

            if (currentOrientation != Orientation.Unknown)
                break;

            yield return null;

            infiniteLoopBreak++;
            if (infiniteLoopBreak > 1000)
                break;
        }
    }

    public static void CheckDeviceOrientation()
    {
#if UNITY_EDITOR
        if (Screen.width < Screen.height)
        {
            if (currentOrientation != Orientation.Portrait)
            {
                ChangeOrientation(Orientation.Portrait);
            }
        }
        else
        {
            if (currentOrientation != Orientation.Landscape)
            {
                ChangeOrientation(Orientation.Landscape);
            }
        }


#else
        if (Screen.orientation == ScreenOrientation.LandscapeLeft ||
           Screen.orientation == ScreenOrientation.LandscapeLeft ||
           Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            if (currentOrientation != Orientation.Landscape) //Orientation changed to LANDSCAPE
            {
                ChangeOrientation(Orientation.Landscape);
            }
        }
        else if (Screen.orientation == ScreenOrientation.Portrait ||
                 Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            if (currentOrientation != Orientation.Portrait) //Orientation changed to PORTRAIT
            {
                ChangeOrientation(Orientation.Portrait);
            }
        }
        else //FALLBACK option if we are in AutoRotate or if we are in Unknown
        {
            ChangeOrientation(Orientation.Landscape);
        }
#endif
    }

    public static void ChangeOrientation(Orientation value)
    {
        Debug.Log("OnOrientationChanged: " + currentOrientation + " " + value);
        if (currentOrientation != value)
        {
            currentOrientation = value;
            OnOrientationChanged?.Invoke(currentOrientation);
        }
    }

    public static void DoStartCoroutine(IEnumerator ienumerator)
    {
        if (instance)
            instance.StartCoroutine(ienumerator);
    }

    public static void DoStopCoroutine(IEnumerator ienumerator)
    {
        if (instance)
            instance.StopCoroutine(ienumerator);
    }

    private Rect uiRect;

    private void LateUpdate()
    {
        if (uiScreenRect == null || uiScreenRect.size != uiRect.size || uiScreenRect.position != uiRect.position)
        {
            UpdateUIScreenRect();
            CheckDeviceOrientation();
            OnSizeChanged?.Invoke();
        }
    }

    public void SupportLandScape(UIToggle toggle)
    {
        Screen.autorotateToLandscapeLeft = toggle.isOn;
        Screen.autorotateToLandscapeRight = toggle.isOn;

        CheckDeviceOrientation();

        UIToast.ShowNotice("Support LandScape: " + Screen.orientation);
    }

    public enum Orientation { Landscape, Portrait, Unknown }
    #endregion

    public void OpenUrl(string url)
    {
        try
        {
            Application.OpenURL(url);
        }
        catch (Exception ex)
        {
            Debug.LogError("Unable to open URL: " + ex.Message);
        }
    }

    private static string escapeURL(string url)
    {
        return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    }

    #region SCREENSHOT
    public static Texture2D screenshotTexture { get; set; }
    private static string screenshotPath => Application.persistentDataPath + "/" + screenshotFileName;

    public void TakeScreenshotAndShare()
    {
        TakeScreenShot(0f, true, (onDone) =>
        {
            ShareScreenshotInGame("Screenshot");
        });
    }

    public static void TakeScreenShot(float delayTime = 1.25f, bool save = false, Action<bool> onDone = null, bool destroyOnDone = false)
    {
        instance.StartCoroutine(DOTakeScreenShot(delayTime, save, onDone, destroyOnDone));
    }

    public static IEnumerator DOTakeScreenShot(float delayTime = 0f, bool save = false, Action<bool> onDone = null, bool destroyOnDone = false)
    {
        yield return new WaitForSeconds(delayTime);
        // We should only read the screen buffer after rendering is complete
        yield return new WaitForEndOfFrame();
        screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture();
        if (save)
            instance.StartCoroutine(DOSaveScreenShot(onDone, destroyOnDone));
    }

    public static void SaveScreenShot(Action<bool> onDone = null, bool destroyOnDone = false)
    {
        instance.StartCoroutine(DOSaveScreenShot(onDone, destroyOnDone));
    }

    public static IEnumerator DOSaveScreenShot(Action<bool> onDone = null, bool destroyOnDone = false)
    {
        if (screenshotTexture)
        {
            // We should only read the screen buffer after rendering is complete
            yield return new WaitForEndOfFrame();

            try
            {
                //// Create a texture the size of the screen, RGB24 format
                //int width = Screen.width;
                //int height = Screen.height;
                //screenshotTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                //screenshotTexture.Apply();

                // Encode texture into PNG
                byte[] bytes = screenshotTexture.EncodeToPNG();
                // Write to a file in the project folder
                File.WriteAllBytes(screenshotPath, bytes);

                DebugMode.Log("DOSaveScreenShot: " + screenshotPath);

                if (destroyOnDone)
                    Destroy(screenshotTexture);

                onDone?.Invoke(true);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                onDone?.Invoke(false);
            }
        }
    }

    public static Sprite SpriteScreenshotInGame
    {
        get
        {
            if (screenshotTexture)
            {
                Rect rect = new Rect(0, 0, screenshotTexture.width, screenshotTexture.height);
                var sprite = Sprite.Create(screenshotTexture, rect, new Vector2(0.5f, 0.5f), 100);
                return sprite;
            }
            return null;
        }
    }

    public static void GetScreenShot(Action<Sprite> sprite)
    {
        DebugMode.Log(" GetScreenShot");

        string path = "file://" + screenshotPath;
#if UNITY_EDITOR
        path = "file:///" + screenshotPath;
#endif
        instance.StartCoroutine(FileExtend.DOLoadSprite(sprite, path));
    }

    public static void DelScreenShot()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (File.Exists(screenshotPath))
                File.Delete(screenshotPath);
            screenshotTexture = null;
        });
    }

    public static void ShareScreenshotInGame(string subject, string shareDescription = "")
    {
        if (File.Exists(screenshotPath))
            Share("Share your high score", subject, shareDescription, screenshotPath);
        else
            TakeAndShareScreenshot(subject, shareDescription);
    }

    public static void TakeAndShareScreenshot(string subject, string description = "")
    {
        ScreenCapture.CaptureScreenshot(screenshotFileName);
        instance.StartCoroutine(instance.DoWaitTakeScreenshot(screenshotPath, (s) =>
        {
            if (s) Share("Screenshot", subject, description, screenshotPath);
        }));
    }

    private IEnumerator DoWaitTakeScreenshot(string fileName, Action<bool> actionOnDone = null)
    {
        var elapsedTime = 0f;
        while (!File.Exists(fileName) && elapsedTime <= 1)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForSeconds(0.1f);
        }

        if (File.Exists(screenshotPath))
        {
            actionOnDone?.Invoke(true);
        }
        else
        {
            actionOnDone?.Invoke(false);
        }
    }
    #endregion

    #region SHARE
    public static void Share(string title, string shareSubject, string shareMessage, string filePath = "", string fileType = "image/png", string shareUrl = "")
    {
        DebugMode.Log("No sharing set up for this platform.");
    }

#if UNITY_IOS
    //[System.Runtime.InteropServices.DllImport("__Internal")]
    //private static extern void showAlertMessage(ref ConfigStruct conf);
    //[System.Runtime.InteropServices.DllImport("__Internal")]
    //private static extern void showSocialSharing(ref SocialSharingStruct conf);
#endif

    public struct ConfigStruct
    {
        public string title;
        public string message;
    }

    public struct SocialSharingStruct
    {
        public string text;
        public string url;
        public string image;
        public string subject;
    }

    public static void CallSocialShareAdvanced(string subject, string message, string img = "", string url = "")
    {
#if UNITY_IOS
        if (!string.IsNullOrEmpty(img))
        {
            SocialSharingStruct conf = new SocialSharingStruct
            {
                subject = subject,
                text = message,
                url = !string.IsNullOrEmpty(url) ? url : shareUrl,
                image = img
            };
            //showSocialSharing(ref conf);
        }
        else
        {
            ConfigStruct conf = new ConfigStruct
            {
                title = subject,
                message = message
            };
            //showAlertMessage(ref conf);
        }
#endif
    }
    #endregion

    #region FEEDBACK
    public static void Feedback(string data)
    {
        string subject = escapeURL("Feedback from " + Application.productName);
        string body = escapeURL("Please enter your message here" + "\n\n\n\n\n" +
            data +
         "!!! Please DO NOT modify this: " + "\n" +
         appVerstionDetail + "\n" +
         "Device Model: " + SystemInfo.deviceModel + "\n" +
         "System Memory: " + SystemInfo.systemMemorySize + "\n" +
         "Graphics Memory: " + SystemInfo.graphicsMemorySize + "\n" +
         "Graphics Name: " + SystemInfo.graphicsDeviceName + "\n" +
         "Graphics Vendor: " + SystemInfo.graphicsDeviceVendor + "\n" +
         "Graphics Version: " + SystemInfo.graphicsDeviceVersion);

        if (!string.IsNullOrEmpty(instance.emailCC))
            Application.OpenURL("mailto:" + instance.email + "?cc=" + instance.emailCC + "&subject=" + subject + "&body=" + body);
        else
            Application.OpenURL("mailto:" + instance.email + "?subject=" + subject + "&body=" + body);
    }
    #endregion
    #region AdminTool
    [SerializeField]
    private List<CanvasGroup> canvasGroups = null;

    private void OnDisable()
    {
        DataManager.OnLoaded -= DataManager_OnLoaded;
    }

    private void OnEnable()
    {
        DataManager.OnLoaded += DataManager_OnLoaded;
    }

    private void DataManager_OnLoaded(GameData gameData)
    {
#if USE_HACK
        if (gameData != null)
        {
            var alpha = DataManager.UserData.isHideUI ? 0 : 1;
            for (int i = 0; i < canvasGroups.Count; i++)
            {
                canvasGroups[i].alpha = alpha;
            }
            DataManager.UserData.isVIP = true;
        }
#endif
    }

#endregion

}