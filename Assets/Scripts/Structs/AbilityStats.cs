using System.Collections;
using System.Collections.Generic;

public struct AbilityStats
{
    public float accuracy;
    public float crit;
    public float damage;
    public float protection;

    public AbilityStats(float accuracy, float crit, float damage, float protection)
    {
        this.accuracy = accuracy;
        this.crit = crit;
        this.damage = damage;
        this.protection = protection;
    }

    public void UpdateWithSelfToAllyRelationship(Relationship relationship)
    {
        List<EnumEmotions> listEmotions = relationship.ListEmotions;
        AbilityStats bestModifiers = new AbilityStats(0, 0, 0, 0); // modifiers to chance of missing [0.5, 1], chance of not critting [0.5, 1], damage, and protection [1.2, 2]
        AbilityStats worstModifiers = new AbilityStats(0, 0, 0, 0); // modifiers to chance of hitting [0.5, 1], chance of critting [0.5, 1], damage, and protection [1.2, 2]

        foreach (EnumEmotions emotion in listEmotions)
        {
            switch (emotion)
            {
                case (EnumEmotions.Scorn):
                    float protMod = -0.4f;
                    break;
                case (EnumEmotions.Esteem):
                    protMod = 0.4f;
                    break;
                case (EnumEmotions.Prejudice):
                    protMod = -0.4f;
                    break;
                case (EnumEmotions.Submission):
                    protMod = 0.2f;
                    float successMod = -0.25f;
                    float critMod = -0.25f;
                    break;
                case (EnumEmotions.Terror):
                    successMod = -0.25f;
                    break;
                case (EnumEmotions.ConflictedFeelings):
                    break;
                case (EnumEmotions.Faith):
                    float missMod = -0.5f;
                    protMod = 0.4f;
                    break;
                case (EnumEmotions.Respect):
                    missMod = -0.5f;
                    break;
                case (EnumEmotions.Condescension):
                    protMod = -0.6f;
                    float dmgMod = -0.5f;
                    break;
                case (EnumEmotions.Recognition):
                    missMod = -0.25f;
                    protMod = 0.2f;
                    dmgMod = -0.5f;
                    break;
                case (EnumEmotions.Hate):
                    dmgMod = -0.25f;
                    successMod = -0.75f;
                    break;
                case (EnumEmotions.ReluctantTrust):
                    missMod = -0.5f;
                    dmgMod = -0.5f;
                    break;
                case (EnumEmotions.Hostility):
                    dmgMod = -0.5f;
                    break;
                case (EnumEmotions.Pity):
                    protMod = 0.2f;
                    break;
                case (EnumEmotions.Devotion):
                    protMod = 0.4f;
                    break;
                case (EnumEmotions.Apprehension):
                    successMod = 0.25f;
                    float failCritMod = -0.25f;
                    break;
                case (EnumEmotions.Friendship):
                    missMod = -0.25f;
                    break;
                case (EnumEmotions.Empathy):
                    // action gratuite, pas ici
                    break;
                default:
                    break;
            }
        }
    }
}
