using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyCharacter : Character
{
    private static Dictionary<EnumClasses, List<EnumTraits>> s_mandatoryTraits = new Dictionary<EnumClasses, List<EnumTraits>>(){
        {EnumClasses.Berserker, new List<EnumTraits> {EnumTraits.Stocky}},
        {EnumClasses.Engineer, new List<EnumTraits> {EnumTraits.Stocky}},
        {EnumClasses.Hitman, new List<EnumTraits> {EnumTraits.Racist}},
        {EnumClasses.Sniper, new List<EnumTraits> {EnumTraits.Racist}},
        {EnumClasses.HoundMaster, new List<EnumTraits> {EnumTraits.Ugly}},
        {EnumClasses.Smuggler, new List<EnumTraits> {EnumTraits.Ugly}}
    };

    private static Dictionary<EnumClasses, List<EnumTraits>> s_commonPossibleTraits = new Dictionary<EnumClasses, List<EnumTraits>>(){
        {EnumClasses.Berserker, new List<EnumTraits> {EnumTraits.Fearful,EnumTraits.Brave}},
        {EnumClasses.Engineer, new List<EnumTraits> {EnumTraits.Fearful,EnumTraits.Brave}},
        {EnumClasses.Hitman, new List<EnumTraits> {EnumTraits.Lovely, EnumTraits.Sprinter}},
        {EnumClasses.Sniper, new List<EnumTraits> {EnumTraits.Lovely,EnumTraits.Sprinter}},
        {EnumClasses.HoundMaster, new List<EnumTraits> {EnumTraits.Brave,EnumTraits.Sprinter}},
        {EnumClasses.Smuggler, new List<EnumTraits> {EnumTraits.Brave,EnumTraits.Sprinter}}
    };

    //Character's archetype
    private EnumClasses _class;
    private List<EnumTraits> _traits = new List<EnumTraits>();

    private Dictionary<AllyCharacter, Relationship> _relationships;
    public Dictionary<AllyCharacter, Relationship> Relationships { get { return _relationships; } }

    public AllyCharacter(EnumClasses characterClass, float maxHealth, float damage, float accuracy, float dodge, float critChances, float rangeShot, float movementPoints, float weight) :
        base(maxHealth, damage, accuracy, dodge, critChances, rangeShot, movementPoints, weight)
    {
        _class = characterClass;
        addMandatoryTraits(_class);
        addRandomTrait(_class);
    }

    public void InitializeRelationships()
    {
        _relationships = new Dictionary<AllyCharacter, Relationship>();
        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            if (ally.Character != this)
            {
                _relationships.Add(ally.AllyCharacter, new Relationship(this));
            }
        }
    }

    private EnumTraits GetRandomTraitsFromClass(EnumClasses characterClass)
    {

        int indice = Random.Range(0, s_mandatoryTraits[characterClass].Count);
        EnumTraits newTrait = s_mandatoryTraits[characterClass][indice];
        return newTrait;
    }

    private void addMandatoryTraits(EnumClasses characterClass)
    {
        for (int i = 0; i < s_mandatoryTraits[characterClass].Count; i++)
        {
            _traits.Add(s_mandatoryTraits[characterClass][i]);
        }
    }

    private void addRandomTrait(EnumClasses characterClass)
    {
        _traits.Add(GetRandomTraitsFromClass(characterClass));
    }
}
