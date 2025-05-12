using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof (AudioSource))]
[RequireComponent (typeof (BoxCollider))] 
public class InfoButton : MonoBehaviour
{
    
    
    #region Serialized Variables
    public static List<InfoButton> infoButtons = new List<InfoButton>();
    [SerializeField] private AudioSource info;

    #endregion

    #region Public Variables
    // Add public variables here
    #endregion

    #region Private Variables
    private float pressDepth = 0.0048f;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        infoButtons.Add(this);
    }

    private void Start()
    {
        // Initialization code
    }

    private void Update()
    {
        // Per-frame logic
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("IndexFinger"))
        {
            Enter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("IndexFinger"))
        {
            Exit();
        }
    }

    private void OnMouseDown()
    {
        Enter();
    }

    private void OnMouseUp()
    {
        Exit();
    }

    #endregion

    #region Public Methods

    public void Stop()
    {
        info.Stop();
    }

    public static void StopAll()
    {
        foreach(InfoButton infoButton in infoButtons)
            infoButton.Stop();
    }
    #endregion

    #region Private Methods

    private void Enter()
    {
        if (info.isPlaying) return;
        InfoButton.StopAll();
        info.Play();
        transform.position = new Vector3(transform.position.x, transform.position.y - pressDepth, transform.position.z);
    }

    private void Exit()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y + pressDepth, transform.position.z);
    }
    #endregion
}
