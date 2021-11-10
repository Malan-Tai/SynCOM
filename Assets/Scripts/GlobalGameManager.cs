using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameManager : MonoBehaviour
{
    #region Singleton
    private static GlobalGameManager instance;
    public static GlobalGameManager Instance { get { return instance; } }

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

    private List<AllyCharacter> _allCharacters;

    private int _money;

    private Dictionary<RegionName, int> _controlStatus;
}
