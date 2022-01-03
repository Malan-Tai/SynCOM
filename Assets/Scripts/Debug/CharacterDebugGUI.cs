using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDebugGUI : MonoBehaviour
{
    private void OnGUI()
    {
        AllyCharacter current = CombatGameManager.Instance.CurrentUnit?.AllyCharacter;

        if (current == null)
        {
            return;
        }

        float ratio = current.HealthPoints / current.MaxHealth;

        GUI.Box(new Rect(1500, 10, 100, 90), "Character");

        GUI.Label(new Rect(1510, 30, 80, 20), new GUIContent("HP : " + (int)current.HealthPoints + " / " + current.MaxHealth));
        current.HealthPoints = current.MaxHealth * GUI.HorizontalSlider(new Rect(1510, 60, 80, 20), ratio, 0, 1);
    }
}
