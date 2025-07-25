using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PopupSetting : MonoBehaviour
{

    [SerializeField] UIAnimation anim;
    [SerializeField] Image fadeImg;
    [SerializeField] Button settingBtn;
    [SerializeField] Button homeBtn;
    [SerializeField] Button closeBtn;
    [SerializeField] RectTransform bgTf;
    [SerializeField] CanvasGroup bgCanvasGroup;
    [SerializeField] RectTransform homeCurrentTf;
    [SerializeField] RectTransform closeCurrentTf;

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        DOTween.Kill(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        settingBtn.onClick.RemoveAllListeners();
        settingBtn.onClick.AddListener(() =>
        {
            SoundManager.Play(SoundHelper.ButtonClick);
            Show();
        });

        homeBtn.onClick.RemoveAllListeners();
        homeBtn.onClick.AddListener(() =>
        {
            SoundManager.Play(SoundHelper.ButtonClick);
            Hide();
        });

        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            SoundManager.Play(SoundHelper.ButtonClick);
            Hide();
        });
    }

    public void Show()
    {
        fadeImg.SetAlpha(0);
        bgCanvasGroup.alpha = 0f;
        bgTf.SetScale(0);
        anim.Show(() =>
        {
            fadeImg.DOFade(1, 0.25f).OnComplete(() =>
            {
                ShowAll(bgTf);
                bgCanvasGroup.alpha = 0.25f;
                bgCanvasGroup.DOFade(1, 0.25f).SetId(gameObject);
                homeBtn.transform.position = homeCurrentTf.position;
                closeBtn.transform.position = closeCurrentTf.position;
            }).SetId(gameObject);
        });
    }

    public void Hide()
    {
        OffAll(bgTf);
        bgCanvasGroup.DOFade(0, 0.3f).OnComplete(() =>
        {
            fadeImg.DOFade(0, 0.25f).OnComplete(() =>
            {
                anim.Hide(() => { });
            }).SetId(gameObject);
        }).SetUpdate(true).SetId(gameObject);
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
