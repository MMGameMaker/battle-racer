using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NotiItem", menuName = "Notification Manager/NotiItem", order = 1)]
public class NotiItem : ScriptableObject
{
    public string id;
    [Space(5)]
    [HideInInspector]
    public string title;
    public string[] titles;
    [Space(5)]
    [HideInInspector]
    public string description;
    public string[] descriptions;
    [Space(5)]
    public Sprite icon;
    public string iconSmall;
    public string iconLarge;
    public string data;
    [Header("DateTime to Fire")]
    public int addDay;
    public int addHour;
    public int addMinute;

    [Header("Time Trigger to Fire")]
    public int fireInMinute;
    public int fireInSecond;
    public DateTime fireTime
    {
        get
        {
            var temp = DateTime.Today.AddDays(1).AddHours(18);
            if (fireInMinute > 0 || fireInSecond > 0)
                temp = DateTime.Now.AddSeconds(fireInMinute * 60 + fireInSecond);
            else
                temp = DateTime.Today.AddDays(addDay).AddHours(addHour).AddMinutes(addMinute);

            if (temp < DateTime.Now)
                temp.AddDays(1);
            return temp;
        }
    }

    public NotiItem(string id, string title, string description, object data)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        if (data != null)
            this.data = JsonUtility.ToJson(data);
    }
}
