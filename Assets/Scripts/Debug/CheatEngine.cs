using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatEngine : RandomEngine
{
    public override int Range(int minInclusive, int maxExclusive)
    {
        if (minInclusive == 0 && maxExclusive == 100)
        {
            return 0;
        }

        return UnityEngine.Random.Range(minInclusive, maxExclusive);
    }

    public override float Range(float minInclusive, float maxInclusive)
    {
        if (minInclusive == 0f && maxInclusive == 1f)
        {
            return 0f;
        }

        return UnityEngine.Random.Range(minInclusive, maxInclusive);
    }
}
