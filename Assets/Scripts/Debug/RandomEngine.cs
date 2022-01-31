using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEngine : MonoBehaviour
{
    #region Singleton
    private static RandomEngine _instance;
    public static RandomEngine Instance { get { return _instance; } }
    private bool _toNullify = true;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            _toNullify = true;
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (_toNullify) _instance = null;
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
