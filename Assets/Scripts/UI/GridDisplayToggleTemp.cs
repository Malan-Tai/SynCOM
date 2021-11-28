using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridDisplayToggleTemp : MonoBehaviour
{
    private void Start()
    {
        Toggle toggle = GetComponent<Toggle>();

        CombatGameManager.Instance.TileDisplay.DisplayGrid(toggle.isOn);
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(
            (bool v) =>
            {
                CombatGameManager.Instance.TileDisplay.DisplayGrid(v);
            }
        );
    }
}
