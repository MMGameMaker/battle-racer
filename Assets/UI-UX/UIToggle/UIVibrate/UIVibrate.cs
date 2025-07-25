using UnityEngine;

public class UIVibrate : UIToggle
{
    public override void Start()
    {
        base.Start();
        toggle.onValueChanged.AddListener(ToggleVibrate);
    }

    private void ToggleVibrate(bool isOn)
    {
        if (isOn)
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE
            
            Handheld.Vibrate();
#endif
        }
    }

    public static void Vibrate()
    {
        if (instance.isOn)
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE

            Handheld.Vibrate();
#endif
        }
    }
}
