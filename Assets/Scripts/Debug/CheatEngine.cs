using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatEngine : RandomEngine
{
    [System.Serializable]
    private class PresetRelationship
    {
        public int selfIndex;
        public int allyIndex;
        public int admirationLvl;
        public int admirationFill;
        public int trustLvl;
        public int trustFill;
        public int sympathyLvl;
        public int sympathyFill;
    }

    private enum NextResult { Random, Min, Max };
    private NextResult _next;

    [SerializeField]
    private EnumClasses[] _classes;

    [SerializeField]
    private PresetRelationship[] _relationships;

    private void Start()
    {
        List<AllyCharacter> cheatSquad = new List<AllyCharacter>();

        GlobalGameManager.Instance.allCharacters.Clear();
        for (int i = 0; i < _classes.Length; i++)
        {
            AllyCharacter character = AllyCharacter.GetRandomAllyCharacter(_classes[i]);
            cheatSquad.Add(character);
            GlobalGameManager.Instance.SetSquadUnit(i, character);
            GlobalGameManager.Instance.AddCharacter(character);
        };

        foreach (PresetRelationship rel in _relationships)
        {
            if (rel.selfIndex < 0 || rel.selfIndex >= cheatSquad.Count || rel.allyIndex < 0 || rel.allyIndex >= cheatSquad.Count) continue;

            AllyCharacter self = cheatSquad[rel.selfIndex];
            AllyCharacter ally = cheatSquad[rel.allyIndex];
            Relationship relationship = self.Relationships[ally];

            if (rel.admirationLvl > 0)
                for (int i = 0; i < rel.admirationLvl; i++) relationship.IncreaseSentiment(EnumSentiment.Admiration, 50);
            else
                for (int i = 0; i > rel.admirationLvl; i--) relationship.IncreaseSentiment(EnumSentiment.Admiration, -50);

            if (rel.trustLvl > 0)
                for (int i = 0; i < rel.trustLvl; i++) relationship.IncreaseSentiment(EnumSentiment.Trust, 50);
            else
                for (int i = 0; i > rel.trustLvl; i--) relationship.IncreaseSentiment(EnumSentiment.Trust, -50);

            if (rel.sympathyLvl > 0)
                for (int i = 0; i < rel.sympathyLvl; i++) relationship.IncreaseSentiment(EnumSentiment.Sympathy, 50);
            else
                for (int i = 0; i > rel.sympathyLvl; i--) relationship.IncreaseSentiment(EnumSentiment.Sympathy, -50);

            relationship.IncreaseSentiment(EnumSentiment.Admiration,    rel.admirationFill);
            relationship.IncreaseSentiment(EnumSentiment.Trust,         rel.trustFill);
            relationship.IncreaseSentiment(EnumSentiment.Sympathy,      rel.sympathyFill);
        }
    }

    public override int Range(int minInclusive, int maxExclusive)
    {
        int res;
        switch (_next)
        {
            case NextResult.Min:
                res = minInclusive;
                break;

            case NextResult.Max:
                res = maxExclusive;
                break;

            default:
                res = UnityEngine.Random.Range(minInclusive, maxExclusive);
                break;
        }

        return res;
    }

    public override float Range(float minInclusive, float maxInclusive)
    {
        switch (_next)
        {
            case NextResult.Min:
                return minInclusive;

            case NextResult.Max:
                return maxInclusive;

            default:
                return UnityEngine.Random.Range(minInclusive, maxInclusive);
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.F1)) // F1 is min, F2 is max
            _next = NextResult.Min;
        else if (Input.GetKey(KeyCode.F2))
            _next = NextResult.Max;
        else
            _next = NextResult.Random;
    }
}
