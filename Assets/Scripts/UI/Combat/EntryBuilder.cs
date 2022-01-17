using UnityEngine;
using EntryParts;

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

public static class EntryBuilder
{
    public static EntryPart[] GetSingleDamageEntry(GridBasedUnit effector, GridBasedUnit target, BaseAbility ability, float damage, bool critical)
    {
        string criticalText = critical ? " critical" : " ";

        return new EntryPart[10]
        {
            new LinkUnitEntryPart(effector.Character.Name, effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER),
            new EntryPart("used"),
            new ColorEntryPart(ability.GetName(), EntryColors.TEXT_ABILITY),
            new EntryPart("on"),
            new LinkUnitEntryPart(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER),
            new EntryPart("with"),
            new ColorEntryPart($"{effector.LinesOfSight[target].cover} cover", EntryColors.TEXT_IMPORTANT),
            new IconEntryPart($"{effector.LinesOfSight[target].cover}Cover", EntryColors.CoverColor(effector.LinesOfSight[target].cover)),
            new EntryPart(": did"),
            new ColorEntryPart($"{damage}{criticalText} damage", EntryColors.TEXT_IMPORTANT)
        };
    }

    public static EntryPart[] GetMissedEntry(GridBasedUnit effector, GridBasedUnit target, BaseAbility ability)
    {
        return new EntryPart[8]
        {
            new LinkUnitEntryPart(effector.Character.Name, effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER),
            new ColorEntryPart("missed", EntryColors.TEXT_IMPORTANT),
            new ColorEntryPart(ability.GetName(), EntryColors.TEXT_ABILITY),
            new EntryPart("on"),
            new LinkUnitEntryPart(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER),
            new EntryPart("with"),
            new ColorEntryPart($"{effector.LinesOfSight[target].cover} cover", EntryColors.TEXT_IMPORTANT),
            new IconEntryPart($"{effector.LinesOfSight[target].cover}Cover", EntryColors.CoverColor(effector.LinesOfSight[target].cover)),
        };
    }

    public static EntryPart[] GetSkipTurnEntry(GridBasedUnit effector)
    {
        return new EntryPart[3]
        {
            new LinkUnitEntryPart(effector.Character.Name, effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER),
            new ColorEntryPart("skipped", EntryColors.TEXT_IMPORTANT),
            new EntryPart("his turn"),
        };
    }
}
