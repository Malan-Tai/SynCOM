using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatEngine : RandomEngine
{
    private enum NextResult { Random, Min, Max };
    private NextResult _next;

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
