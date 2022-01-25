using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEngine : MonoBehaviour
{
    #region Singleton
    private static RandomEngine instance;
    public static RandomEngine Instance { get { return instance; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    #endregion

    public virtual int Range(int minInclusive, int maxExclusive)
    {
        return UnityEngine.Random.Range(minInclusive, maxExclusive);
    }

    public virtual float Range(float minInclusive, float maxInclusive)
    {
        return UnityEngine.Random.Range(minInclusive, maxInclusive);
    }
}
