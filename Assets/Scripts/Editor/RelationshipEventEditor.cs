using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RelationshipEvent))]
public class RelationshipEventEditor : Editor
{
    private bool _foldTrigger = true;
    private bool _foldInvolved = true;
    private bool _foldEffect = true;

    #region properties
    /// trigger
    private SerializedProperty _triggerType;

    // involved
    private SerializedProperty _onlyCheckDuoAlly;
    private SerializedProperty _requiresEmotions;
    private SerializedProperty _requiredEmotionsTowardsSource;
    private SerializedProperty _requiredEmotionsTowardsTarget;

    // attack
    private SerializedProperty _targetsAlly;
    private SerializedProperty _onMiss;
    private SerializedProperty _onHit;
    private SerializedProperty _onCrit;
    private SerializedProperty _onDamage;
    private SerializedProperty _onFatal;

    /// effect
    private SerializedProperty _effectType;
    private SerializedProperty _interrupts;

    // relationship gauges
    private SerializedProperty _reciprocal;
    private SerializedProperty _admChange;
    private SerializedProperty _truChange;
    private SerializedProperty _symChange;
    #endregion

    // is called once when according object gains focus in the hierachy
    private void OnEnable()
    {
        _triggerType                    = serializedObject.FindProperty("triggerType");

        _onlyCheckDuoAlly               = serializedObject.FindProperty("onlyCheckDuoAlly");
        _requiresEmotions               = serializedObject.FindProperty("requiresEmotions");
        _requiredEmotionsTowardsSource  = serializedObject.FindProperty("requiredEmotionsTowardsSource");
        _requiredEmotionsTowardsTarget  = serializedObject.FindProperty("requiredEmotionsTowardsTarget");
        
        _targetsAlly                    = serializedObject.FindProperty("targetsAlly");
        _onMiss                         = serializedObject.FindProperty("onMiss");
        _onHit                          = serializedObject.FindProperty("onHit");
        _onCrit                         = serializedObject.FindProperty("onCrit");
        _onDamage                       = serializedObject.FindProperty("onDamage");
        _onFatal                        = serializedObject.FindProperty("onFatal");


        _effectType                     = serializedObject.FindProperty("effectType");
        _interrupts                     = serializedObject.FindProperty("interrupts");

        _reciprocal                     = serializedObject.FindProperty("reciprocal");
        _admChange                      = serializedObject.FindProperty("admirationChange");
        _truChange                      = serializedObject.FindProperty("trustChange");
        _symChange                      = serializedObject.FindProperty("sympathyChange");
    }

    public override void OnInspectorGUI()
    {
        // fetch current values from the real instance into the serialized "clone"
        serializedObject.Update();

        if (_foldTrigger = EditorGUILayout.BeginFoldoutHeaderGroup(_foldTrigger, "Trigger"))
        {
            EditorGUILayout.PropertyField(_triggerType);

            if (_triggerType.enumValueIndex == (int)RelationshipEventTriggerType.Attack)
            {
                EditorGUILayout.PropertyField(_targetsAlly);
                EditorGUILayout.PropertyField(_onHit);
                EditorGUILayout.PropertyField(_onMiss);
                EditorGUILayout.PropertyField(_onCrit);
                EditorGUILayout.PropertyField(_onDamage);
                EditorGUILayout.PropertyField(_onFatal);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();

        if (_foldInvolved = EditorGUILayout.Foldout(_foldInvolved, "Involved Units", true))
        {
            EditorGUILayout.PropertyField(_onlyCheckDuoAlly);
            EditorGUILayout.PropertyField(_requiresEmotions);

            if (_requiresEmotions.boolValue)
            {
                EditorGUILayout.PropertyField(_requiredEmotionsTowardsSource);
                EditorGUILayout.PropertyField(_requiredEmotionsTowardsTarget);
            }
        }
        EditorGUILayout.Space();

        if (_foldEffect = EditorGUILayout.BeginFoldoutHeaderGroup(_foldEffect, "Effect"))
        {
            EditorGUILayout.PropertyField(_effectType);
            EditorGUILayout.PropertyField(_interrupts);

            if (_effectType.enumValueIndex == (int)RelationshipEventEffectType.RelationshipGaugeChange)
            {
                EditorGUILayout.PropertyField(_reciprocal);
                EditorGUILayout.PropertyField(_admChange);
                EditorGUILayout.PropertyField(_truChange);
                EditorGUILayout.PropertyField(_symChange);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // write back serialized values to the real instance
        // automatically handles all marking dirty and undo/redo
        serializedObject.ApplyModifiedProperties();
    }
}
