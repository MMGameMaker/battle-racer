using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPanelEventScript : EventTrigger
{
    public static event System.Action<Vector3> OnPointerDownHandle = delegate { };
    public static event System.Action<Vector3> OnDragHandle = delegate { };
    public static event System.Action<Vector3> OnDragEndHandle = delegate { };
    public static event System.Action<Vector3> OnDragBeginHandle = delegate { };

    private void Start()
    {

    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        OnPointerDownHandle?.Invoke(eventData.position);
        OnDragBeginHandle?.Invoke(eventData.position);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        OnDragEndHandle?.Invoke(eventData.position);
    }
    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
    }
    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        OnDragHandle?.Invoke(eventData.position);
    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        //OnDragEndHandle?.Invoke(eventData.position);
    }
    //    void Update()
    //    {

    //#if UNITY_EDITOR
    //        ProcessForEditor();
    //#elif (UNITY_ANDROID || UNITY_IOS)
    //        ProcessForMobile();
    //#endif
    //    }

    //    private void ProcessForEditor()
    //    {
    //        if ((Input.GetButtonDown("Fire1")) && !EventSystem.current.IsPointerOverGameObject())
    //        {
    //            MouseDown();
    //        }
    //    }

    //    private void ProcessForMobile()
    //    {
    //        if (Input.touchCount > 0)
    //        {
    //            Touch touch = Input.GetTouch(0);

    //            if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
    //            {
    //                MouseDown();
    //            }
    //        }
    //    }

    //    private static void MouseDown()
    //    {
    //        OnPointerDownHandle?.Invoke(Vector3.zero);
    //    }
}
