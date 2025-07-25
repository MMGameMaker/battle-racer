using DG.Tweening;
using MyBox;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PopupLuckySpin : MonoBehaviour
{
    public static PopupLuckySpin instance;

    [SerializeField] UIAnimation anim;
    [SerializeField] LuckySpinContent contentPrefab;
    [SerializeField] RectTransform contentTf;
    [SerializeField] LuckySpinAsset luckySpinAsset;
    [SerializeField] RectTransform spinTf;
    [SerializeField] Image bgImage;
    [SerializeField] Image sandImage;
    [SerializeField, ReadOnly] float[] probabilities = new float[] { 0.01f, 0.23f, 0.1f, 0.15f, 0.1f, 0.01f, 0.1f, 0.3f };
    [SerializeField, ReadOnly] float timeSpin = 10f;
    [SerializeField] GameObject ledGroup1;
    [SerializeField] GameObject ledGroup2;
    [SerializeField] RectTransform headerTf;
    [SerializeField] Button spinBtn;
    [SerializeField] Button closeBtn;
    [SerializeField] Button x2ClaimBtn;
    [SerializeField] Button claimBtn;
    [SerializeField] Image congratulationImage;
    [SerializeField] Image rewardImage;
    [SerializeField] Text rewardValueTxt;
    [SerializeField] CanvasGroup mainCanvas;
    [SerializeField] CanvasGroup rewardCanvas;

    private LuckySpinReward currentReward = null;
    private int targetAngle;
    private float timer;
    private bool isActiveLed;
    private bool isStartActiveLed;
    private bool isSpin;

    private void Awake()
    {
        instance = this;
        contentPrefab.CreatePool(1);
    }

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
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            closeBtn.transform.DOScale(0, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
            mainCanvas.DOFade(0, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                Hide();
            }).SetId(gameObject);
        });
        spinBtn.onClick.RemoveAllListeners();
        spinBtn.onClick.AddListener(() =>
        {
            Spin();
        });
        x2ClaimBtn.onClick.RemoveAllListeners();
        x2ClaimBtn.onClick.AddListener(() =>
        {
            ClaimReward();
        });
        claimBtn.onClick.RemoveAllListeners();
        claimBtn.onClick.AddListener(() =>
        {
            ClaimReward();
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStartActiveLed) return;
        timer += Time.deltaTime;
        if (timer > 0.25f)
        {
            timer = 0;
            isActiveLed = !isActiveLed;
            ledGroup1.SetActive(isActiveLed);
            ledGroup2.SetActive(!isActiveLed);
        }
    }

    [ButtonMethod]
    public void Show()
    {
        bgImage.SetAlpha(0);
        spinTf.SetScale(0);
        sandImage.SetAlpha(0);
        spinBtn.transform.SetScale(0);
        closeBtn.transform.SetScale(0);
        headerTf.SetScale(0);
        mainCanvas.alpha = 1;
        rewardCanvas.alpha = 0;
        isStartActiveLed = false;
        anim.Show(() =>
        {
            bgImage.DOFade(1, 0.25f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                headerTf.DOScale(1, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
                sandImage.DOFade(1, 0.5f).SetEase(Ease.InOutSine).SetId(gameObject);
                spinTf.DOScale(1, 0.75f).SetEase(Ease.OutBounce).SetId(gameObject);
                SpawnCard();
            }).SetId(gameObject);
        });
    }

    private void SpawnCard()
    {
        IEnumerator YieldSpawnCard()
        {
            yield return new WaitForSeconds(0.5f);
            currentReward = luckySpinAsset.luckySpinRewards[0];
            for (int i = 0; i < luckySpinAsset.luckySpinRewards.Length; i++)
            {
                var card = contentPrefab.Spawn(contentTf, Vector2.zero);
                card.canvasGroup.alpha = 0;
                card.transform.eulerAngles = new Vector3(0, 0, i * (-360 / luckySpinAsset.luckySpinRewards.Length) + (360 / luckySpinAsset.luckySpinRewards.Length) / 2);
                card.FillData(luckySpinAsset.luckySpinRewards[i]);
                card.canvasGroup.DOFade(1, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
                yield return new WaitForSeconds(0.15f);
            }
            spinBtn.transform.DOScale(1, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
            closeBtn.transform.DOScale(1, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
            isStartActiveLed = true;
        }
        StartCoroutine(YieldSpawnCard());
    }

    [ButtonMethod]
    public void Hide()
    {
        anim.Hide(() =>
        {
            contentPrefab.RecycleAll();
        });
    }

    public LuckySpinReward GetRandomGift()
    {
        float random = Random.Range(0f, 1f);
        float cumulativeProbability = 0f;
        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulativeProbability += probabilities[i];
            if (random <= cumulativeProbability)
            {
                var reward = luckySpinAsset.luckySpinRewards[i];
                return reward;
            }
        }
        return null;
    }

    [ButtonMethod]
    private void Spin()
    {
        if (isSpin) return;
        isSpin = true;
        var gift = GetRandomGift();
        spinBtn.transform.DOScale(0, 0.5f).SetEase(Ease.InOutSine).SetId(gameObject);
        closeBtn.transform.DOScale(0, 0.5f).SetEase(Ease.InOutSine).SetId(gameObject);
        if (gift.index > currentReward.index)
        {
            targetAngle = (gift.index - currentReward.index) * 45;
        }
        else if (gift.index == currentReward.index)
        {
            targetAngle = (int)transform.eulerAngles.z;
        }
        else
        {
            targetAngle = (luckySpinAsset.luckySpinRewards.Length - currentReward.index + gift.index) * 45;
        }

        targetAngle = (360 - targetAngle) + 3600 / 2;

        currentReward = gift;

        contentTf.transform.DORotate(new Vector3(0, 0, -targetAngle), timeSpin, RotateMode.LocalAxisAdd).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            isSpin = false;
            ShowReward();
        });
    }

    private void ShowReward()
    {
        IEnumerator YieldShowReward()
        {
            congratulationImage.SetAlpha(0);
            rewardValueTxt.transform.SetScale(0);
            rewardImage.transform.SetScale(0);
            x2ClaimBtn.transform.SetScale(0);
            claimBtn.transform.SetScale(0);

            rewardImage.sprite = currentReward.rewardSpriteIcon;
            rewardValueTxt.text = $"{currentReward.rewardAmount}";

            yield return new WaitForSeconds(0.5f);

            sandImage.DOFade(0.25f, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
            spinTf.DOScale(0.25f, 0.5f).SetEase(Ease.InOutSine).SetId(gameObject);
            mainCanvas.DOFade(0, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                rewardCanvas.alpha = 1;
                congratulationImage.DOFade(1, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
                rewardValueTxt.transform.DOScale(1, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
                rewardImage.transform.DOScale(1, 0.25f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    claimBtn.transform.DOScale(1, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
                    x2ClaimBtn.transform.DOScale(1, 0.25f).SetEase(Ease.InOutSine).SetId(gameObject);
                }).SetId(gameObject);
            }).SetId(gameObject);
        }

        StartCoroutine(YieldShowReward());
    }

    private void ClaimReward()
    {
        rewardCanvas.DOFade(0, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            Hide();
        }).SetId(gameObject);
    }
}

