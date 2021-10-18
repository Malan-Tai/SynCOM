using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationshipDebugGUI : MonoBehaviour
{
    private void OnGUI()
    {
        AllyUnit current = CombatGameManager.Instance.CurrentUnit;

        int i = 0;
        foreach (Relationship relationship in current.AllyCharacter.Relationships.Values)
        {
            GUI.Box  (new Rect(10, i * 100 + 10, 100, 90), "Relationship #" + i);

            GUI.Label(new Rect(20, i * 100 + 30, 80, 20), new GUIContent("Tru: " + relationship.GetGaugeValue(EnumSentiment.Trust)));
            GUI.Label(new Rect(20, i * 100 + 50, 80, 20), new GUIContent("Adm: " + relationship.GetGaugeValue(EnumSentiment.Admiration)));
            GUI.Label(new Rect(20, i * 100 + 70, 80, 20), new GUIContent("Sym: " + relationship.GetGaugeValue(EnumSentiment.Sympathy)));

            i++;
        }
    }
}
