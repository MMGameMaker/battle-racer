using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ColorUIExtend
{
    #region Image
    public static Color GetColor(this Graphic obj)
    {
        return obj.color;
    }

    public static Color SetColor(this Graphic obj, Color color)
    {
        return obj.color = color;
    }

    public static Color SetAlpha(this Graphic obj, float alpha)
    {
        var color = obj.color;
        color.a = alpha;
        obj.color = color;
        return color;
    }
    #endregion

    #region TextMesh
    public static Color GetColor(this TextMesh obj)
    {
        return obj.color;
    }

    public static Color SetColor(this TextMesh obj, Color color)
    {
        return obj.color = color;
    }

    public static Color SetAlpha(this TextMesh obj, float alpha)
    {
        var color = obj.color;
        color.a = alpha;
        obj.color = color;
        return color;
    }
    #endregion
}
