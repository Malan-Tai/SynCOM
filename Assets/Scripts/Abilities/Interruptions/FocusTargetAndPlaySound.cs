using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusTargetAndPlaySound : Interruption
{
    private float _time;
    private GridBasedUnit _target;
    private SoundManager.Sound _sound;

    protected override void Init(InterruptionParameters parameters)
    {
        _target = parameters.target;
        _time = parameters.time;
        _sound = parameters.sound;
    }

    protected override IEnumerator InterruptionCoroutine()
    {
        if (_sound != SoundManager.Sound.None)
        {
            CombatGameManager.Instance.Camera.SwitchParenthood(_target);
            yield return _time / 2f;

            SoundManager.PlaySound(_sound);
            yield return new WaitForSeconds(SoundManager.GetAudioClipLength(_sound));

            yield return _time / 2f;
        }

        IsDone = true;
    }
}
