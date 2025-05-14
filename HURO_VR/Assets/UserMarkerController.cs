using System;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class UserMarkerController : MonoBehaviour
{

    public enum MarkerType
    {
        Tutorial1,
        Tutorial2,
        Tutorial3,
        Tutorial4,
        Tutorial5,
        StartSimulation,
        None
    }
    
    
    #region Serialized Variables
    // Add any [SerializeField] variables here if needed
    // Add Headers: [Header("Logging")]
    // Add Tips: [Tooltip("Runs the algorithm every X seconds.")]

    #endregion

    #region Public Variables

    public static event Action<MarkerType> OnMarkerHit;
    [FormerlySerializedAs("markerBox")] public float markerBoxSize;
    [SerializeField] private MarkerType markerType;
    #endregion

    #region Private Variables

    private UserObstacle user;
    private AudioLibrary audioLibrary;
    [SerializeField]
    private AudioLibrary.AudioType audioType = AudioLibrary.AudioType.None;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        user = FindAnyObjectByType<UserObstacle>();
        if (audioLibrary == null)
        {
            audioLibrary = GameObject.Find("AudioLibrary").GetComponent<AudioLibrary>();
        }

        var scene = GameObject.Find("Scene");
        markerBoxSize *= scene.transform.localScale.z;
    }

    public void SetMarkerType(MarkerType markerType)
    {
        this.markerType = markerType;
        audioType = AudioLibrary.AudioType.None;
        foreach (var type in Enum.GetValues(typeof(AudioLibrary.AudioType)))
            if (markerType.ToString().Equals(type.ToString()))
                audioType = (AudioLibrary.AudioType)type;
    }

    private void Update()
    {
        if (Utility.Utils.IsInXZBox(user.transform, transform.position, markerBoxSize))
        {
            if (audioType != null)
                audioLibrary.PlayAudio(audioType);
            if (markerType != null) 
                OnMarkerHit?.Invoke(markerType);
            gameObject.SetActive(false);
        }
    }
    #endregion

    #region Public Methods
    // Add public methods here
    #endregion

    #region Private Methods
    // Add private methods here
    #endregion
}
