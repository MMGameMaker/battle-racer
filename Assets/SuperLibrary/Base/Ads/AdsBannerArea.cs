using UnityEngine;

namespace Base.Ads
{
    public class AdsBannerArea : MonoBehaviour
    {
        //    [SerializeField]
        //    protected RectTransform rectTransform;
        //    protected Vector2 anchorMin = new Vector2(0, 0);
        //    protected Vector2 anchorMax = new Vector2(0, 0);

        //    private void Awake()
        //    {
        //        if (rectTransform == null)
        //            rectTransform = GetComponent<RectTransform>();
        //        anchorMin = rectTransform.anchorMin;
        //        anchorMax = rectTransform.anchorMax;
        //        AdsManager.AdsBannerAreaList.Add(this);
        //    }

        //    private void OnDestroy()
        //    {
        //        AdsManager.AdsBannerAreaList.Remove(this);
        //    }

        //    private void OnEnable()
        //    {
        //        AdsManager.UpdateBannerArea();
        //    }

        //    public void SetArea(float newAnchor, BannerPos bannerPos)
        //    {
        //        if (AdsManager.Settings.useBanner != AdMediation.NONE)
        //        {
        //            if (rectTransform == null)
        //                rectTransform = GetComponent<RectTransform>();
        //            if (rectTransform == null)
        //            {
        //                Debug.LogError(name + " AdsBannerArea rectTransform NULL");
        //                return;
        //            }

        //            if (bannerPos == BannerPos.BOTTOM)
        //            {
        //                rectTransform.anchorMin = new Vector2(anchorMin.x, anchorMin.y + newAnchor);
        //            }
        //            else if (bannerPos == BannerPos.TOP)
        //            {
        //                rectTransform.anchorMax = new Vector2(anchorMax.x - newAnchor, anchorMax.y);
        //            }
        //            else
        //            {
        //                rectTransform.anchorMin = anchorMin;
        //                rectTransform.anchorMax = anchorMax;
        //            }
        //        }
        //    }
    }
}
