using UnityEngine;

public static class EntryColors
{
    public static readonly Color ICON_DUO_ABILITY = Color.cyan;
    public static readonly Color TEXT_ABILITY = Color.cyan;
    public static readonly Color TEXT_IMPORTANT = Color.yellow;
    public static readonly Color LINK_UNIT = new Color(1f, 180f / 255f, 126f / 255f);
    public static readonly Color LINK_UNIT_HOVER = Color.Lerp(LINK_UNIT, Color.black, 0.2f);
    public static readonly Color TEXT_POSITIVE_EMOTION = Color.green;
    public static readonly Color TEXT_NEGATIVE_EMOTION = Color.red;
    public static readonly Color TEXT_NEUTRAL_EMOTION = Color.gray;
}