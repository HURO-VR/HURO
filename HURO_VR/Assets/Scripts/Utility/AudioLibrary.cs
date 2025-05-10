using System.Collections.Generic;
using UnityEngine;

public class AudioLibrary : MonoBehaviour
{

    public enum AudioType
    {
        // User Feedback
        Tutorial1,
        Tutorial2,
        Tutorial3,
        Tutorial4,
        Tutorial5,
    }

    //public static AudioLibrary Instance;


    [System.Serializable]
    public struct AudioClipEntry
    {
        public AudioType type;
        public AudioClip clip;
        public float volume;
    }

    public List<AudioClipEntry> audioClips;
    private Dictionary<AudioType, AudioClipEntry> audioClipDictionary;

    private AudioSource audioSource;
    public List<AudioType> audioOnStart = new List<AudioType>();

    private void Awake()
    {
        //if (Instance == null)
        //{
        //    Instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // Initialize the dictionary
        audioClipDictionary = new Dictionary<AudioType, AudioClipEntry>();
        foreach (var entry in audioClips)
        {
            audioClipDictionary[entry.type] = entry;
        }
    }

    public void Start()
    {
        foreach (var start in audioOnStart) PlayAudio(start);
    }

    public void PlayAudio(AudioType type)
    {
        if (audioClipDictionary.TryGetValue(type, out AudioClipEntry clip))
        {
            audioSource.PlayOneShot(clip.clip, clip.volume);
        }
        else
        {
            Debug.LogWarning($"Audio type {type} not found!");
        }
    }

    public void Stop()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }
}
