using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{

    [Serializable]
    public struct StartPosition
    {
        public GameObject gameObject;
        public Vector3 position;
        public Vector3 rotation;
    }
    #region Serialized Variables
    [SerializeField] List<StartPosition> startPositions;

    #endregion

    #region Public Variables
    // Add public variables here
    #endregion

    #region Private Variables
    // Add private variables here
    #endregion

    #region Unity Methods



    private void Update()
    {
        // Per-frame logic
    }
    #endregion

    #region Public Methods
    public void PlaceObjects()
    {
        var room = GameObject.FindAnyObjectByType<SimRoom>();
        foreach (var obj in startPositions)
        {
            var instance = Instantiate(obj.gameObject);
            instance.transform.SetParent(room.transform);
            instance.transform.localPosition = obj.position;
            instance.transform.localRotation = Quaternion.Euler(obj.rotation);
        }
    }
    #endregion

    #region Private Methods
    // Add private methods here
    #endregion
}
