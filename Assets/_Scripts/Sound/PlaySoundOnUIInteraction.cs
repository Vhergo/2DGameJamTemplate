using UnityEngine;

public class PlaySoundOnUIInteraction : MonoBehaviour
{
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip releaseSound;
    [SerializeField] private AudioClip exitSound;

    
    public void PlayOnSelect() {
        SoundManager.Instance.PlaySound(selectSound);
    }

    public void PlayOnHover() {
        SoundManager.Instance.PlaySound(hoverSound);
    }

    public void PlayOnRelease() {
        SoundManager.Instance.PlaySound(releaseSound);
    }

    public void PlayOnExit() {
        SoundManager.Instance.PlaySound(hoverSound);
    }
}
