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
    private AudioLibrary.AudioType audioType;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        user = FindAnyObjectByType<UserObstacle>();
        if (audioLibrary == null)
        {
            audioLibrary = GameObject.Find("AudioLibrary").GetComponent<AudioLibrary>();
        }
    }

    public void SetMarkerType(MarkerType markerType)
    {
        this.markerType = markerType;
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
