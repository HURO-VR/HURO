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
    public Type type {get; private set;} 
    
    [SerializeField] private string triggerTag = "IndexFinger";

    private void Awake()
    {
        if (name.ToLower().Contains("up")) type = Type.Increment;
        if (name.ToLower().Contains("down")) type = Type.Decrement;
    }

    private void Enter()
    {
        OnTrigger?.Invoke();
        transform.position = new Vector3(transform.position.x, transform.position.y - pressDepth, transform.position.z);
    }

    private void Exit()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y + pressDepth, transform.position.z);
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