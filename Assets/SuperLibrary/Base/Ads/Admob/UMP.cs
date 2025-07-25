using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Base;
using Base.Ads;
using Cysharp.Threading.Tasks;
#if USE_ADOPEN || USE_ADCOLLAP
using GoogleMobileAds.Ump.Api;
#endif

public class UMP : MonoBehaviour
{
#if USE_ADOPEN || USE_ADCOLLAP
    protected static string TAG = "UMP ";
    /// <summary>
    /// If true, it is safe to call MobileAds.Initialize() and load Ads.
    /// </summary>
    public static bool CanRequestAds => ConsentInformation.CanRequestAds();
    public static bool HasUnknownError = false;
    [Header("DEBUG")]
    [SerializeField]
    private DebugGeography debugGeography = DebugGeography.Disabled;
    [SerializeField, Tooltip("https://developers.google.com/admob/unity/test-ads")]
    private List<string> testDeviceIds;
    [SerializeField]
    private Button buttonReset;

    protected static UMP instance = null;

    private void Awake()
    {
        instance = this;
        if (buttonReset)
        {
            buttonReset.onClick.AddListener(ResetConsentInformation);
        }
    }

    protected static Action OnCanRequestAd;
    protected static bool isChecking = false;
    /// <summary>
    /// Startup method for the Google User Messaging Platform (UMP) SDK
    /// which will run all startup logic including loading any required
    /// updates and displaying any required forms.
    /// </summary>
    public static IEnumerator DOGatherConsent(Action _onCanRequestAd)
    {
        if (instance == null)
            throw new Exception(TAG + "AdmobConsentController NULL");

        if (isChecking)
        {
            Debug.LogError(TAG + ConsentInformation.ConsentStatus.ToString().ToUpper() + " CHECKING");
            yield break;
        }

        isChecking = true;

        if (_onCanRequestAd == null)
        {
            ResetConsentInformation();
            FirebaseManager.LogEvent("ump_check_again");
        }
        else
        {
            OnCanRequestAd = _onCanRequestAd;
            FirebaseManager.LogEvent("ump_check");
        }

        var requestParameters = new ConsentRequestParameters();
        if (instance.debugGeography != DebugGeography.Disabled)
        {
            requestParameters = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,  // False means users are not under age.
                ConsentDebugSettings = new ConsentDebugSettings
                {
                    DebugGeography = instance.debugGeography, // For debugging consent settings by geography.                
                    TestDeviceHashedIds = instance.testDeviceIds, // https://developers.google.com/admob/unity/test-ads
                }
            };
        }

