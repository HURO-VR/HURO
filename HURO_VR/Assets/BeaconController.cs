using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BeaconController : MonoBehaviour
{
    #region Serialized Variables
    [FormerlySerializedAs("litMaterial")]
    [Header("Beacon Settings")]
    [Tooltip("Material used while beacons are flashing.")]
    [SerializeField] Material flashMaterial;

    [Tooltip("Flash interval in seconds.")]
    [SerializeField] float flashInterval;
    
    public bool IsFlashing => flashRoutine != null;
    #endregion

    #region Private Variables
    List<Renderer> beacons = new List<Renderer>();
    Material originalMaterial;
    Coroutine flashRoutine;
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
                beacon.material = isLit ? originalMaterial : flashMaterial;

            isLit = !isLit;
            yield return new WaitForSeconds(flashInterval);
        }
    }

    #endregion
}