using Base.Ads;
using DG.Tweening;
using MyBox;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Spine.Unity;
using Spine;
using Base;

public class UIGameOver : MonoBehaviour
{
    [Header("Base")]
    [SerializeField]
    protected UIAnimation anim = null;
    public UIAnimStatus Status = UIAnimStatus.IsHide;

    [Header("Result")]
    [SerializeField]
    protected UIAnimation animLose = null;
    [SerializeField]
    protected UIAnimation animWin = null;
    [SerializeField]
    protected UIAnimation animContinue = null;

    public float timeDelayBtnBack = 0.5f;

    public static UIGameOver instance;

    [SerializeField] Image fadeImg;

    [Header("Win")]
    [SerializeField] Button homeBtn;
    [SerializeField] Button adsBtn;
    [SerializeField] Button nextBtn;
    [SerializeField] Button replayBtn;
    [SerializeField] RectTransform groundWinTf;
    [SerializeField] RectTransform rewardSpinTf;
    [SerializeField] SkeletonGraphic victorySkeletonGraphic;
    [SerializeField] SkeletonGraphic victoryBoardSkeletonGraphic;
    [SerializeField] RectTransform startVictoryBoardAnimTf;
    [SerializeField] RectTransform endVictoryBoardAnimTf;
    [SerializeField] RectTransform rewardSpinStartTf;
    [SerializeField] RectTransform rewardSpinEndTf;

    [SerializeField] RectTransform arrowSpinTf;
    [SerializeField] RectTransform arrowSpinStartTf;
    [SerializeField] RectTransform arrowSpinEndTf;

    [SerializeField] RectTransform[] topRewardTfs;

    [SerializeField] Text adsRewardTxt;

    private string victoryUp = "victory_up";
    private string victoryLoop = "victory_loop";

    private string woodenDown = "wooden_board_down";
    private string woodenLoop = "wooden_board_loop";

    private bool isVictory = false;

    [Header("Continue")]
    [SerializeField] CanvasGroup continuecanvasGroup;
    [SerializeField] Button closeBtn;
    [SerializeField] Button acceptBtn;
    [SerializeField] RectTransform boardTf;
    [SerializeField] Image adsContinueImg;

    protected void Awake()
    {
        if (anim == null)
            anim = GetComponent<UIAnimation>();
        instance = this;
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        DOTween.Kill(gameObject);
    }

