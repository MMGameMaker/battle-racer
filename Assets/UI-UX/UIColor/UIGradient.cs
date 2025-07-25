using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//Only work with Image and Text
public class UIGradient : BaseMeshEffect
{
    [SerializeField]
    private Type gradientType = Type.Horizontal;
    [SerializeField]
    private Blend blendMode = Blend.Override;
    [Range(-1, 1)]
    [SerializeField]
    private float _offset = 0f;
    [SerializeField]
    private Gradient effectGradient = new Gradient() { colorKeys = new GradientColorKey[] { new GradientColorKey(new Color32(165, 0, 75, 255), 0), new GradientColorKey(new Color32(200, 25, 25, 255), 1) } };

    #region Properties
    public Blend BlendMode
    {
        get { return blendMode; }
        set { blendMode = value; }
    }

    public Gradient EffectGradient
    {
        get { return effectGradient; }
        set { effectGradient = value; }
    }

    public Type GradientType
    {
        get { return gradientType; }
        set { gradientType = value; }
    }

    public float Offset
    {
        get { return _offset; }
        set { _offset = value; }
    }
    #endregion

    public override void ModifyMesh(VertexHelper helper)
    {
        if (!IsActive() || helper.currentVertCount == 0)
            return;

        List<UIVertex> vertexList = new List<UIVertex>();

        helper.GetUIVertexStream(vertexList);

        int nCount = vertexList.Count;
        switch (GradientType)
        {
            case Type.Horizontal:
                {
                    float left = vertexList[0].position.x;
                    float right = vertexList[0].position.x;
                    float x = 0f;

                    for (int i = nCount - 1; i >= 1; --i)
                    {
                        x = vertexList[i].position.x;

                        if (x > right) right = x;
                        else if (x < left) left = x;
                    }

                    float width = 1f / (right - left);
                    UIVertex vertex = new UIVertex();

                    for (int i = 0; i < helper.currentVertCount; i++)
                    {
                        helper.PopulateUIVertex(ref vertex, i);

                        vertex.color = BlendColor(vertex.color, EffectGradient.Evaluate((vertex.position.x - left) * width - Offset));

                        helper.SetUIVertex(vertex, i);
                    }
                }
                break;

            case Type.Vertical:
                {
                    float bottom = vertexList[0].position.y;
                    float top = vertexList[0].position.y;
                    float y = 0f;

                    for (int i = nCount - 1; i >= 1; --i)
                    {
                        y = vertexList[i].position.y;

                        if (y > top) top = y;
                        else if (y < bottom) bottom = y;
                    }

                    float height = 1f / (top - bottom);
                    UIVertex vertex = new UIVertex();

                    for (int i = 0; i < helper.currentVertCount; i++)
                    {
                        helper.PopulateUIVertex(ref vertex, i);

                        vertex.color = BlendColor(vertex.color, EffectGradient.Evaluate((vertex.position.y - bottom) * height - Offset));

                        helper.SetUIVertex(vertex, i);
                    }
                }
                break;
        }
    }

    public Color BlendColor(Color colorA, Color colorB)
    {
        switch (BlendMode)
        {
            default: return colorB;
            case Blend.Add: return colorA + colorB;
            case Blend.Multiply: return colorA * colorB;
        }
    }

    public Color colorFirst
    {
        get
        {
            if (effectGradient?.colorKeys != null)
                return effectGradient.colorKeys.FirstOrDefault().color;
            return Color.white;
        }
    }

    public Color colorLast
    {
        get
        {
            if (effectGradient?.colorKeys != null)
                return effectGradient.colorKeys.LastOrDefault().color;
            return Color.white;
        }
    }

    public Gradient Set(Color first, Color last)
    {
        return new Gradient() { colorKeys = new GradientColorKey[] { new GradientColorKey(first, 0), new GradientColorKey(last, 1) } };
    }

    public enum Type
    {
        Horizontal,
        Vertical
    }

    public enum Blend
    {
        Override,
        Add,
        Multiply
    }
}