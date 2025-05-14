using System.Collections.Generic;
using System.Linq;
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
        BackgroundNoise,
        SimulationFinish,
        StartSimulation,
        Beep,
        None
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
    private AudioSource backgroundAudioSource;
    public List<AudioType> audioOnStart = new List<AudioType>();
    public static AudioLibrary instance;
    private void Awake()
    {
        if (instance == null) instance = this;
        //else
        //{
        //    Destroy(gameObject);
        //}
        var sources = GetComponents<AudioSource>().ToList();
        if (sources.Count < 3)
            sources.Add(gameObject.AddComponent<AudioSource>());
        if (sources.Count < 3)
            sources.Add(gameObject.AddComponent<AudioSource>());
        
        audioSource = sources[0];
        backgroundAudioSource = sources[1];
        // Initialize the dictionary
        audioClipDictionary = new Dictionary<AudioType, AudioClipEntry>();
        foreach (var entry in audioClips)
        {
            audioClipDictionary[entry.type] = entry;
        }
    }

    public void Start()
    {
        foreach (var start in audioOnStart) PlayAudio(start, true);
    }

    public void PlayAudio(AudioType type, bool background = false)
    {
        if (audioClipDictionary.TryGetValue(type, out AudioClipEntry clip))
        {
            if (!background) audioSource.Stop();
            if (!background) audioSource.PlayOneShot(clip.clip, clip.volume);
            else backgroundAudioSource.PlayOneShot(clip.clip, clip.volume);
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