        // The Google Mobile Ads SDK provides the User Messaging Platform (Google's
        // IAB Certified consent management platform) as one solution to capture
        // consent for users in GDPR impacted countries. This is an example and
        // you can choose another consent management platform to capture consent.
        Debug.Log(TAG + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> UPDATE");

        ConsentInformation.Update(requestParameters, (FormError error) =>
        {
            if (error != null)
            {
                Debug.LogError(TAG + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> " + error.Message);
                FirebaseManager.LogEvent("ump_update_error_" + ConsentInformation.ConsentStatus.ToString(), new Dictionary<string, object> { { "errordescription", error.Message } });
                FirebaseManager.LogEvent("ump_can_request_" + ConsentInformation.CanRequestAds().ToString());
                isChecking = false;
                HasUnknownError = true;
                return;
            }

            if (CanRequestAds) // Determine the consent-related action to take based on the ConsentStatus.
            {
                // Consent has already been gathered or not required.
                // Return control back to the user.
                Debug.Log(TAG + "Update " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " -- Consent has already been gathered or not required");
                FirebaseManager.LogEvent("ump_update_success_" + ConsentInformation.ConsentStatus.ToString());
                FirebaseManager.LogEvent("ump_can_request_" + ConsentInformation.CanRequestAds().ToString());
                isChecking = false;

                if (OnCanRequestAd != null)
                {
                    var t = ActionAfterBackToMain(OnCanRequestAd);
                }
                return;
            }

            // Consent not obtained and is required.
            // Load the initial consent request form for the user.
            Debug.Log(TAG + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> LOAD AND SHOW ConsentForm If Required");
            FirebaseManager.LogEvent("ump_loadshow");
            ConsentForm.LoadAndShowConsentFormIfRequired((FormError error) =>
            {
                if (error != null) // Form load failed.
                {
                    Debug.LogError(TAG + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> " + error.Message);
                    FirebaseManager.LogEvent("ump_loadshow_error", new Dictionary<string, object> { { "errordescription", error.Message } });
                    HasUnknownError = true;
                }
                else  // Form showing succeeded.
                {
                    Debug.Log(TAG + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> LOAD AND SHOW SUCCESS");
                    FirebaseManager.LogEvent("ump_loadshow_success");
                }
                isChecking = false;
            });
        });

        Debug.Log(TAG + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> WAIT!");
        while (isChecking && (ConsentInformation.ConsentStatus == ConsentStatus.Required || ConsentInformation.ConsentStatus == ConsentStatus.Unknown))
            yield return null;

        FirebaseManager.LogEvent("ump_status_" + ConsentInformation.ConsentStatus.ToString());
        FirebaseManager.LogEvent("ump_can_request_" + ConsentInformation.CanRequestAds().ToString());
        Debug.Log(TAG + ConsentInformation.ConsentStatus.ToString().ToUpper());

        if (CanRequestAds && OnCanRequestAd != null)
        {
            OnCanRequestAd?.Invoke();
        }
    }

    /// <summary>
    /// Shows the privacy options form to the user.
    /// </summary>
    /// <remarks>
    /// Your app needs to allow the user to change their consent status at any time.
    /// Load another form and store it to allow the user to change their consent status
    /// </remarks>
    public static void ShowPrivacyOptionsForm(Button privacyButton, Action<string> onComplete)
    {
        Debug.Log(TAG + "Showing privacy options form...");
        FirebaseManager.LogEvent("ump_option_show");
        ConsentForm.ShowPrivacyOptionsForm((FormError error) =>
        {
            if (error != null)
            {
                Debug.LogError(TAG + "Showing privacy options form - ERROR " + error.Message);
                onComplete?.Invoke(error.Message);
                FirebaseManager.LogEvent("ump_option_show_error", new Dictionary<string, object> { { "error", error.Message } });
            }
            else  // Form showing succeeded.
            {
                if (privacyButton)
                    privacyButton.interactable = ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
                Debug.Log(TAG + "Showing privacy options form - SUCCESS");
                onComplete?.Invoke(null);
                FirebaseManager.LogEvent("ump_option_show_success");
            }
        });
    }

    /// <summary>
    /// Reset ConsentInformation for the user.
    /// </summary>
    public static void ResetConsentInformation()
    {
        UIToast.ShowNotice("Ooop...! \"We\" want asks for your consent to improve the best experience!");
        FirebaseManager.LogEvent("ump_reset");
        ConsentInformation.Reset();
    }
#else
    protected static string TAG = "UMP ";
    public static bool CanRequestAds => false;
    public static IEnumerator DOGatherConsent(Action _onCanRequestAd)
    {
        Debug.LogWarning(TAG + "Set Symbol USE_ADMOB in Player Settings");
        yield return null;
    }

    public static void ShowPrivacyOptionsForm(Button privacyButton, Action<string> onComplete)
    {
        Debug.LogWarning(TAG + "Set Symbol USE_ADMOB in Player Settings");
    }

    public void ResetConsentInformation()
    {
        Debug.LogWarning(TAG + "Set Symbol USE_ADMOB in Player Settings");
    }
#endif
    public static async UniTask ActionAfterBackToMain(Action callback)
    {
        await UniTask.SwitchToMainThread();
        await UniTask.Yield();
        callback?.Invoke();
    }

}
