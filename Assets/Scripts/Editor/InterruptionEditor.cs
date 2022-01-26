using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InterruptionScriptableObject))]
public class InterruptionEditor : Editor
{
    #region properties
    private SerializedProperty _interruptionType;

    private SerializedProperty _time;
    private SerializedProperty _text;
    private SerializedProperty _sprite;
    #endregion

    // is called once when according object gains focus in the hierachy
    private void OnEnable()
    {
        _interruptionType = serializedObject.FindProperty("interruptionType");

        _time = serializedObject.FindProperty("time");
        _text = serializedObject.FindProperty("text");
        _sprite = serializedObject.FindProperty("sprite");
    }

    public override void OnInspectorGUI()
    {
        // fetch current values from the real instance into the serialized "clone"
        serializedObject.Update();

        EditorGUILayout.PropertyField(_interruptionType);

        if (_interruptionType.enumValueIndex == (int)InterruptionType.FocusTargetForGivenTime)
        {
            EditorGUILayout.PropertyField(_time);
        }
        else if (_interruptionType.enumValueIndex == (int)InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback)
        {
            EditorGUILayout.PropertyField(_time);
            EditorGUILayout.PropertyField(_text);
        }
        else if (_interruptionType.enumValueIndex == (int)InterruptionType.FocusTargetForGivenTimeAndFireImageFeedback)
        {
            EditorGUILayout.PropertyField(_time);
            EditorGUILayout.PropertyField(_sprite);
        }

        // write back serialized values to the real instance
        // automatically handles all marking dirty and undo/redo
        serializedObject.ApplyModifiedProperties();
    }
}
