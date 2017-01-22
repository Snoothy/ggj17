using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    static SoundManager _instance;
    public GameController gc;
    public static SoundManager Instance { get { return _instance; } }
    public AudioSource sourceSingleSounds;
    //public AudioClip acJump, acStomp, acStompBegin, acHit, acSelect, acHardHit, acHatOff, acSpawn, acLand, acExplode;
    public SoundClip scJump, scStomp, scStompBegin, scHit, scSelect, scHardHit, scHatOff, scSpawn, scLand, scExplode, scVox1, scVox2, scVox3, scVox4, scBack, scSiren;
    [Header("Audio for hats")]
    public SoundClip[] hatSoundClips;

    public AudioSource[] sources;
    public float FadeSecondsSplash = 4.5f;
    public float FadeSecondsBattle = 6f;
    float[] values = new float[] { 1, 0, 0, 0, 0, 0 };
    float fadeVal = 1;
    bool isSplash = false;

    private void Awake()
    {
        _instance = this;
        FadeSplash();
    }

    private void Start()
    {

    }

    private void Update()
    {
        for(int i = 0; i < sources.Length; i++)
        {
            sources[i].volume = sources[i].volume * (1f - fadeVal) + values[i] * fadeVal;
        }
        fadeVal += Time.deltaTime / (isSplash ? FadeSecondsSplash : FadeSecondsBattle);
        fadeVal = Mathf.Clamp(fadeVal, 0, 1);
    }

    public void FadeSplash()
    {
        Debug.Log("SPLASH");
        isSplash = true;
        fadeVal = 0;

        values[0] = 0.5f;
        values[1] = 0;
        values[2] = 0;
        values[3] = 0;
        values[4] = 0;
        values[5] = 0;
    }

    public void FadeBattle()
    {
        Debug.Log("BATTLE");
        isSplash = false;
        fadeVal = 0;

        values[0] = 0;
        values[1] = 0;
        values[2] = 0;
        /*int maxWins = 0;
        foreach (PlayerControls pc in gc.GetJoinedPlayers())
        {
            maxWins = pc.GetWins > maxWins ? pc.GetWins : maxWins;
        }
        values[3] = maxWins == 0 ? 0.5f : 0;
        values[4] = maxWins == 1 ? 0.5f : 0;
        values[5] = maxWins == 2 ? 0.5f : 0;*/

        values[3] = gc.currentRound < 3 ? 0.5f : 0;
        values[4] = gc.currentRound >= 3 && gc.currentRound < 6 ? 0.5f : 0;
        values[5] = gc.currentRound >= 6 ? 0.5f : 0;
    }

    public void PlaySound(SoundClip clip)
    {
        sourceSingleSounds.PlayOneShot(clip.clip, clip.volume);
    }

    public void PlayHatSound(int hatid)
    {
        PlaySound(hatSoundClips[hatid % hatSoundClips.Length]);
    }

    public void PlayRandomHatSound()
    {
        PlaySound(hatSoundClips[(int)(UnityEngine.Random.value * hatSoundClips.Length) % hatSoundClips.Length]);
    }

    [System.Serializable]
    public class SoundClip
    {
        public AudioClip clip;
        public float volume;
    }
}
