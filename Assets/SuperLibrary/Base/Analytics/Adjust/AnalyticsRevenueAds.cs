//using com.adjust.sdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticsRevenueAds
{
    public static void SendEvent(ImpressionData data)
    {
        SendEventRealtime(data);


    }

    private static void SendEventRealtime(ImpressionData data)
    {
#if USE_ADJUST
        Firebase.Analytics.Parameter[] AdParameters = {
             new Firebase.Analytics.Parameter("ad_platform", "applovin"),
             new Firebase.Analytics.Parameter("ad_source", data.NetworkName),
             new Firebase.Analytics.Parameter("ad_unit_name", data.AdUnitIdentifier),
             new Firebase.Analytics.Parameter("currency","USD"),
             new Firebase.Analytics.Parameter("value",data.Revenue),
             new Firebase.Analytics.Parameter("placement",data.Placement),
             new Firebase.Analytics.Parameter("country_code",data.CountryCode),
             new Firebase.Analytics.Parameter("ad_format",data.AdFormat),
        };

        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression_rocket", AdParameters);
#endif
    }


    public static void SendRevAOAToAdjust(double rev, string adUnit, string placement)
    {
        //AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAdMob);
        //adjustAdRevenue.setRevenue(rev, "USD");
        // optional fields
        //adjustAdRevenue.setAdRevenueNetwork("appopenads");
        //adjustAdRevenue.setAdRevenueUnit(adUnit);
        //adjustAdRevenue.setAdRevenuePlacement(placement);
        // track Adjust ad revenue
        //Adjust.trackAdRevenue(adjustAdRevenue);
    }
#if USE_MAX
    public static void SendRevAOAToAdjust(double rev)
    {
        //AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAdMob);
        //adjustAdRevenue.setRevenue(rev, "USD");
        // optional fields
        //adjustAdRevenue.setAdRevenueNetwork("appopenads");
        //adjustAdRevenue.setAdRevenueUnit(data.adUnit);
        //adjustAdRevenue.setAdRevenuePlacement(data.placement);
        // track Adjust ad revenue
        //Adjust.trackAdRevenue(adjustAdRevenue);
    }

#endif

#if USE_IRON
    public static void SendRevToAdjust(IronSourceImpressionData data)
    {

        AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceIronSource);
        adjustAdRevenue.setRevenue((double)data.revenue, "USD");
        // optional fields
        adjustAdRevenue.setAdRevenueNetwork(data.adNetwork);
        adjustAdRevenue.setAdRevenueUnit(data.adUnit);
        adjustAdRevenue.setAdRevenuePlacement(data.placement);
        // track Adjust ad revenue
        Adjust.trackAdRevenue(adjustAdRevenue);
    }
#endif

}

public class ImpressionData
{
    public string CountryCode;
    public string NetworkName;
    public string AdUnitIdentifier;
    public string Placement;
    public double Revenue;
    public string AdFormat;

}
