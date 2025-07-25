#if UNITY_EDITOR && UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;
using System.Linq;

public class iOSBuildPostProcess
{

    const string TrackingDescription = "This game includes ads. To improve your experience and see ads that match your interests, allow tracking.";

    [PostProcessBuild(0)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToXcode)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            AddPListValues(pathToXcode);
            FixediOS13NotificationBug(pathToXcode);
        }
    }

    static void AddPListValues(string pathToXcode)
    {
        // Get Plist from Xcode project 
        string plistPath = pathToXcode + "/Info.plist";

        // Read in Plist 
        PlistDocument plistObj = new PlistDocument();
        plistObj.ReadFromString(File.ReadAllText(plistPath));

        // set values from the root obj
        PlistElementDict plistRoot = plistObj.root;

        // Set value in plist
        plistRoot.SetString("NSUserTrackingUsageDescription", TrackingDescription);
        plistRoot.SetString("NSCalendarsUsageDescription", "$(PRODUCT_NAME) user your calendar.");
        plistRoot.SetString("NSLocationAlwaysUsageDescription", "$(PRODUCT_NAME) user your localtion.");
        plistRoot.SetString("NSLocationWhenInUseUsageDescription", "$(PRODUCT_NAME) user your localtion.");
        plistRoot.SetBoolean("ITSAppUsesNonExemptEncryption", false);


#if USE_IRON && !USE_MAX
        //iron config
        PlistElementArray adNetworks = plistRoot.CreateArray("SKAdNetworkItems");
        PlistElementDict ironsource = adNetworks.AddDict();

        //SKAdNetwork IDs Manager
        //https://developers.is.com/ironsource-mobile/unity/managing-skadnetwork-ids/
        ironsource.SetString("SKAdNetworkIdentifier", "https://developers.is.com/ironsource-mobile/unity/managing-skadnetwork-ids");
#endif

        //save
        File.WriteAllText(plistPath, plistObj.WriteToString());

        Debug.Log("AddPListValues");
    }

    private static void FixediOS13NotificationBug(string pathToXcode)
    {
#if USE_NOTI
        string managerPath = pathToXcode + "/Libraries/com.unity.mobile.notifications/Runtime/iOS/Plugins/UnityNotificationManager.m";
        var text = File.ReadAllLines(managerPath).ToList();
        for (int i = 0; i < text.Count; i++)
        {
            if (text[i] == @"- (void)updateScheduledNotificationList")
            {
                text.RemoveRange(i, 7);
                text.Insert(i, @"- (void)updateScheduledNotificationList
                {
                    UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
                    if (@available(iOS 15, *))
                    {
                        dispatch_async(dispatch_get_main_queue(), ^{
                            [center getPendingNotificationRequestsWithCompletionHandler:^(NSArray < UNNotificationRequest *> *_Nonnull requests) {
                                self.cachedPendingNotificationRequests = requests;
                            }];
                        });
                    }
                    else
                    {
                        [center getPendingNotificationRequestsWithCompletionHandler:^(NSArray < UNNotificationRequest *> *_Nonnull requests) {
                            self.cachedPendingNotificationRequests = requests;
                        }];
                    }

                }");
                break;
            }
        }

        File.WriteAllLines(managerPath, text.ToArray());

        Debug.Log("FixediOS13NotificationBug");
#endif
    }
}
#endif