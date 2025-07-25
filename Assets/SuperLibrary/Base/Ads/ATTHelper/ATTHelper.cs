using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
#if UNITY_IOS
/*
1. In the Unity Editor, select Window > Package Manager to open the Package Manager.
2. Select the iOS 14 Advertising Support package from the list, then select the most recent verified version.
3. Click the Install or Update button.
*/

using Unity.Advertisement.IosSupport;
#endif

#if USE_FACEBOOK && UNITY_IOS && !UNITY_EDITOR
public static class AudienceNetworkSettings
{
    [DllImport("__Internal")]
    private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

    public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
    {
        FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);

        Debug.Log("FBAdSettingsBridgeSetAdvertiserTrackingEnabled: " + advertiserTrackingEnabled);
    }
}
#endif

public class ATTHelper : MonoBehaviour
{
    public static IEnumerator DOCheckATT()
    {
#if UNITY_IOS
        AppsFlyerSDK.AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);

        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
        }
#if !UNITY_EDITOR
        while (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            yield return null;
#endif

#if USE_FACEBOOK && UNITY_IOS && !UNITY_EDITOR
        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED)
            AudienceNetworkSettings.SetAdvertiserTrackingEnabled(true);
        else
            AudienceNetworkSettings.SetAdvertiserTrackingEnabled(false);
#endif
#endif
        yield return null;
    }
}
