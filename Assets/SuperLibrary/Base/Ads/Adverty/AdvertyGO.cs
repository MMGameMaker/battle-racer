using UnityEngine;

public class AdvertyGO : MonoBehaviour
{
    public bool destroyIfDontUse = true;

    private void Awake()
    {
        AdvertyHelper.OnHideAdInGameChanged += OnHideAdInGameChanged;
    }

    private void OnDestroy()
    {
        AdvertyHelper.OnHideAdInGameChanged -= OnHideAdInGameChanged;
    }

    protected void OnHideAdInGameChanged(bool isOn)
    {
        Destroy(gameObject);
    }

    private void OnEnable()
    {
#if USE_ADVERTY
        if (DataManager.GameConfig != null && DataManager.GameConfig.adUseInPlay == false && destroyIfDontUse)
        {
            Destroy(gameObject);
        }
#else
        Destroy(gameObject);
#endif
    }
}
