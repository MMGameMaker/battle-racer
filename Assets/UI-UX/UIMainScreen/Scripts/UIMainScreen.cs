using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIMainScreen : MonoBehaviour
{
    public static UIMainScreen Instance;
    public UIAnimStatus Status => anim.Status;
    public Button playLevelBtn;

    [SerializeField] GameUIManager gameUIManager;
    [SerializeField] CanvasGroup playBtnCanvas;
    [SerializeField] Button settingBtn;
    [SerializeField] RectTransform settingStartTf;
    [SerializeField] RectTransform settingEndTf;

    private UIAnimation anim;

    void Awake()
    {
        Instance = this;
        anim = GetComponent<UIAnimation>();
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        DOTween.Kill(gameObject);
    }

    private void OnDestroy()
    {
    }

    private void Start()
    {
        anim.OnShowCompleted.RemoveAllListeners();
        anim.OnShowCompleted.AddListener(() =>
        {
            ShowButtonMainScreen();
        });

    }

    public void Show(TweenCallback onStart = null, TweenCallback onCompleted = null)
    {
        anim.Show(() =>
        {
            MusicManager.UnPause();
            UILoadGame.Hide();
        });
    }

    public void Hide()
    {
        anim.Hide();
    }

    public void Ins_BtnPlayClick()
    {
        if (GameStateManager.CurrentState == GameState.Idle)
        {
            GameStateManager.LoadGame(null);
        }
    }

    private void ShowButtonMainScreen()
    {
        playLevelBtn.transform.SetScale(0.5f);
        playBtnCanvas.alpha = 0.75f;
        playBtnCanvas.DOFade(1, 0.125f).SetEase(Ease.InOutSine).SetDelay(0.2f).SetId(gameObject);
        playLevelBtn.transform.DOScale(1, 0.2f).SetEase(Ease.InOutSine).SetDelay(0.2f).SetId(gameObject);
        settingBtn.transform.position = settingStartTf.position;
        settingBtn.transform.DOMove(settingEndTf.position, 0.2f).SetEase(Ease.InOutSine).SetDelay(0.2f).SetId(gameObject);
    }
}
