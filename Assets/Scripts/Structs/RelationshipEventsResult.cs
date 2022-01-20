using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RelationshipEventsResult
{
    public bool refusedDuo;
    public bool freeActionForSource;
    public bool freeActionForDuo;

    public GridBasedUnit    sacrificedTarget;
    public AllyUnit         stolenDuoUnit;

    public List<InterruptionParameters>     interruptions;
    public List<Buff>                       buffs;

    public ChangeActionTypes changedActionTo;
}
