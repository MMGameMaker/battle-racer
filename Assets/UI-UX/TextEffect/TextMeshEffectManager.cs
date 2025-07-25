using UnityEngine;
using DG.Tweening;
using System;

public class TextMeshEffectManager : MonoBehaviour
{

    [SerializeField]
    private TextMesh textMesh = null;
    [SerializeField]
    private float moveYOffset = 1;
    [SerializeField]
    private float timeAnimation = 0.25f;
    [SerializeField]
    private int initialPoolSize = 10;

    private void Start()
    {
        textMesh.CreatePool(initialPoolSize);
        textMesh.gameObject.SetActive(false);
    }

    public void DoAnimation(string text, Color color, Vector3 startPos)
    {
        DoAnimation(text, color, startPos, moveYOffset, timeAnimation);
    }

    public void DoAnimation(string text, Color color, Vector3 startPos, float moveYOffset = 0, float timeAnimation = 0)
    {
        var temp = textMesh.Spawn(transform, startPos);
        temp.color = color;
        temp.DoMoveY(text, startPos.y + moveYOffset, timeAnimation, temp.Recycle);
    }
}

public static class TextMeshExtend
{
    public static void DoMoveY(this TextMesh textMesh, string text, float yOffset = 1, float timeAnimation = 0.25f, Action actionOnDone = null)
    {
        textMesh.text = text;
        textMesh.transform.DOMoveY(yOffset, timeAnimation).OnComplete(() => actionOnDone?.Invoke());
    }
}
