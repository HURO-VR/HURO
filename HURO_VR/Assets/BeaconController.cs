using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BeaconController : MonoBehaviour
{
    #region Serialized Variables
    [FormerlySerializedAs("flashMaterial")]
    [FormerlySerializedAs("litMaterial")]
    [Header("Beacon Settings")]
    [Tooltip("Material used while beacons are flashing.")]
    [SerializeField] Material collisionMaterial;
    [SerializeField] Material stuckMaterial;
    

    [Tooltip("Flash interval in seconds.")]
    [SerializeField] float flashInterval;
    
    public bool IsFlashing => flashRoutine != null;
    #endregion

    #region Private Variables
    List<Renderer> beacons = new List<Renderer>();
    Material originalMaterial;
    Coroutine flashRoutine;
    bool isStuck = false;
    
    #endregion

    #region Unity Methods

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            foreach (Transform child2 in child)
            {
                if (child2.CompareTag("Beacon"))
                {
                    Renderer r = child2.GetComponent<Renderer>();
                    if (r != null)
                        beacons.Add(r);
                }
            }
        }

        if (beacons.Count > 0)
            originalMaterial = beacons[0].material;
        Debug.Log($"Beacon Controller {beacons.Count} beacon found.");
    }

    #endregion

    #region Public Methods

    public void FlashBeacons()
    {
        if (flashRoutine == null)
            flashRoutine = StartCoroutine(FlashRoutine());
    }

    public void SetStuck(bool stuck)
    {
        this.isStuck = stuck;
    }

    public void DisableBeacons()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        foreach (var beacon in beacons)
            beacon.material = originalMaterial;
    }

    #endregion

    #region Private Methods

    private IEnumerator FlashRoutine()
    {
        bool isLit = false;
        while (true)
        {
            foreach (var beacon in beacons)
                beacon.material = isLit ? originalMaterial : isStuck ? stuckMaterial : collisionMaterial;

            isLit = !isLit;
            yield return new WaitForSeconds(0.5f);
        }
    }

    #endregion
}