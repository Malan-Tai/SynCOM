using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationshipDebugGUI : MonoBehaviour
{
    private void OnGUI()
    {
        AllyUnit current = CombatGameManager.Instance.CurrentUnit;

        if (current == null)
        {
            return;
        }

        int i = 0;
        foreach (AllyUnit unit in CombatGameManager.Instance.AllAllyUnits)
        {
            if (current == unit) continue;
            Relationship relationship = current.AllyCharacter.Relationships[unit.AllyCharacter];

            GUI.Box(new Rect(10, i * 100 + 10, 100, 90), "To " + unit.Character.Name);

            GUI.Label(new Rect(20, i * 100 + 30, 80, 20), new GUIContent("TRU" + relationship.GetGaugeLevel(EnumSentiment.Trust) + ": " + relationship.GetGaugeValue(EnumSentiment.Trust)));
            GUI.Label(new Rect(20, i * 100 + 50, 80, 20), new GUIContent("ADM" + relationship.GetGaugeLevel(EnumSentiment.Admiration) + ": " + relationship.GetGaugeValue(EnumSentiment.Admiration)));
            GUI.Label(new Rect(20, i * 100 + 70, 80, 20), new GUIContent("SYM" + relationship.GetGaugeLevel(EnumSentiment.Sympathy) + ": " + relationship.GetGaugeValue(EnumSentiment.Sympathy)));

            if (GUI.Button(new Rect(120, i * 100 + 10, 100, 20), "Print"))
            {
                //relationship.UpdateEmotions();

                string emotionString = "emotions: ";
                foreach (EnumEmotions emotion in relationship.ListEmotions)
                {
                    emotionString += emotion.ToString() + ", ";
                }
                print(emotionString);
            }

            if (GUI.Button(new Rect(120, i * 100 + 32, 45, 20), "-10"))
            {
                relationship.IncreaseSentiment(EnumSentiment.Trust, -10);
            }
            if (GUI.Button(new Rect(170, i * 100 + 32, 45, 20), "+10"))
            {
                relationship.IncreaseSentiment(EnumSentiment.Trust, 10);
            }

            if (GUI.Button(new Rect(120, i * 100 + 54, 45, 20), "-10"))
            {
                relationship.IncreaseSentiment(EnumSentiment.Admiration, -10);
            }
            if (GUI.Button(new Rect(170, i * 100 + 54, 45, 20), "+10"))
            {
                relationship.IncreaseSentiment(EnumSentiment.Admiration, 10);
            }

            if (GUI.Button(new Rect(120, i * 100 + 76, 45, 20), "-10"))
            {
                relationship.IncreaseSentiment(EnumSentiment.Sympathy, -10);
            }
            if (GUI.Button(new Rect(170, i * 100 + 76, 45, 20), "+10"))
            {
                relationship.IncreaseSentiment(EnumSentiment.Sympathy, 10);
            }

            i++;
        }
    }
}
