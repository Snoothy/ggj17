using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    static SoundManager _instance;
    public static SoundManager Instance { get { return _instance; } }
    public AudioSource sourceSingleSounds;
    public AudioClip acJump, acStomp, acStompBegin, acHit, acSelect, acHardHit, acHatOff, acSpawn, acLand;

    private void Awake()
    {
        _instance = this;
    }

    public void PlaySound(AudioClip clip)
    {
        sourceSingleSounds.PlayOneShot(clip, 1);
    }
}
