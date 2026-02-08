using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    [SerializeField] AudioSource sfxSource;
    [Space] [SerializeField] AudioClip uiPressClip;
    [Space] [SerializeField] AudioClip followerCollectionClip;
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

    public void PlayFollowerCollectionClip()
    {
        PlaySfx(followerCollectionClip);
    }

    public void PlayFollowerPickUpClip()
    {
        PlaySfx(followerPickUpClip);
    }

    public void PlayFollowerDropOffClip()
    {
        PlaySfx(followerDropOffClip);
    }

    public void PlayFollowerFightClip()
    {
        PlaySfx(followerFightClip);
    }

    public void PlayZoneCaptureDoneClip()
    {
        PlaySfx(zoneCaptureDoneClip);
    }

    public void PlayZoneCaptureFailedClip()
    {
        PlaySfx(zoneCaptureFailedClip);
    }

    public void PlayUIPressClip()
    {
        PlaySfx(uiPressClip);
    }

    private void PlaySfx(AudioClip clip)
    {
        if(clip != null)
            sfxSource.PlayOneShot(clip);
    }
}