    protected void Start()
    {
        anim.OnStart.RemoveAllListeners();
        anim.OnStart.AddListener(() =>
        {
            SetUpProperty();
        });

        animWin.OnShowCompleted.RemoveAllListeners();
        animWin.OnShowCompleted.AddListener(() =>
        {
            ShowVictory();
        });

        homeBtn.onClick.RemoveAllListeners();
        homeBtn.onClick.AddListener(() =>
        {
            InteractableAllButton();
            if (isVictory)
            {
                arrowSpinTf.DOKill();
                HideVictory(0);
            }
            else
            {
                HideLose(0);
                animLose.Hide(() => { anim.Hide(); });
            }
        });

        adsBtn.onClick.RemoveAllListeners();
        adsBtn.onClick.AddListener(() =>
        {
            InteractableAllButton();
            arrowSpinTf.DOKill();
            HideVictory(1);
        });
        nextBtn.onClick.RemoveAllListeners();
        nextBtn.onClick.AddListener(() =>
        {

            InteractableAllButton();
            arrowSpinTf.DOKill();
            HideVictory(1);
        });
        replayBtn.onClick.RemoveAllListeners();
        replayBtn.onClick.AddListener(() =>
        {
            InteractableAllButton();
            if (isVictory)
            {
                arrowSpinTf.DOKill();
                HideVictory(2);
            }
            else
            {
                HideLose(2);
                animLose.Hide(() => { anim.Hide(); });
            }
        });

        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            HideReborn(1);
        });

        acceptBtn.onClick.RemoveAllListeners();
        acceptBtn.onClick.AddListener(() =>
        {
            HideReborn(0);
        });
    }

    private void LateUpdate()
    {
        var value = arrowSpinTf.localPosition.x;
        if (value < topRewardTfs[0].localPosition.x || value > topRewardTfs[1].localPosition.x)
        {
            adsRewardTxt.text = $"{2 * GameStatisticsManager.goldEarn}";
        }
        else if (value < topRewardTfs[2].localPosition.x || value > topRewardTfs[3].localPosition.x)
        {
            adsRewardTxt.text = $"{3 * GameStatisticsManager.goldEarn}";
        }
        else
        {
            adsRewardTxt.text = $"{5 * GameStatisticsManager.goldEarn}";
        }

    }

    public void Show(GameState gameState, object data)
    {
#if USE_IRON || USE_MAX || USE_ADMOB
        //Base.Ads.AdsManager.TotalTimePlay += GameStatisticsManager.TimePlayInGameEnd;
#endif
        if (gameState == GameState.GameOver)
        {
            animLose.gameObject.SetActive(true);
            ShowResult(false);
        }
        else if (gameState == GameState.Complete)
        {
            animWin.gameObject.SetActive(true);
            ShowResult(true);
        }
        else if (gameState == GameState.RebornContinue)
        {
            animContinue.gameObject.SetActive(true);
            ShowContinue();
        }
    }

    public void Hide(Action onHideDone = null)
    {
        Status = UIAnimStatus.IsAnimationHide;
        animLose.Hide();
        animWin.Hide();

        anim.Hide(() =>
        {
            onHideDone?.Invoke();
            Status = UIAnimStatus.IsHide;
        });
    }

    public virtual void ShowResult(bool isWin)
    {
        if (isWin)
        {
            anim.Show(() =>
            {
                animWin.Show();
            });
        }
        else
        {
            ShowLose();
        }
        isVictory = isWin;
    }

    private void ShowVictory()
    {
        fadeImg.DOFade(1, 0.75f).SetId(gameObject).OnComplete(() =>
        {
            groundWinTf.DOScaleY(1.25f, 0.25f).SetEase(Ease.InQuad).OnComplete(() =>
            {
                groundWinTf.DOScaleY(1, 0.15f).SetEase(Ease.OutQuad).SetDelay(0.05f).OnComplete(() =>
                {
                    victorySkeletonGraphic.AnimationState.TimeScale += 2;
                    victorySkeletonGraphic.transform.localScale = Vector3.one;
                    victorySkeletonGraphic.AnimationState.SetAnimation(0, victoryUp, false).Complete += VictoryAnimLoop;
                }).SetId(gameObject);
            }).SetId(gameObject);
        }).SetId(gameObject);
    }

    private void VictoryAnimLoop(TrackEntry trackEntry)
    {
        IEnumerator YieldShowResult()
        {
            victorySkeletonGraphic.AnimationState.TimeScale = 1;
            victorySkeletonGraphic.AnimationState.SetAnimation(0, victoryLoop, true);

            yield return new WaitForSeconds(0.2f);

            victoryBoardSkeletonGraphic.gameObject.SetActive(true);
            victoryBoardSkeletonGraphic.AnimationState.SetEmptyAnimation(0, 0);
            victoryBoardSkeletonGraphic.transform.DOMove(endVictoryBoardAnimTf.position, 0.5f).SetId(gameObject);
            victoryBoardSkeletonGraphic.AnimationState.SetAnimation(0, woodenDown, false);

            rewardSpinTf.DOMove(rewardSpinEndTf.position, 0.35f).SetDelay(0.15f).OnComplete(() =>
            {
                arrowSpinTf.transform.DOMove(arrowSpinEndTf.position, 0.75f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetId(gameObject);
                EnableAllButton();
                homeBtn.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetId(gameObject);
                adsBtn.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetId(gameObject);
                nextBtn.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetId(gameObject);
                replayBtn.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetId(gameObject);
            }).SetId(gameObject);

            yield return new WaitForSeconds(0.2f);

            victoryBoardSkeletonGraphic.AnimationState.SetAnimation(0, woodenLoop, true);
        }

        StartCoroutine(YieldShowResult());
    }

    private void SetUpProperty()
    {
        fadeImg.SetAlpha(0);
        groundWinTf.SetScaleY(0);
        victorySkeletonGraphic.transform.localScale = Vector3.zero;
        victoryBoardSkeletonGraphic.gameObject.SetActive(false);
        homeBtn.transform.localScale = Vector3.zero;
        adsBtn.transform.localScale = Vector3.zero;
        nextBtn.transform.localScale = Vector3.zero;
        replayBtn.transform.localScale = Vector3.zero;
        rewardSpinTf.position = rewardSpinStartTf.position;
        arrowSpinTf.position = arrowSpinStartTf.position;
        EnableAllButton(false);
        InteractableAllButton(true);
    }

    private void HideVictory(int index)
    {
        rewardSpinTf.DOMove(rewardSpinStartTf.position, 0.5f).SetId(gameObject);
        homeBtn.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.OutBack).SetId(gameObject);
        adsBtn.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.OutBack).SetId(gameObject);
        nextBtn.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.OutBack).SetId(gameObject);
        replayBtn.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.OutBack).SetId(gameObject);
        victoryBoardSkeletonGraphic.transform.DOMove(startVictoryBoardAnimTf.position, 0.5f).SetDelay(0.25f).OnComplete(() =>
        {
            victorySkeletonGraphic.transform.localScale = Vector3.zero;
            groundWinTf.DOScaleY(0, 0.25f).SetEase(Ease.InQuad).OnComplete(() =>
            {
                fadeImg.DOFade(0.5f, 0.275f).OnComplete(() =>
                {
                    Hide(() =>
                    {
                        SwitchGameState(index);
                    });
                }).SetId(gameObject);
            }).SetId(gameObject);
        }).SetId(gameObject);
    }

    private void ShowLose()
    {
        homeBtn.gameObject.SetActive(true);
        replayBtn.gameObject.SetActive(true);
        animLose.Show(() =>
        {
            homeBtn.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetId(gameObject);
            replayBtn.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetId(gameObject);
        });
    }

    private void HideLose(int index)
    {
        SwitchGameState(index);
    }

    private void SwitchGameState(int stateId)
    {
        switch (stateId)
        {
            case 0:
                //Back To Home
                UILoadGame.Init(true, () =>
                {
                    GameStateManager.Idle(null);
                });
                break;
            case 1:
                //Next Level
                GameStateManager.Play(null);
                break;
            case 2:
                //Replay
                UILoadGame.Init(true, null);
                GameStateManager.Restart(null);
                break;
        }
    }

    private void InteractableAllButton(bool isInteract = false)
    {
        homeBtn.interactable = isInteract;
        adsBtn.interactable = isInteract;
        nextBtn.interactable = isInteract;
        replayBtn.interactable = isInteract;
    }

    private void EnableAllButton(bool isEnable = true)
    {
        homeBtn.gameObject.SetActive(isEnable);
        adsBtn.gameObject.SetActive(isEnable);
        nextBtn.gameObject.SetActive(isEnable);
        replayBtn.gameObject.SetActive(isEnable);
    }

    private void ShowContinue()
    {
        anim.Show(() =>
        {
            animContinue.Show(() =>
            {
                fadeImg.SetAlpha(0);
                continuecanvasGroup.alpha = 0;
                boardTf.SetScale(0);
                acceptBtn.transform.SetScale(0);
                closeBtn.transform.SetScale(0);
                adsContinueImg.transform.SetScale(1);

                fadeImg.DOFade(1, 0.75f).SetId(gameObject).OnComplete(() =>
                {
                    continuecanvasGroup.alpha = 0.5f;
                    boardTf.SetScale(0.5f);
                    continuecanvasGroup.DOFade(1, 0.25f).OnComplete(() =>
                    {
                        ShowAll(acceptBtn.transform, 0);
                        ShowAll(closeBtn.transform, 0.5f);
                        adsContinueImg.DOKill();
                        adsContinueImg.transform.DOScale(Vector3.one * 1.025f, 0.75f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetDelay(0.2f).SetId(gameObject);
                    }).SetId(gameObject);
                    boardTf.DOScale(1, 0.25f).SetId(gameObject);
                });
            });
        });
    }

    public void HideReborn(int index)
    {
        OffAll(acceptBtn.transform, 0);
        OffAll(closeBtn.transform, 0);
        boardTf.DOScale(0.5f, 0.25f).SetDelay(0.2f).SetId(gameObject);
        continuecanvasGroup.DOFade(0, 0.25f).OnComplete(() =>
        {
            animContinue.Hide(() =>
            {
                switch (index)
                {
                    case 0:
                        anim.Hide(() =>
                        {
                            GameStateManager.Play(null);
                        });
                        break;
                    case 1:
                        GameStateManager.WaitGameOver(null);
                        break;
                }
            });
        }).SetDelay(0.2f).SetId(gameObject);
    }

    public void ShowAll(Transform tf, float delayTime)
    {
        tf.DOKill();
        tf.SetScale(0);
        tf.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetDelay(delayTime).SetId(gameObject);
    }

    public void OffAll(Transform tf, float delayTime)
    {
        tf.DOKill(true);
        tf.DOScale(Vector3.zero, 0.2f).SetDelay(delayTime).SetId(gameObject);
    }
}
