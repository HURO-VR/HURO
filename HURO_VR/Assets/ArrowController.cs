using System;
using UnityEngine;

/// <summary>
/// Calls the assigned event when triggered by a specific tag (e.g., "IndexFinger").
/// </summary>
public class ArrowController : MonoBehaviour
{
    public enum Type
    {
        Increment,
        Decrement,
    }
    public event Action OnTrigger;
    private float pressDepth = 0.0048f;
    private bool canPress = true;
    public Type type {get; private set;} 
    
    [SerializeField] private string triggerTag = "IndexFinger";

    private void Awake()
    {
        if (name.ToLower().Contains("up")) type = Type.Increment;
        if (name.ToLower().Contains("down")) type = Type.Decrement;
    }

    private void CanPress()
    {
        canPress = true;
    }

    private void Enter()
    {
        if (canPress == false) return;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - pressDepth, transform.localPosition.z);
        canPress = false;
        Invoke("CanPress", 0.2f);
        try
        {
            OnTrigger?.Invoke();
        }
        catch
        {
            
        }
    }

    private void Exit()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + pressDepth, transform.localPosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag)) Enter();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(triggerTag)) Exit();
    }

    private void OnMouseDown()
    {
        Enter();
    }

    private void OnMouseUp()
    {
        Exit();
    }

    public void SetType(Type type)
    {
        this.type = type;
    }
}