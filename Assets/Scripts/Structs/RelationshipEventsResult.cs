using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RelationshipEventsResult
{
    public bool refusedDuo;
    public bool freeActionForSource;
    public bool freeActionForDuo;
    public List<InterruptionParameters> interruptions;
}
