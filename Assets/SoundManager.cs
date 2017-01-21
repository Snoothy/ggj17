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
    public AudioClip acJump, acStomp, acStompBegin, acHit, acSelect, acHardHit, acHatOff, acSpawn, acLand, acExplode;

    public AudioSource source1, source2, source3;
    float[] values = new float[] { 1, 0, 0 };
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
        source1.volume = source1.volume * (1f - fadeVal) + values[0] * fadeVal;
        source2.volume = source2.volume * (1f - fadeVal) + values[1] * fadeVal;
        source3.volume = source3.volume * (1f - fadeVal) + values[2] * fadeVal;
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
                if(UnityEngine.Random.value > 0.5f)
                {
                    values[1] = 0.5f;
                    values[2] = 0;
                }
                else
                {
                    values[1] = 0f;
                    values[2] = 0.5f;
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
            }
        }
        wasGameStarted = gc.IsGameStarted;
    }

    public void PlaySound(AudioClip clip)
    {
        sourceSingleSounds.PlayOneShot(clip, 1);
    }
}
