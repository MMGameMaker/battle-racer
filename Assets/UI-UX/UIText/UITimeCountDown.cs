using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UITimeCountDown : MonoBehaviour
{
    [SerializeField]
    private Text countDownStatus = null;
    [SerializeField]
    private string countDownStatusOnTime = "NOW Action!";
    public string CountDownStatusOnTime
    {
        get
        {
            var temp = PlayerPrefs.GetString("countDownStatusOnTime", "NOW Action!");
            if (!string.IsNullOrEmpty(temp) && temp != countDownStatusOnTime)
                countDownStatusOnTime = temp;
            return countDownStatusOnTime;
        }
        set
        {
            countDownStatusOnTime = value;
            PlayerPrefs.SetString("countDownStatusOnTime", countDownStatusOnTime);
            PlayerPrefs.Save();
        }
    }
    [SerializeField]
    private bool hideStatusIfNotTime = true;

    [Space(10)]
    [SerializeField]
    private Button button = null;
    [Space(10)]
    public UnityEvent EventIsNotTime = null;
    public UnityEvent EventIsTime = null;


    [SerializeField]
    private DateTime pointTime = new DateTime();
    public DateTime PointTime
    {
        get => pointTime;
        set
        {
            if (pointTime != value && pointTime == new DateTime())
            {
                pointTime = value;
                StopAllCoroutines();
                StartCoroutine(CountdownUpdate());
                PlayerPrefs.SetString(name, pointTime.ToString());
                PlayerPrefs.Save();
            }
        }
    }

    private bool isTime;
    public bool IsTime
    {
        get
        {
            if (PointTime == new DateTime())
                isTime = false;
            else
                isTime = PointTime <= DateTime.Now;
            return isTime;
        }

        set => isTime = value;
    }

    public int TimeInSeconds
    {
        set
        {
            PointTime = DateTime.Now.AddSeconds(value);
        }
    }

    public int TimeInMinutes
    {
        set
        {
            PointTime = DateTime.Now.AddMinutes(value);
        }
    }

    public int TimeInHours
    {
        set
        {
            PointTime = DateTime.Now.AddHours(value);
        }
    }

    private string status = "";

    public event Action<DateTime> OnTime = delegate
    {
        Debug.Log("NOW is the time! Do something!");
    };

    private void Awake()
    {
        button?.onClick.RemoveAllListeners();
        button?.onClick.AddListener(() =>
        {
            if (IsTime)
            {
                ResetTime();
                EventIsTime?.Invoke();
            }
            else
                EventIsNotTime?.Invoke();
        });

        if (DateTime.TryParse(PlayerPrefs.GetString(name, pointTime.ToString()), out pointTime))
        {
            Debug.Log(pointTime.ToString());
        }

        UpdateStatus();
    }

    public void ResetTime()
    {
        StopAllCoroutines();
        pointTime = new DateTime();
        UpdateStatus();
    }

    private void OnEnable()
    {
        StartCoroutine(CountdownUpdate());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator CountdownUpdate()
    {
        countDownStatus?.gameObject.SetActive(true);
        UpdateStatus();
        while (PointTime > DateTime.Now)
        {
            UpdateStatus();
            yield return new WaitForSeconds(1);
        }
    }

    private void UpdateStatus()
    {
        status = PointTime.ToCountDown();
        if (string.IsNullOrEmpty(status))
        {
            StopAllCoroutines();
            if (PointTime > new DateTime())
            {
                if (countDownStatus)
                    countDownStatus.text = countDownStatusOnTime;
                OnTime?.Invoke(PointTime);
            }
            else
            {
                if (countDownStatus)
                {
                    countDownStatus.text = "--";
                    if (hideStatusIfNotTime)
                        countDownStatus.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (countDownStatus)
                countDownStatus.text = "-" + status;
        }
    }
}
