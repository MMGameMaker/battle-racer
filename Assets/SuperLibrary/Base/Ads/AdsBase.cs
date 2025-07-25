using System;
using UnityEngine;
using static Base.Ads.AdsManager;

namespace Base.Ads
{
    public abstract class AdsBase<T> : MonoBehaviour where T : AdsBase<T>
    {
        public static string TAG = "[ADS] ";
        public AdMediation mediation = AdMediation.NONE;

        [Header("Interstitial")]
        protected static string interPlacemenetName = "DefaultInterstitial";
        protected static string interItemName = "Default";
        private Action<AdEvent, AdType> onInterShowSuccess = null;
        protected AdEvent interEvent = AdEvent.Load;


        public static bool IsInitInter = false;

        protected static int interCountMax = 3;
        public static int interCountTry = 0;

        [Header("VideoRewared")]
        protected static string rewardPlacementName = "DefaultRewardedVideo";
        protected static string rewardItemName = "Default";
        protected Action<AdEvent, AdType> onRewardShowSuccess = null;
        protected AdEvent rewardEvent = AdEvent.Load;

        public static bool IsInitReward = false;

        protected static int rewardCountMax = 3;
        protected static int rewardCountTry = 0;

        #region VIDEO REWARED
        public abstract void RewardInit();

        public abstract void RewardLoad();

        public abstract void RewardOnReady(bool avaiable);

        protected virtual void RewardOnLoadFailed(object obj)
        {
            //PauseApp(false);

            onRewardShowSuccess?.Invoke(AdEvent.LoadNotAvaiable, AdType.Reward);
            onRewardShowSuccess = null;

            if (rewardCountTry < rewardCountMax)
            {
                rewardCountTry++;
                Invoke("RewardLoad", 15);
            }
        }

        protected virtual void RewardOnShowFailed(object obj)
        {
            //PauseApp(false);

            onRewardShowSuccess?.Invoke(AdEvent.ShowFailed, AdType.Reward);
            onRewardShowSuccess = null;

            RewardLoad();

            Time.timeScale = 1;
        }

        public virtual void RewardOnClose()
        {
            try
            {
                Debug.Log(TAG + "RewardOnClose: " + rewardEvent);

                //PauseApp(false);

                if (rewardEvent == AdEvent.ShowSuccess)
                {
                    onRewardShowSuccess?.Invoke(AdEvent.ShowSuccess, AdType.Reward);
                    onRewardShowSuccess = null;
                }

                SetStatus(AdType.Reward, AdEvent.Close, rewardPlacementName, rewardItemName, mediation);

                rewardEvent = AdEvent.Load;

                RewardLoad();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);

                onRewardShowSuccess?.Invoke(AdEvent.Exception, AdType.Reward);
                onRewardShowSuccess = null;
            }

            Time.timeScale = 1;
        }

        public virtual void RewardOnClick()
        {
        }

        public virtual void RewardOnShowSuscess(object obj)
        {
            try
            {
                rewardEvent = AdEvent.ShowSuccess;
                SetStatus(AdType.Reward, AdEvent.ShowSuccess, rewardPlacementName, rewardItemName, mediation);

                onRewardShowSuccess?.Invoke(AdEvent.ShowSuccess, AdType.Reward);
                onRewardShowSuccess = null;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            Debug.Log(TAG + "RewardOnShowSuscess");
            Time.timeScale = 1;
        }

        protected void RewardShow(Action<AdEvent, AdType> status)
        {
            //PauseApp(true);

            rewardEvent = AdEvent.ShowStart;
            rewardCountTry = 0;
            onRewardShowSuccess = status;
            SetStatus(AdType.Reward, AdEvent.ShowStart, rewardPlacementName, rewardItemName, mediation);
        }
        #endregion

        #region INTERSTITIAL
        public abstract void InterInit();

        public abstract void InterLoad();

        public abstract void InterOnReady();

        protected virtual void InterOnLoadFailed(object obj = null)
        {
            //PauseApp(false);

            onInterShowSuccess?.Invoke(AdEvent.LoadNotAvaiable, AdType.Inter);
            onInterShowSuccess = null;

            if (interCountTry < interCountMax)
            {
                interCountTry++;
                Invoke("InterLoad", 15);
            }
        }

        protected virtual void InterOnShowFailed(object obj = null)
        {
            //PauseApp(false);

            onInterShowSuccess?.Invoke(AdEvent.ShowFailed, AdType.Inter);
            onInterShowSuccess = null;

            InterLoad();

            Time.timeScale = 1;
        }

        public virtual void InterOnShowSuscess()
        {
            try
            {
                interEvent = AdEvent.ShowSuccess;
                SetStatus(AdType.Inter, AdEvent.ShowSuccess, interPlacemenetName, interItemName, mediation);

                onInterShowSuccess?.Invoke(AdEvent.ShowSuccess, AdType.Inter);
                onInterShowSuccess = null;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            Debug.Log(TAG + "InterOnShowSuscess");
            Time.timeScale = 1;

        }

        public virtual void InterOnClose()
        {
            try
            {
                Debug.Log(TAG + "InterOnClose " + interEvent);
                //PauseApp(false);

                if (interEvent == AdEvent.ShowSuccess)
                {
                    onInterShowSuccess?.Invoke(AdEvent.ShowSuccess, AdType.Inter);
                    onInterShowSuccess = null;
                }

                SetStatus(AdType.Inter, AdEvent.Close, interPlacemenetName, interItemName, mediation);

                interEvent = AdEvent.Load;

                InterLoad();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);

                onInterShowSuccess?.Invoke(AdEvent.Exception, AdType.Inter);
                onInterShowSuccess = null;
            }

            Time.timeScale = 1;
        }

        public virtual void InterOnClicked()
        {
        }

        protected void InterShow(Action<AdEvent, AdType> status)
        {
            //PauseApp(true);

            interEvent = AdEvent.ShowStart;
            interCountTry = 0;
            onInterShowSuccess = status;
            SetStatus(AdType.Inter, AdEvent.ShowStart, interPlacemenetName, interItemName, mediation);
        }
        #endregion
    }
}