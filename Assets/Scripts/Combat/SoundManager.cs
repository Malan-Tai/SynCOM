using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundManager
{
    public enum Sound
    {
        //voices
        FWRelentlessAssaultEngineerMale, FLRelentlessAssaultEngineerMale, EWRelentlessAssaultEngineerMale, ELRelentlessAssaultEngineerMale,
        FWRelentlessAssaultEngineerFemale, FLRelentlessAssaultEngineerFemale, EWRelentlessAssaultEngineerFemale, ELRelentlessAssaultEngineerFemale,
        FWRelentlessAssaultSmugglerMale, FLRelentlessAssaultSmugglerMale, EWRelentlessAssaultSmugglerMale, ELRelentlessAssaultSmugglerMale,
        FWRelentlessAssaultSmugglerFemale, FLRelentlessAssaultSmugglerFemale, EWRelentlessAssaultSmugglerFemale, ELRelentlessAssaultSmugglerFemale,
        FWRelentlessAssaultBerserkerMale, FLRelentlessAssaultBerserkerMale, EWRelentlessAssaultBerserkerMale,ELRelentlessAssaultBerserkerMale,
        FWRelentlessAssaultBerserkerFeMale, FLRelentlessAssaultBerserkerFeMale, EWRelentlessAssaultBerserkerFeMale, ELRelentlessAssaultBerserkerFeMale,
        FWRelentlessAssaultBodyguardMale, FLRelentlessAssaultBodyguardMale, EWRelentlessAssaultBodyguardMale, ELRelentlessAssaultBodyguardMale,
        FWRelentlessAssaultBodyguardFemale, FLRelentlessAssaultBodyguardFemale, EWRelentlessAssaultBodyguardFemale, ELRelentlessAssaultBodyguardFemale,
        FWRelentlessAssaultSniperMale, FLRelentlessAssaultSniperMale, EWRelentlessAssaultSniperMale, ELRelentlessAssaultSniperMale,
        FWRelentlessAssaultSniperFemale, FLRelentlessAssaultSniperFemale, EWRelentlessAssaultSniperFemale, ELRelentlessAssaultSniperFemale,
        FWRelentlessAssaultAlchemistMale, FLRelentlessAssaultAlchemistMale, EWRelentlessAssaultAlchemistMale, ELRelentlessAssaultAlchemistMale,
        FWRelentlessAssaultAlchemistFemale, FLRelentlessAssaultAlchemistFemale, EWRelentlessAssaultAlchemistFemale, ELRelentlessAssaultAlchemistFemale,

        FWGrenadeTossMale, FLGrenadeTossMale, EWGrenadeTossMale, ELGrenadeTossMale,
        FWGrenadeTossFemale, FLGrenadeTossFemale, EWGrenadeTossFemale, ELGrenadeTossFemale,
        FWMortarMale, FLMortarMale, EWMortarMale, ELMortarMale,
        FWMortarFemale, FLMortarFemale, EWMortarFemale, ELMortarFemale,
        FWDevouringMale, FLDevouringMale, EWDevouringMale, ELDevouringMale,
        FWDevrouingFemale, FLDevrouingFemale, EWDevrouingFemale, ELDevrouingFemale,
        FWDwarfTossMale, FLDwarfTossMale, EWDwarfTossMale, ELDwarfTossMale, 
        FWDwarfTossFemale, FLDwarfTossFemale, EWDwarfTossFemale, ELDwarfTossFemale,
        FWLongShotMale, FLLongShotMale, EWLongShotMale, ELLongShotMale, 
        FWLongShotFemale, FLLongShotFemale, EWLongShotFemale, ELLongShotFemale, 
        FWSuppressiveFireMale, FLSuppressiveFireMale, EWSuppressiveFireMale, ELSuppressiveFireMale, 
        FWSuppressiveFireFemale, FLSuppressiveFireFemale, EWSuppressiveFireFemale, ELSuppressiveFireFemale,
        FWShieldMale, FLShieldMale, EWShieldMale, ELShieldMale,
        FWShieldFemale, FLShieldFemale, EWShieldFemale, ELShieldFemale,
        FWChargeMale, FLChargeMale, EWChargeMale, ELChargeMale,
        FWChargeFemale, FLChargeFemale, EWChargeFemale, ELChargeFemale,
        FSmugglerMale, ESmugglerMale,
        FSmugglerFemale, ESmugglerFemale,


        //Sound Effects
        BasicShotGatling, BasicShotSniper, BasicPunch, BasicEnemyShot, Devouring, DwarfToss, FirstAid, GrenadeToss,
        HealingRain, HunkerDown, LongShot, Mortar, ShieldAndStrike, Smuggle, SuppressiveFire, PepTalk, WildCharge,
        None
    }

    //private static GameObject oneShotGameObject;
    //private static AudioSource oneShotAudioSource;

    public static void PlaySound(Sound sound, float volume = 1f)
    {
        if (sound == Sound.None) return;

        GameObject soundGameObject = new GameObject("Sound");
        
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        audioSource.clip = GetAudioClip(sound);
        audioSource.volume = volume ;
        audioSource.Play();
        Object.Destroy(soundGameObject, audioSource.clip.length);
    }

    public static AudioClip GetAudioClip(Sound sound)
    {
        if (sound == Sound.None) return null;

        foreach (SoundAssets.SoundAudioClip soundAudioClip in SoundAssets.instance.soundAudioClipsArray)
        {
            if (soundAudioClip.sound == sound)
            {
                return soundAudioClip.audioClip;
            }
        }
        Debug.LogError("Sound " + sound + " not found");
        return null;
    }

    public static float GetAudioClipLength(Sound sound)
    {
        if (sound == Sound.None) return 0f;

        foreach (SoundAssets.SoundAudioClip soundAudioClip in SoundAssets.instance.soundAudioClipsArray)
        {
            if (soundAudioClip.sound == sound)
            {
                return soundAudioClip.audioClip.length;
            }
        }
        Debug.LogError("Sound " + sound + " not found");
        return 0f;
    }
}
