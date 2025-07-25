using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using DG.Tweening;

public class DailyCheckin : MonoBehaviour
{
    private int dayId;

    [SerializeField] Image statusImg;
    [SerializeField] Button claimedBtn;
    [SerializeField] Sprite claimed;
    [SerializeField] Sprite claim;
    [SerializeField] Sprite notclaim;
    [SerializeField] Text headerTxt;
    [SerializeField] Image rewardIcon;
    [SerializeField] Text amountTxt;

    private void OnDisable()
    {
        DOTween.Kill(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        claimedBtn.onClick.RemoveAllListeners();
        claimedBtn.onClick.AddListener(() =>
        {
            Ins_OnClaimed();
        });
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void Ins_OnClaimed()
    {
        claimedBtn.interactable = false;
        DailyCheckinDataManager.DailyGiftDataAsset.list[dayId].isDailyClaimed = true;
        DailyCheckinDataManager.dailyCheckinUserData.totalTimeClaimed++;
        DailyCheckinDataManager.dailyCheckinUserData.LastTimeCheckinUpdate = DateTime.Now;
        if (DailyCheckinDataManager.dailyCheckinUserData.totalTimeClaimed >= DailyCheckinDataManager.DailyGiftDataAsset.list.Count)
        {
            DailyCheckinDataManager.dailyCheckinUserData.totalTimeClaimed = 0;
            foreach (var day in DailyCheckinDataManager.DailyGiftDataAsset.list)
                day.isDailyClaimed = false;
        }
        statusImg.sprite = claimed;
        DailyCheckinDataManager.Save();
        PopupDailyCheckin.instance.Hide(1);
    }

    public void FillData(DailyGiftData data)
    {
        dayId = data.index;
        rewardIcon.sprite = data.iconDailyGift;
        headerTxt.text = data.name;
        amountTxt.text = data.amount.ToString();
        claimedBtn.interactable = (DailyCheckinDataManager.dailyCheckinUserData.LastTimeCheckinUpdate.Day == DateTime.Now.Day
            && DailyCheckinDataManager.dailyCheckinUserData.LastTimeCheckinUpdate.Month == DateTime.Now.Month
            && DailyCheckinDataManager.dailyCheckinUserData.LastTimeCheckinUpdate.Year == DateTime.Now.Year) ? false : (!data.isDailyClaimed
                  && DailyCheckinDataManager.dailyCheckinUserData.totalTimeClaimed == dayId);
        statusImg.sprite = claimedBtn.interactable ? claim : DailyCheckinDataManager.dailyCheckinUserData.totalTimeClaimed > dayId ? claimed : notclaim;
    }

    public void Show()
    {
        transform.SetScale(0.5f);
        transform.DOScale(1.1f, 0.15f).OnComplete(() =>
        {
            transform.DOScale(0.8f, 0.15f).OnComplete(() =>
            {
                transform.DOScale(1f, 0.15f).SetId(gameObject);
            }).SetId(gameObject);
        }).SetId(gameObject);
    }
}
