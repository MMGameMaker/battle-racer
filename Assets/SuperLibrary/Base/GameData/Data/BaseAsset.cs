using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using System.Reflection;

/// <summary>
/// [CreateAssetMenu(fileName = "NameDatasAsset", menuName = "DataAsset/NameDatasAsset")]
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class BaseAsset<T> : ScriptableObject where T : SaveData
{
    [NonSerialized]
    private T current = null;
    public T Current
    {
        get
        {
            if (current == null || string.IsNullOrEmpty(current.name))
            {
                current = list.LastOrDefault(x => x.isSelected && x.isUnlocked);
            }

            if (current == null || string.IsNullOrEmpty(current.name))
            {
                current = list.FirstOrDefault();
                if (current != null)
                {
                    current.isUnlocked = true;
                    current.isSelected = true;
                }
            }
            return current;
        }
        set
        {
            if (current != value)
            {
                if (current != null)
                    current.isSelected = false;
                current = value;
                current.isSelected = true;
#if UNITY_EDITOR
                Debug.Log(current.GetType() + " OnChanged " + current.id);
#endif
                OnChanged?.Invoke(current, unlockedList);
            }
        }
    }

    [Header("Datas")]
    public List<T> list = new List<T>();

    public delegate void DataChangedDelegate(T current, List<T> list);
    public static event DataChangedDelegate OnChanged;
    public void SetChanged(T data)
    {
        OnChanged?.Invoke(data, unlockedList);
    }

    public List<T> unlockedList
    {
        get
        {
            return list?.Where(x => x.isUnlocked == true && x.unlockPrice >= 0).ToList();
        }
    }
    public List<T> lockList
    {
        get
        {
            return list?.Where(x => x.isUnlocked == false && x.unlockPrice >= 0).ToList();
        }
    }

    public List<SaveData> itemSaveList
    {
        get => list.Where(x => x.isUnlocked || x.unlockPay > 0)
            .Select(x => new SaveData { id = x.id, isUnlocked = x.isUnlocked, isSelected = x.isSelected, count = x.count, unlockPay = x.unlockPay }).ToList();
    }

    public void ConvertToData(List<SaveData> saveData)
    {
        foreach (var i in saveData)
        {
            var temp = list.FirstOrDefault(x => x.id == i.id);
            if (temp != null)
            {
                temp.count = i.count;
                temp.unlockPay = i.unlockPay;

                if (i.isUnlocked)
                    temp.isUnlocked = i.isUnlocked;
                if (i.isSelected)
                    temp.isSelected = i.isSelected;
            }
        }
    }

    public List<T> GetCanUnlockList(float price)
    {
        return list?.Where(x => !x.isUnlocked && x.unlockPrice <= price).ToList();
    }


    public void UnlockAll()
    {
        bool isUnlockedAll = list.Count(x => x.isUnlocked) >= list.Count();
        foreach (var i in list)
            i.isUnlocked = !isUnlockedAll;
        list.FirstOrDefault().isUnlocked = true;
    }

    /// <summary>
    /// Reset Data Asset Settings
    /// </summary>
    public virtual void ResetData()
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].count = 0;
            list[i].countHit = 0;
            list[i].isSelected = false;
            list[i].isUnlocked = false;
            list[i].unlockPay = 0;
        }
        current = null;
    }

    public void ClearLog()
    {
#if UNITY_EDITOR
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
#endif
    }
}