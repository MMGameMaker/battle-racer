
public enum AdType
{
    Inter = 0,
    Reward = 1,
    Banner = 2,
    AppOpen = 3,
    Rectangle = 4,
    BannerCollap = 5
}
public enum AdEvent
{
    None = -1,
    Load = 0,
    LoadAvaiable = 1,
    LoadNotAvaiable = 2,
    LoadNoInternet = 3,
    Offer = 4,
    Show = 5,
    ShowStart = 6,
    ShowFailed = 7,
    ShowSuccess = 8,
    Click = 9,
    Close = 10,
    Cancel = 11,
    ShowNoInternet = 12,
    ShowNotAvailable = 13,
    ShowNotTime = 14,
    Exception = 15,
    LoadTimeout = 16
}
public enum AdMediation
{
    NONE = 0,
    IRON = 1,
    MAX = 2,
    ADMOD = 3,
    APPOPEN = 4
}
public enum BannerPos
{
    NONE = 0,
    TOP = 1,
    BOTTOM = 2
}
