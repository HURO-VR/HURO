using UnityEngine;

/// <summary>
/// Scales physical head movement to create amplified virtual movement.
/// Attach this to the parent object of your OVRCameraRig.
/// </summary>
public class ScaledWalking : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Movement multiplier. Physical movement is amplified by this factor.")]
    private float movementScale;
    
    [SerializeField]
    [Tooltip("Reference to the camera/head transform")]
    private Transform headTransform;
    
    [SerializeField]
    [Tooltip("Only track horizontal movement (XZ plane)")]
    private bool horizontalOnly = true;
    
    [SerializeField]
    [Tooltip("Minimum physical movement threshold (in meters) before scaling is applied")]
    private float movementThreshold = 0.01f;
    
    [SerializeField]
    [Tooltip("Maximum speed cap (in meters per second) for scaled movement")]
    private float maxSpeed = 5.0f;
    
    // Store previous position for calculating movement delta
    private Vector3 previousHeadPosition;
    
    // For smoothing motion
    private Vector3 smoothedMovement = Vector3.zero;
    
    [SerializeField]
    [Tooltip("Smoothing factor for movement (0 = no smoothing, higher = more smoothing)")]
    private float smoothingFactor = 0.8f;

    private void Start()
    {
        // If no head transform is specified, try to find it automatically
        if (headTransform == null)
        {
            OVRCameraRig cameraRig = GetComponentInChildren<OVRCameraRig>();
            if (cameraRig != null)
            {
                headTransform = cameraRig.centerEyeAnchor;
                Debug.Log("Found head transform automatically: " + headTransform.name);
            }
            else
            {
                Debug.LogError("No head transform assigned and couldn't find OVRCameraRig. Please assign the head transform manually.");
                enabled = false;
                return;
            }
        }
        
        // Initialize previous position
        previousHeadPosition = headTransform.position;
    }

    private void Update()
    {
        ApplyScaledMovement();
    }
    
    private void ApplyScaledMovement()
    {
        // Calculate movement delta in world space
        Vector3 currentHeadPosition = headTransform.position;
        Vector3 movementDelta = currentHeadPosition - previousHeadPosition;
        
        // Apply horizontal-only constraint if enabled
        if (horizontalOnly)
        {
            movementDelta.y = 0;
        }
        
        // Only apply scaled movement if above threshold
        if (movementDelta.magnitude > movementThreshold)
        {
            // Calculate the movement vector (scaled by movementScale)
            Vector3 scaledMovement = movementDelta * (movementScale - 1); // Subtract 1 because the head already moves naturally
            
            // Apply smoothing if required
            if (smoothingFactor > 0)
            {
                smoothedMovement = Vector3.Lerp(smoothedMovement, scaledMovement, 1 - smoothingFactor);
                scaledMovement = smoothedMovement;
            }
            
            // Apply speed cap if necessary
            float speedCap = maxSpeed * Time.deltaTime;
            if (scaledMovement.magnitude > speedCap)
            {
                scaledMovement = scaledMovement.normalized * speedCap;
            }
            
            // Apply the scaled movement to this transform (the parent of the camera)
            transform.position += scaledMovement;
        }
        else
        {
            // Reset smoothed movement when below threshold to prevent drift
            smoothedMovement = Vector3.zero;
        }
        
        // Update previous position for next frame
        previousHeadPosition = currentHeadPosition;
    }
}