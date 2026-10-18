using UnityEngine;
namespace ChudOS.Core.AGUI.GUIHelpers;
[System.Serializable]
internal class GUISettingsData
{
    public Color WindowColor = new(0.07f, 0.05f, 0.11f, 1f);
    public Color ButtonColor = new(0.17f, 0.10f, 0.27f, 1f);
    public Color SliderTrackColor = new(0.13f, 0.09f, 0.20f, 1f);
    public Color SliderThumbColor = new(0.47f, 0.25f, 0.79f, 1f);
}
