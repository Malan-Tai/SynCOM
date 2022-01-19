using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mortar : BaseDuoAbility
{
    private List<Tile> _areaOfEffectTiles = new List<Tile>();
    private List<EnemyUnit> _targets = new List<EnemyUnit>();
    private List<AllyUnit> _allyTargets = new List<AllyUnit>();

    private AbilityStats _selfShotStats;

    private int _radius = 3;

    public override string GetAllyDescription()
    {
        return "Send a beacon in the air to indicate your position. You take cover but have a small chance to get hit.";
    }
    public override string GetDescription()
    {
        return "Fire a splinter-filled grenade on you ally's position, hoping they’ll take cover in time.";
    }
    public override string GetName()
    {
        return "Mortar";
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null;
    }

    protected override void ChooseAlly()
    {
        _selfShotStats = new AbilityStats(0, 0, 1.5f, 0, 0, _effector);
        _selfShotStats.UpdateWithEmotionModifiers(_chosenAlly);

        _areaOfEffectTiles = CombatGameManager.Instance.GridMap.GetAreaOfEffectDiamond(_chosenAlly.GridPosition, _radius);
        CombatGameManager.Instance.TileDisplay.DisplayTileZone("DamageZone", _areaOfEffectTiles, false);

        _targets.Clear();
        foreach (EnemyUnit enemy in CombatGameManager.Instance.EnemyUnits)
        {
            if ((enemy.GridPosition - _chosenAlly.GridPosition).magnitude <= _radius) //That's a circle not a diamond
            {
                _targets.Add(enemy);
            }
        }
        _allyTargets.Clear();
        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            if ((ally.GridPosition - _chosenAlly.GridPosition).magnitude <= _radius) //That's a circle not a diamond
            {
                _allyTargets.Add(ally);
            }
        }

        //CombatGameManager.Instance.Camera.SwitchParenthood(_chosenAlly);
    }

    protected override void EnemyTargetingInput()
    {

    }

    public override void Execute()
    {
        _allyTargets.Remove(_chosenAlly);

        // Only the _chosenAlly knows the attack is incomming and (almost) always take cover
        if (UnityEngine.Random.Range(0, 100) > 90)
        {
            Debug.Log("[Mortar] Ally didn't take cover in time");
            FriendlyFireDamage(_effector, _chosenAlly, _selfShotStats.GetDamage(), _chosenAlly);
        }

        foreach (EnemyUnit target in _targets)
        {
            SelfShoot(target, _selfShotStats, alwaysHit: true, canCrit: false);
        }
        foreach (AllyUnit ally in _allyTargets)
        {
            FriendlyFireDamage(_effector, ally, _selfShotStats.GetDamage(), ally);
        }
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return true;
    }

    public override string GetShortDescription()
    {
        return "Fires a grenade to a beacon thrown by an ally, who has a chance to be hit.";
    }

    protected override void EndAbility()
    {
        base.EndAbility();
        CombatGameManager.Instance.TileDisplay.HideTileZone("DamageZone");
    }
}
