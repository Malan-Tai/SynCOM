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
    private SerializedProperty _triggersOnlyOnce;

    // involved
    private SerializedProperty _onlyCheckDuoAlly;
    private SerializedProperty _dontCheckDuoAlly;
    private SerializedProperty _onlyCheckTarget;
    private SerializedProperty _dontCheckTarget;
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

    // heal
    private SerializedProperty _minMaxHealthRatio;

    // kill
    private SerializedProperty _killSteal;

    // start action
    private SerializedProperty _startedAction;

    /// effect
    private SerializedProperty _effectType;
    private SerializedProperty _interrupts;

    // relationship gauges
    private SerializedProperty _reciprocal;
    private SerializedProperty _admChange;
    private SerializedProperty _truChange;
    private SerializedProperty _symChange;
    private SerializedProperty _sourceToTarget;
    private SerializedProperty _admirationChangeSTT;
    private SerializedProperty _trustChangeSTT;
    private SerializedProperty _sympathyChangeSTT;

    // generic chance field
    private SerializedProperty _chance;

    // interruptions
    private SerializedProperty _interruptions;
    private SerializedProperty _interruptionsOnSource;

    // free action
    private SerializedProperty _freeAction;
    private SerializedProperty _freeActionForDuo;

    // sacrifice
    private SerializedProperty _maxRange;

    // buff
    private SerializedProperty _buffsOnSource;
    private SerializedProperty _buffsOnTarget;

    // change action
    private SerializedProperty _changeActionTo;

    #endregion

    // is called once when according object gains focus in the hierachy
    private void OnEnable()
    {
        _triggerType                    = serializedObject.FindProperty("triggerType");
        _triggersOnlyOnce               = serializedObject.FindProperty("triggersOnlyOnce");

        _onlyCheckDuoAlly               = serializedObject.FindProperty("onlyCheckDuoAlly");
        _dontCheckDuoAlly               = serializedObject.FindProperty("dontCheckDuoAlly");
        _onlyCheckTarget                = serializedObject.FindProperty("onlyCheckTarget");
        _dontCheckTarget                = serializedObject.FindProperty("dontCheckTarget");
        _requiresEmotions               = serializedObject.FindProperty("requiresEmotions");
        _requiredEmotionsTowardsSource  = serializedObject.FindProperty("requiredEmotionsTowardsSource");
        _requiredEmotionsTowardsTarget  = serializedObject.FindProperty("requiredEmotionsTowardsTarget");
        
        _targetsAlly                    = serializedObject.FindProperty("targetsAlly");
        _onMiss                         = serializedObject.FindProperty("onMiss");
        _onHit                          = serializedObject.FindProperty("onHit");
        _onCrit                         = serializedObject.FindProperty("onCrit");
        _onDamage                       = serializedObject.FindProperty("onDamage");
        _onFatal                        = serializedObject.FindProperty("onFatal");

        _minMaxHealthRatio              = serializedObject.FindProperty("minMaxHealthRatio");

        _killSteal                      = serializedObject.FindProperty("killSteal");

        _startedAction                  = serializedObject.FindProperty("startedAction");


        _effectType                     = serializedObject.FindProperty("effectType");
        _interrupts                     = serializedObject.FindProperty("interrupts");

        _reciprocal                     = serializedObject.FindProperty("reciprocal");
        _admChange                      = serializedObject.FindProperty("admirationChange");
        _truChange                      = serializedObject.FindProperty("trustChange");
        _symChange                      = serializedObject.FindProperty("sympathyChange");
        _sourceToTarget                 = serializedObject.FindProperty("sourceToCurrent");
        _admirationChangeSTT            = serializedObject.FindProperty("admirationChangeSTC");
        _trustChangeSTT                 = serializedObject.FindProperty("trustChangeSTC");
        _sympathyChangeSTT              = serializedObject.FindProperty("sympathyChangeSTC");

        _chance                         = serializedObject.FindProperty("chance");

        _interruptions                  = serializedObject.FindProperty("interruptionsOnCurrent");
        _interruptionsOnSource          = serializedObject.FindProperty("interruptionsOnSource");

        _freeAction                     = serializedObject.FindProperty("freeAction");
        _freeActionForDuo               = serializedObject.FindProperty("freeActionForDuo");

        _maxRange                       = serializedObject.FindProperty("maxRange");

        _buffsOnSource                  = serializedObject.FindProperty("buffsOnSource");
        _buffsOnTarget                  = serializedObject.FindProperty("buffsOnTarget");

        _changeActionTo                 = serializedObject.FindProperty("changeActionTo");
    }

    public override void OnInspectorGUI()
    {
        // fetch current values from the real instance into the serialized "clone"
        serializedObject.Update();

        if (_foldTrigger = EditorGUILayout.BeginFoldoutHeaderGroup(_foldTrigger, "Trigger"))
        {
            EditorGUILayout.PropertyField(_triggerType);
            EditorGUILayout.PropertyField(_triggersOnlyOnce);

            if (_triggerType.enumValueIndex == (int)RelationshipEventTriggerType.Attack)
            {
                EditorGUILayout.PropertyField(_targetsAlly);
                EditorGUILayout.PropertyField(_onHit);
                EditorGUILayout.PropertyField(_onMiss);
                EditorGUILayout.PropertyField(_onCrit);
                EditorGUILayout.PropertyField(_onDamage);
                EditorGUILayout.PropertyField(_onFatal);
            }
            else if (_triggerType.enumValueIndex == (int)RelationshipEventTriggerType.Heal)
            {
                EditorGUILayout.PropertyField(_minMaxHealthRatio);
            }
            else if (_triggerType.enumValueIndex == (int)RelationshipEventTriggerType.Kill)
            {
                if (_targetsAlly.boolValue) EditorGUILayout.PropertyField(_targetsAlly);
                else if (_killSteal.boolValue) EditorGUILayout.PropertyField(_killSteal);
                else
                {
                    EditorGUILayout.PropertyField(_targetsAlly);
                    EditorGUILayout.PropertyField(_killSteal);
                }
            }
            else if (_triggerType.enumValueIndex == (int)RelationshipEventTriggerType.FriendlyFire)
            {
                EditorGUILayout.PropertyField(_onFatal);
            }
            else if (_triggerType.enumValueIndex == (int)RelationshipEventTriggerType.StartAction)
            {
                EditorGUILayout.PropertyField(_startedAction);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();

        if (_foldInvolved = EditorGUILayout.Foldout(_foldInvolved, "Involved Units", true))
        {
            if (_triggerType.enumValueIndex == (int)RelationshipEventTriggerType.BeginDuo ||
                _triggerType.enumValueIndex == (int)RelationshipEventTriggerType.EndExecutedDuo ||
                _triggerType.enumValueIndex == (int)RelationshipEventTriggerType.StartAction)
            {
                _onlyCheckDuoAlly.boolValue = true;
                EditorGUILayout.PropertyField(_onlyCheckDuoAlly);
            }
            else if (_onlyCheckDuoAlly.boolValue)
            {
                EditorGUILayout.PropertyField(_onlyCheckDuoAlly);
            }
            else if (_dontCheckDuoAlly.boolValue)
            {
                EditorGUILayout.PropertyField(_dontCheckDuoAlly);
            }
            else if (_onlyCheckTarget.boolValue)
            {
                EditorGUILayout.PropertyField(_onlyCheckTarget);
            }
            else if (_dontCheckTarget.boolValue)
            {
                EditorGUILayout.PropertyField(_dontCheckTarget);
            }
            else
            {
                EditorGUILayout.PropertyField(_onlyCheckDuoAlly);
                EditorGUILayout.PropertyField(_dontCheckDuoAlly);
                EditorGUILayout.PropertyField(_onlyCheckTarget);
                EditorGUILayout.PropertyField(_dontCheckTarget);
            }

            EditorGUILayout.PropertyField(_requiresEmotions);

            if (_requiresEmotions.boolValue)
            {
                EditorGUILayout.PropertyField(_requiredEmotionsTowardsSource);
                EditorGUILayout.PropertyField(_requiredEmotionsTowardsTarget);
            }
        }
        EditorGUILayout.Space();

        if (_foldEffect = EditorGUILayout.Foldout(_foldEffect, "Effect", true))
        {
            EditorGUILayout.PropertyField(_effectType);
            EditorGUILayout.PropertyField(_interrupts);

            if (_effectType.enumValueIndex == (int)RelationshipEventEffectType.RelationshipGaugeChange)
            {
                EditorGUILayout.PropertyField(_reciprocal);
                EditorGUILayout.PropertyField(_admChange);
                EditorGUILayout.PropertyField(_truChange);
                EditorGUILayout.PropertyField(_symChange);

                if (!_reciprocal.boolValue)
                {
                    EditorGUILayout.PropertyField(_sourceToTarget);
                    if (_sourceToTarget.boolValue)
                    {
                        EditorGUILayout.PropertyField(_admirationChangeSTT);
                        EditorGUILayout.PropertyField(_trustChangeSTT);
                        EditorGUILayout.PropertyField(_sympathyChangeSTT);
                    }
                }
            }
            else if (_effectType.enumValueIndex == (int)RelationshipEventEffectType.RefuseToDuo ||
                    _effectType.enumValueIndex == (int)RelationshipEventEffectType.StealDuo ||
                    _effectType.enumValueIndex == (int)RelationshipEventEffectType.FreeAttack)
            {
                EditorGUILayout.PropertyField(_chance);
            }
            else if (_effectType.enumValueIndex == (int)RelationshipEventEffectType.FreeAction)
            {
                EditorGUILayout.PropertyField(_chance);
                EditorGUILayout.PropertyField(_freeAction);
                EditorGUILayout.PropertyField(_freeActionForDuo);
            }
            else if (_effectType.enumValueIndex == (int)RelationshipEventEffectType.Sacrifice)
            {
                EditorGUILayout.PropertyField(_chance);
                EditorGUILayout.PropertyField(_maxRange);
            }
            else if (_effectType.enumValueIndex == (int)RelationshipEventEffectType.Buff)
            {
                EditorGUILayout.PropertyField(_buffsOnSource);
                EditorGUILayout.PropertyField(_buffsOnTarget);
            }
            else if (_effectType.enumValueIndex == (int)RelationshipEventEffectType.ChangeAction)
            {
                EditorGUILayout.PropertyField(_changeActionTo);
                EditorGUILayout.PropertyField(_chance);
            }

            if (_interrupts.boolValue)
            {
                EditorGUILayout.PropertyField(_interruptions);
                EditorGUILayout.PropertyField(_interruptionsOnSource);
            }
        }

        // write back serialized values to the real instance
        // automatically handles all marking dirty and undo/redo
        serializedObject.ApplyModifiedProperties();
    }
}
