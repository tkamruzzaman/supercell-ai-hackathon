using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
[SerializeField] AudioSource sfxSource;

[SerializeField] AudioClip followerCollectionClip;
[SerializeField] AudioClip followerPickUpClip;
[SerializeField] AudioClip followerDropOffClip;
[SerializeField] AudioClip followerFightClip;
[SerializeField] AudioClip zoneCaptureDoneClip;
[SerializeField] AudioClip zoneCaptureFailedClip;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySfx(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
