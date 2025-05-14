using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class NoStuckZone : MonoBehaviour
{
    
    
    #region Serialized Variables
    // Add any [SerializeField] variables here if needed
    // Add Headers: [Header("Logging")]
    // Add Tips: [Tooltip("Runs the algorithm every X seconds.")]

    #endregion

    #region Public Variables
    // Add public variables here
    #endregion

    #region Private Variables
    // Add private variables here
    #endregion

    #region Unity Methods
    private void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Update()
    {
        // Per-frame logic
    }
    private void OnTriggerEnter(Collider other)
    {
        var lift = other.GetComponent<ForkliftController>();
        if (lift != null && other.isTrigger == false)
        {
            lift.NoStuckZoneEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var lift = other.GetComponent<ForkliftController>();
        if (lift != null && other.isTrigger == false)
        {
            lift.NoStuckZoneExit();
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
