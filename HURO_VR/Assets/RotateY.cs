using UnityEngine;

/// <summary>
/// Rotates the object continuously around its Y axis.
/// </summary>
public class RotateY : MonoBehaviour
{
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 30f;

    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}