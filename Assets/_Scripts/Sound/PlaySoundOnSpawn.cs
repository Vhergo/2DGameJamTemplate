using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnSpawn : MonoBehaviour
{
    // Put this script on any object you want to have a spawn sound
    // Attach the sound you want in the inspector
    [SerializeField] private AudioClip clip;

    private void Start() {
        SoundManager.Instance.PlaySound(clip);
    }
}
