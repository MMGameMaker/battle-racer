using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;

public class UILoadGame : MonoBehaviour
{
    [SerializeField] Text percentLabel = null;

    [Header("Slider")]
    [SerializeField] Slider processSlider = null;

    [SerializeField] UIAnimation anim = null;

    [SerializeField] DOTweenAnimation logoGame;

    public static UILoadGame instance = null;
    public static float currentProcess;
    public static float lastProcess;
    public CanvasGroup canvasGroup;

    private void Awake()
    {
        instance = this;
    }

    private void OnDisable()
    {
        DOTween.Kill(instance.gameObject);
    }

    private void Start()
    {
        anim.OnStart.RemoveAllListeners();
        anim.OnStart.AddListener(() =>
        {
            logoGame.transform.SetScale(1);
            logoGame.DORestart();
        });
    }

    public static void Process(float start = 0, float end = 1, float process = -1, string status = "", float step = 0.05f)
    {

        if (string.IsNullOrEmpty(status))
            status = "Processing... please wait!";

        if (process == -1)
        {
            currentProcess += step;
        }
        else
        {
            //0.05 -> 0.7 -> 0.7-> 0.8 -> 1.0
            currentProcess = start + (end - start) * process;
        }

        if (currentProcess >= 0)
        {
            if (instance.processSlider)
                instance.processSlider.value = currentProcess;
        }
    }

    public static void Init(bool show, TweenCallback actionOnDone)
    {
        instance.canvasGroup.alpha = 1;
        instance.processSlider.value = 0.1f;
        currentProcess = 0f;
        if (!show)
        {
            instance.anim.Hide(actionOnDone);
        }
        else
        {
            instance.anim.Show(null, actionOnDone);
        }
    }

    public static void Hide()
    {
        instance.canvasGroup.DOFade(0, 0.2f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            instance.anim.Hide();
        }).SetId(instance.gameObject);
    }
}