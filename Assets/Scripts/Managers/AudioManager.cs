using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Sound[] sounds;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.audioSource = gameObject.AddComponent<AudioSource>();
            s.audioSource.clip = s.clip;
            s.audioSource.volume = s.volume;
            s.audioSource.pitch = s.pitch;
            s.audioSource.loop = s.loop;
        }
    }

    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null)
        { return; }
        s.audioSource.DOFade(s.volume, 0.3f);
        s.audioSource.Play();
    }

    public void ChangePitch(float pitch, string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        { return; }
        s.audioSource.DOPitch(pitch, 2f);
    }

    public void FadeSound(float volumeTarget, string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        { return; }
        if(name == "pour")
        {
            s.audioSource.DOFade(volumeTarget, 0.4f).onComplete = StopPour;
        }
        else
        {
            s.audioSource.DOFade(volumeTarget, 0.4f);
        }

    }
    void StopPour()
    {
        Sound s = Array.Find(sounds, sound => sound.name == "pour");
        s.audioSource.Stop();
    }
    public void StopSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        { return; }
        s.audioSource.Stop();
    }
}
