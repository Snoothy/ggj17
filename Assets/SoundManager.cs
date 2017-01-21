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
    public SoundClip scJump, scStomp, scStompBegin, scHit, scSelect, scHardHit, scHatOff, scSpawn, scLand, scExplode;
    [Header("Audio for hats")]
    public AudioClip[] hatSounds;
    public SoundClip[] hatSoundClips;

    public AudioSource[] sources;
    float[] values = new float[] { 1, 0, 0, 0, 0, 0 };
    float fadeVal = 1;
    bool isSelectionPlaying = true, wasGameStarted = false;

    private void Awake()
    {
        _instance = this;
        Fade();
    }

    private void Start()
    {
        GameController.GameStateChanged += Fade;
    }

    private void Update()
    {
        for(int i = 0; i < sources.Length; i++)
        {
            sources[i].volume = sources[i].volume * (1f - fadeVal) + values[i] * fadeVal;
        }
        fadeVal += Time.deltaTime * 0.35f;
        fadeVal = Mathf.Clamp(fadeVal, 0, 1);
    }

    void Fade()
    {
        if(gc.IsGameStarted != wasGameStarted)
        {
            if (gc.IsGameStarted)
            {
                if(isSelectionPlaying)
                    fadeVal = 0;

                isSelectionPlaying = false;
                values[0] = 0;
                values[1] = 0;
                values[2] = 0;
                
                if (UnityEngine.Random.value > 0.5f)
                {
                    values[3] = 0.5f;
                    values[4] = 0;
                    values[5] = 0;
                }
                else
                {
                    values[3] = 0f;
                    values[4] = 0.5f;
                    values[5] = 0;
                }
            }
            else
            {
                if (!isSelectionPlaying)
                    fadeVal = 0;

                isSelectionPlaying = true;
                values[0] = 0.5f;
                values[1] = 0;
                values[2] = 0;
                values[3] = 0;
                values[4] = 0;
                values[5] = 0;
            }
        }
        wasGameStarted = gc.IsGameStarted;
    }

    public void PlaySound(SoundClip clip)
    {
        sourceSingleSounds.PlayOneShot(clip.clip, clip.volume);
    }

    public void PlayHatSound(int hatid)
    {
        PlaySound(hatSoundClips[hatid % hatSounds.Length]);
    }

    [System.Serializable]
    public class SoundClip
    {
        public AudioClip clip;
        public float volume;
    }
}
