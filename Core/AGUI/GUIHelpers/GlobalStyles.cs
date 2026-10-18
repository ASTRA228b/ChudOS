using UnityEngine;

namespace ChudOS.Core.AGUI.GUIHelpers;

public static class GlobalStyles
{
    public static GUIStyle? WindowStyle, Buttonss, SliderStyle, SliderThumbStyle;
    private static Texture2D? WindowTex, ButtonTex, SliderTex, ThumbTex;
    private static readonly Color WindowColor = new(0.07f, 0.05f, 0.11f);
    private static readonly Color ButtonColor = new(0.17f, 0.10f, 0.27f);
    private static readonly Color SliderColor = new(0.13f, 0.09f, 0.20f);
    private static readonly Color ThumbColor = new(0.47f, 0.25f, 0.79f);

    public static void INIT()
    {
        if (WindowStyle != null) return;
        WindowTex = GlobalTex.MakeTex(1, 1, WindowColor);
        ButtonTex = GlobalTex.MakeTex(1, 1, ButtonColor);
        SliderTex = GlobalTex.MakeTex(1, 1, SliderColor);
        ThumbTex = GlobalTex.MakeTex(1, 1, ThumbColor);
        WindowStyle = new GUIStyle(GUI.skin.window);
        Buttonss = new GUIStyle(GUI.skin.button);
        SliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
        SliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
        ApplyBackground(WindowStyle, WindowTex);
        ApplyBackground(Buttonss, ButtonTex);
        ApplyBackground(SliderStyle, SliderTex);
        ApplyBackground(SliderThumbStyle, ThumbTex);
        WindowStyle.normal.textColor = Color.white;
        Buttonss.normal.textColor = Color.white;
        Buttonss.hover.textColor = Color.white;
        Buttonss.active.textColor = Color.white;
        Buttonss.fixedHeight = 27f;
    }

    private static void ApplyBackground(GUIStyle style, Texture2D tex)
    {
        style.normal.background = tex;
        style.hover.background = tex;
        style.active.background = tex;
        style.focused.background = tex;
        style.onNormal.background = tex;
        style.onHover.background = tex;
        style.onActive.background = tex;
        style.onFocused.background = tex;
    }

    public static void Cleanup()
    {
        foreach (Texture2D? tex in new[] { WindowTex, ButtonTex, SliderTex, ThumbTex }) if (tex != null) GameObject.Destroy(tex);
        WindowStyle = Buttonss = SliderStyle = SliderThumbStyle = null;
    }
}
