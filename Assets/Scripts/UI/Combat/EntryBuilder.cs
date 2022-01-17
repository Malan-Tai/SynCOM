using UnityEngine;
using EntryParts;

public static class EntryBuilder
{
    public static readonly Color ICON_DUO_ABILITY_COLOR = Color.cyan;
    public static readonly Color TEXT_ABILITY_COLOR = Color.cyan;
    public static readonly Color TEXT_IMPORTANT = Color.yellow;
    public static readonly Color LINK_UNIT_COLOR = Color.red;
    public static readonly Color LINK_UNIT_HOVER_COLOR = Color.Lerp(Color.red, Color.black, 0.2f);

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

    public static EntryPart[] GetSingleDamageEntry(GridBasedUnit effector, GridBasedUnit target, BaseAbility ability, float damage, bool critical)
    {
        string criticalText = critical ? " critical" : " ";

        return new EntryPart[10]
        {
            new LinkUnitEntryPart(effector.Character.Name, effector, LINK_UNIT_COLOR, LINK_UNIT_HOVER_COLOR),
            new EntryPart("used"),
            new ColorEntryPart(ability.GetName(), TEXT_ABILITY_COLOR),
            new EntryPart("on"),
            new LinkUnitEntryPart(target.Character.Name, target, LINK_UNIT_COLOR, LINK_UNIT_HOVER_COLOR),
            new EntryPart("with"),
            new ColorEntryPart($"{effector.LinesOfSight[target].cover} cover", TEXT_IMPORTANT),
            new IconEntryPart($"{effector.LinesOfSight[target].cover}Cover", CoverColor(effector.LinesOfSight[target].cover)),
            new EntryPart(": did"),
            new ColorEntryPart($"{damage}{criticalText} damage", TEXT_IMPORTANT)
        };
    }

    public static EntryPart[] GetMissedEntry(GridBasedUnit effector, GridBasedUnit target, BaseAbility ability)
    {
        return new EntryPart[8]
        {
            new LinkUnitEntryPart(effector.Character.Name, effector, LINK_UNIT_COLOR, LINK_UNIT_HOVER_COLOR),
            new ColorEntryPart("missed", TEXT_IMPORTANT),
            new ColorEntryPart(ability.GetName(), TEXT_ABILITY_COLOR),
            new EntryPart("on"),
            new LinkUnitEntryPart(target.Character.Name, target, LINK_UNIT_COLOR, LINK_UNIT_HOVER_COLOR),
            new EntryPart("with"),
            new ColorEntryPart($"{effector.LinesOfSight[target].cover} cover", TEXT_IMPORTANT),
            new IconEntryPart($"{effector.LinesOfSight[target].cover}Cover", CoverColor(effector.LinesOfSight[target].cover)),
        };
    }
}
