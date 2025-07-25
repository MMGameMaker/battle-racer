using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIAnimation))]
public class UIInGame : MonoBehaviour
{
    public static UIInGame Instance;
    public UIAnimStatus Status => anim.Status;

    [SerializeField] UIAnimation anim = null;
    [SerializeField] Button pauseButton = null;
    [SerializeField] RectTransform pauseStartTf;
    [SerializeField] RectTransform pauseEndTf;

    private RectTransform originPauseTf;

    private void Awake()
    {
        Instance = this;
        if (anim == null)
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
        pauseButton?.onClick.RemoveAllListeners();
        pauseButton?.onClick.AddListener(() =>
        {
            SoundManager.Play(SoundHelper.ButtonClick);
            if (GameStateManager.CurrentState == GameState.Play)
                GameStateManager.Pause(null);
        });
        anim.OnShowCompleted.RemoveAllListeners();
        anim.OnShowCompleted.AddListener(() =>
        {
            pauseButton.transform.DOMove(pauseEndTf.position, 0.25f);
        });
    }

    public void Show()
    {
        pauseButton.transform.position = pauseStartTf.position;
        anim.Show();
    }

    public void Hide()
    {
        anim.Hide();
    }
}
