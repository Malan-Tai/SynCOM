using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveSystem.saveFunction();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            SaveSystem.loadFunction();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log(GlobalGameManager.Instance.currentSquad[1].CharacterClass);
        }
    }
}
