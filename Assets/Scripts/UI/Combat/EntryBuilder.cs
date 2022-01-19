using UnityEngine;
using EntryParts;
using System.Collections.Generic;

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
    public static EntryPart[] GetDamageEntry(GridBasedUnit effector, GridBasedUnit target, BaseAbility ability, float damage, bool critical)
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
            new IconEntryPart($"{effector.LinesOfSight[target].cover}Cover", EntryColors.CoverColor(effector.LinesOfSight[target].cover)),
            new ColorEntryPart($"{effector.LinesOfSight[target].cover} cover", EntryColors.TEXT_IMPORTANT),
            new EntryPart(": did"),
            new ColorEntryPart($"{damage}{criticalText} damage", EntryColors.TEXT_IMPORTANT)
        };
    }

    public static EntryPart[] GetDuoDamageEntry(
        GridBasedUnit effector, GridBasedUnit duo,
        GridBasedUnit target, BaseAbility ability,
        in ShootResult selfResults, in ShootResult allyResults)
    {
        List<EntryPart> entry = new List<EntryPart>
        {
            new LinkUnitEntryPart(effector.Character.Name, effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER),
            new EntryPart("and"),
            new LinkUnitEntryPart(duo.Character.Name, duo, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER),
            new EntryPart("used"),
            new IconEntryPart($"Duo", EntryColors.ICON_DUO_ABILITY),
            new ColorEntryPart(ability.GetName(), EntryColors.TEXT_ABILITY),
            new EntryPart("on"),
            new LinkUnitEntryPart(target.Character.Name, target, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER),
            new EntryPart("with"),
            new IconEntryPart($"{effector.LinesOfSight[target].cover}Cover", EntryColors.CoverColor(effector.LinesOfSight[target].cover)),
            new ColorEntryPart($"{effector.LinesOfSight[target].cover} cover", EntryColors.TEXT_IMPORTANT),
            new EntryPart(":"),
            new LinkUnitEntryPart(effector.Character.Name, effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER),
        };

        if (selfResults.Landed)
        {
            string selfCriticalText = selfResults.Critical ? " critical" : " ";
            entry.Add(new EntryPart("did"));
            entry.Add(new ColorEntryPart($"{selfResults.Damage}{selfCriticalText} damage", EntryColors.TEXT_IMPORTANT));
        }
        else
        {
            entry.Add(new ColorEntryPart("missed", EntryColors.TEXT_IMPORTANT));
        }

        entry.Add(new EntryPart("and"));
        entry.Add(new LinkUnitEntryPart(duo.Character.Name, duo, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER));

        if (allyResults.Landed)
        {
            string allyCriticalText = allyResults.Critical ? " critical" : " ";
            entry.Add(new EntryPart("did"));
            entry.Add(new ColorEntryPart($"{allyResults.Damage}{allyCriticalText} damage", EntryColors.TEXT_IMPORTANT));
        }
        else
        {
            entry.Add(new ColorEntryPart("missed", EntryColors.TEXT_IMPORTANT));
        }

        return entry.ToArray();
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
            new IconEntryPart($"{effector.LinesOfSight[target].cover}Cover", EntryColors.CoverColor(effector.LinesOfSight[target].cover)),
            new ColorEntryPart($"{effector.LinesOfSight[target].cover} cover", EntryColors.TEXT_IMPORTANT),
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

    public static EntryPart[] GetHunkerDownEntry(GridBasedUnit effector)
    {
        return new EntryPart[2]
        {
            new LinkUnitEntryPart(effector.Character.Name, effector, EntryColors.LINK_UNIT, EntryColors.LINK_UNIT_HOVER),
            new ColorEntryPart("hunkered down", EntryColors.TEXT_IMPORTANT),
        };
    }
}
