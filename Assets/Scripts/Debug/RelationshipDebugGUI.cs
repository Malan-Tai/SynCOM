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
            GUI.Box(new Rect(10, i * 100 + 10, 100, 90), "Relationship #" + i);

            GUI.Label(new Rect(20, i * 100 + 30, 80, 20), new GUIContent("TRU" + relationship.GetGaugeLevel(EnumSentiment.Trust) + ": " + relationship.GetGaugeValue(EnumSentiment.Trust)));
            GUI.Label(new Rect(20, i * 100 + 50, 80, 20), new GUIContent("ADM" + relationship.GetGaugeLevel(EnumSentiment.Admiration) + ": " + relationship.GetGaugeValue(EnumSentiment.Admiration)));
            GUI.Label(new Rect(20, i * 100 + 70, 80, 20), new GUIContent("SYM" + relationship.GetGaugeLevel(EnumSentiment.Sympathy) + ": " + relationship.GetGaugeValue(EnumSentiment.Sympathy)));

            if (GUI.Button(new Rect(120, i * 100 + 10, 100, 20), "Update & Print"))
            {
                relationship.UpdateEmotions();

                string emotionString = "emotions: ";
                foreach (EnumEmotions emotion in relationship.ListEmotions)
                {
                    emotionString += emotion.ToString() + ", ";
                }
                print(emotionString);
            }

            i++;
        }
    }
}
