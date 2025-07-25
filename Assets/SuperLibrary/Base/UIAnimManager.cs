using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIAnimManager : MonoBehaviour
{
    protected static UIAnimManager instance = null;

    public static Transform RootTransform = null;

    public static Transform SafeAreaTransform = null;

    public static List<UIAnimation> ScreenList = new List<UIAnimation>();

    public static List<UIAnimation> PopupList = new List<UIAnimation>();

    public static RectTransform RootRectTransform = null;

    private static Vector2 startAnchoredPosition2D = Vector2.zero;

    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);

        instance = this;
        if (RootTransform == null)
            RootTransform = GetComponent<Transform>();
        if (RootRectTransform == null)
            RootRectTransform = GetComponent<RectTransform>();
        SafeAreaTransform = RootTransform.GetComponentInChildren<Transform>();
        if (SafeAreaTransform == null)
            SafeAreaTransform = GameObject.Find("SafeArea")?.transform;
    }

    public static void StartDoCoroutine(IEnumerator enumerator)
    {
        instance?.StartCoroutine(enumerator);
    }
}
