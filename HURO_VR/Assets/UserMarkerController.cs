using System;
using System.Collections.Generic;
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
    static List<UserMarkerController> markers = new List<UserMarkerController>();
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
        markers.Add(this);
        var scene = GameObject.Find("Scene");
        Debug.Log($"Added marker. Now: {markers.Count}");
        if (markerType != MarkerType.Tutorial1) gameObject.SetActive(false);
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
        if (user != null)
        if (Utility.Utils.IsInXZBox(user.transform, transform.position, markerBoxSize))
        {
            if (audioType != null)
                audioLibrary.PlayAudio(audioType);
            if (markerType != null) 
                OnMarkerHit?.Invoke(markerType);
            gameObject.SetActive(false);
        }
    }

    public static UserMarkerController TryActivateMarker(MarkerType markerType)
    {
        foreach  (var marker in markers)
            if (markerType == marker.markerType)
            {
                marker.gameObject.SetActive(true);
                return marker;
            }
        return null;
    }
    #endregion

    #region Public Methods
    // Add public methods here
    #endregion

    #region Private Methods
    // Add private methods here
    #endregion
}
