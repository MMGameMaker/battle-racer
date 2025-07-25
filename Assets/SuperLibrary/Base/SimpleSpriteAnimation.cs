using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimpleSpriteAnimation : MonoBehaviour
{
    [SerializeField] SpriteRenderer sr;
    [SerializeField] List<Sprite> allSpirte = new List<Sprite>();
    [SerializeField] int fps = 24;
    [SerializeField] bool showOnStart = true;

    int frameCount = 0;
    Coroutine animationCoroutine = null;
    private void Awake()
    {
        if(sr == null)
            sr = GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        frameCount = 0;
        if (showOnStart)
            Play();
    }
    private void OnDisable()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
    }
    private IEnumerator YieldPlayAnim()
    {
        var wait = new WaitForSeconds(1f / fps);
        frameCount = 0;
        while (true)
        {
            sr.sprite = allSpirte[frameCount % allSpirte.Count];
            frameCount++;
            yield return wait;
        }
    }
    public void Play()
    {
        if(animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(YieldPlayAnim());
    }
}
