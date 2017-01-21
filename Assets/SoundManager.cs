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

    public AudioMixerSnapshot snap1, snap2, snap3;
    bool isPlayingSelection = true;

    /*private void Awake()
    {
        _instance = this;
        FadeToSelection()
    }

    private void Start()
    {
        GameController.GameStateChanged += GameController_GameStateChanged;
    }

    private void GameController_GameStateChanged()
    {
        if(gc.IsGameStarted)
        {

        }
        else
        {

        }
    }*/

    public void PlaySound(AudioClip clip)
    {
        sourceSingleSounds.PlayOneShot(clip, 1);
    }
}
