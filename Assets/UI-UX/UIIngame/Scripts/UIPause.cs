using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIAnimation))]
public class UIPause : MonoBehaviour
{
    [SerializeField]
    UIAnimation anim;

    [SerializeField] Button closeBtn;
    [SerializeField] Button homeBtn;
    [SerializeField] Button replayBtn;
    [SerializeField] RectTransform contentTf;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Animator closeAnimator;
    [SerializeField] Animator homeAnimator;
    [SerializeField] Animator replayAnimator;
    [SerializeField] Image fadeImg;

    float timeDelay = 0.4f;

    private void OnDisable()
    {
        DOTween.Kill(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            SoundManager.Play(SoundHelper.ButtonClick);
            Ins_BtnCloseClick();
        });

        homeBtn.onClick.RemoveAllListeners();
        homeBtn.onClick.AddListener(() =>
        {
            SoundManager.Play(SoundHelper.ButtonClick);
            Ins_BtnHomeClick();
        });

        replayBtn.onClick.RemoveAllListeners();
        replayBtn.onClick.AddListener(() =>
        {
            SoundManager.Play(SoundHelper.ButtonClick);
            Ins_BtnRestartClick();
        });
    }

    public void Show()
    {
        closeAnimator.enabled = false;
        homeAnimator.enabled = false;
        replayAnimator.enabled = false;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        fadeImg.SetAlpha(0);
        anim.Show(() =>
        {
            fadeImg.DOFade(1, 0.25f).SetUpdate(true).OnComplete(() =>
            {
                ShowAll(contentTf.transform);
                canvasGroup.alpha = 0.25f;
                canvasGroup.DOFade(1, 0.25f).OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    closeAnimator.enabled = true;
                    homeAnimator.enabled = true;
                    replayAnimator.enabled = true;
                }).SetUpdate(true).SetId(gameObject);
            }).SetId(gameObject);
        });
    }

    public void Hide()
    {
        anim.Hide();
    }

    private void HidePopup()
    {
        OffAll(contentTf.transform);
        canvasGroup.DOFade(0, 0.3f).OnComplete(() =>
        {
        }).SetUpdate(true).SetId(gameObject);
    }

    public void Ins_BtnHomeClick()
    {
        if (GameStateManager.CurrentState == GameState.Pause)
        {
            Time.timeScale = 1;
            HidePopup();
            DOVirtual.DelayedCall(timeDelay, () =>
            {
                MusicManager.UnPause();
                Hide();
                UILoadGame.Init(true, null);
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    GameStateManager.Idle(null);
                }).SetId(gameObject);
            }).SetUpdate(true).SetId(gameObject);
        }
    }
    public void Ins_BtnRestartClick()
    {
        if (GameStateManager.CurrentState == GameState.Pause)
        {
            HidePopup();
            DOVirtual.DelayedCall(timeDelay, () =>
            {
                Time.timeScale = 1;
                MusicManager.UnPause();
                Hide();
                UILoadGame.Init(true, null);
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    GameStateManager.Restart(null);
                }).SetId(gameObject);
            }).SetUpdate(true).SetId(gameObject);
        }
    }

    public void Ins_BtnCloseClick()
    {
        if (GameStateManager.CurrentState == GameState.Pause)
        {
            HidePopup();
            DOVirtual.DelayedCall(timeDelay, () =>
            {
                GameStateManager.Play(null);
                MusicManager.UnPause();
            }).SetUpdate(true).SetId(gameObject);
        }
    }

    public void ShowAll(Transform tf)
    {
        tf.DOKill();
        tf.SetScale(0.75f);
        tf.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true).SetId(gameObject);
    }

    public void OffAll(Transform tf)
    {
        tf.DOKill(true);
        tf.DOScale(0.5f, 0.35f).SetEase(Ease.InOutBack).SetUpdate(true).SetId(gameObject);
    }
}
