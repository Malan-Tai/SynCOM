using UnityEngine;

public static class EntryColors
{
    public static readonly Color ICON_DUO_ABILITY = Color.cyan;
    public static readonly Color TEXT_ABILITY = Color.cyan;
    public static readonly Color TEXT_IMPORTANT = Color.yellow;
    public static readonly Color LINK_UNIT = Color.red;
    public static readonly Color LINK_UNIT_HOVER = Color.Lerp(Color.red, Color.black, 0.2f);

    public static Color CoverColor(EnumCover cover)
    {
        switch (cover)
        {
            case EnumCover.None:
                return Color.red;
            case EnumCover.Half:
                return Color.yellow;
            case EnumCover.Full:
                return Color.green;
        }

        return Color.white;
    }
}