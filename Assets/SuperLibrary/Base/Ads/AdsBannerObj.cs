using UnityEngine;
using UnityEngine.UI;

public class AdsBannerObj : MonoBehaviour
{
    public AspectRatioFitter aspectRatioFitter = null;
    public RectTransform rectTransform = null;
    public float scaleDelta = 1f;

    private void Awake()
    {
        if (aspectRatioFitter == null)
            aspectRatioFitter = GetComponent<AspectRatioFitter>();
        rectTransform = transform.GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (transform.parent.TryGetComponent(out RectTransform parent) && aspectRatioFitter != null)
        {
            if (parent.sizeDelta.x > parent.sizeDelta.y)
            {
                aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                scaleDelta = parent.sizeDelta.x / 480;
            }
            else
            {
                aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                scaleDelta = parent.sizeDelta.x / 320;
            }
        }
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
