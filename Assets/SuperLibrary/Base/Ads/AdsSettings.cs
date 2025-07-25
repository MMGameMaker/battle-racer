using UnityEngine;

namespace Base.Ads
{
    [CreateAssetMenu(fileName = "AdsSettings", menuName = "DataAsset/AdsSettings")]
    public class AdsSettings : ScriptableObject
    {
        public const string resDir = "Assets/Resources";

        public const string fileName = "AdsSettingsAsset";

        public const string fileExtension = ".asset";

        public float ratioInterPerReward = 3;
        public int autoRetryMax = 3;
        public AdMediation useBanner = AdMediation.MAX;
        public BannerPos bannerPosition = BannerPos.TOP;

        [Header("IRON")]
        public string ironAndroidAppKey = string.Empty;
        public string ironIOSAppKey = string.Empty;

        [Header("MAX")]
        public string maxSdkKey = string.Empty;
        public string maxAndroidBannerUnitId = string.Empty;
        public string maxAndroidInterUnitId = string.Empty;
        public string maxAndroidRewaredUnitId = string.Empty;
        public string maxAndroidOpenUnitId = string.Empty;

        public string maxIOSBannerUnitId = string.Empty;
        public string maxIOSInterUnitId = string.Empty;
        public string maxIOSRewaredUnitId = string.Empty;
        public string maxIOSOpenUnitId = string.Empty;

        [Header("ADMOB")]
        public string adMobAndroidAppId = string.Empty;
        public string adMobIOSAppId = string.Empty;

        public string admobAndroidBannerUnitId = string.Empty;
        public string admobAndroidInterUnitId = string.Empty;
        public string admobAndroidRewaredUnitId = string.Empty;
        public string admobAndroidBannerRectangleUnitId = string.Empty;

        public string admobIOSBannerUnitId = string.Empty;
        public string admobIOSInterUnitId = string.Empty;
        public string admobIOSRewaredUnitId = string.Empty;
        public string admobIOSBannerRectangleUnitId = string.Empty;

        [Header("APP OPEN ADS")]
        public string openAndroidUnitIdTier1 = string.Empty;
        public string openAndroidUnitIdTier2 = string.Empty;
        public string openAndroidUnitIdTier3 = string.Empty;
        public string openIOSUnitId = string.Empty;

        public static AdsSettings instance = null;
        public static AdsSettings Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                instance = Resources.Load<AdsSettings>(fileName);
                return instance;
            }
        }
    }
}