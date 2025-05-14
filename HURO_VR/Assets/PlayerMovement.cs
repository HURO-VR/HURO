using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in meters per second")]
    public float moveSpeed = 2.0f;
    
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 60.0f;

    [Tooltip("Optional: Camera rig reference. If null, will try to find OVRCameraRig in scene")]
    public Transform cameraRig;

    [Tooltip("Whether to use snap turning instead of smooth turning")]
    public bool useSnapTurning = false;
    
    [Tooltip("Angle in degrees for snap turning")]
    public float snapTurnAngle = 30.0f;
    
    [Tooltip("Threshold for snap turning activation")]
    public float snapTurnThreshold = 0.6f;

    private Transform cameraTransform;
    private Transform playerTransform;
    private CharacterController characterController;
    private bool snapTurnReady = true;

    private void Awake()
    {
        // Get the character controller component
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("Character Controller component is missing from this GameObject");
            enabled = false;
            return;
        }

        playerTransform = transform;

        // Find camera reference if not set
        if (cameraRig == null)
        {
            cameraRig = transform.parent;
        }

        // Get the center eye anchor as the camera reference
        cameraTransform = cameraRig.GetComponentInChildren<OVRCameraRig>()?.centerEyeAnchor;
        if (cameraTransform == null)
        {
            Debug.LogError("Could not find center eye anchor. Make sure OVRCameraRig is properly set up.");
            enabled = false;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }
    
    private void HandleMovement()
    {
        if (Mathf.Abs(transform.position.x) > 10f || Mathf.Abs(transform.position.z) < 10f) return;
        // Get RIGHT thumbstick input for movement
        Vector2 movementInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);

        // Skip if no input
        if (movementInput.magnitude < 0.1f)
            return;

        // Convert thumbstick input to movement vector
        Vector3 moveDirection = new Vector3(movementInput.x, 0, movementInput.y);

        // Get camera forward and right vectors (ignore vertical component)
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        // Calculate move direction relative to camera orientation
        Vector3 movement = cameraRight * moveDirection.x + cameraForward * moveDirection.z;
        
        // Apply movement
        characterController.Move(movement * moveSpeed * Time.deltaTime);
    }
    
    private void HandleRotation()
    {
        // Get LEFT thumbstick input for rotation
        Vector2 rotationInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
        
        // Only use horizontal input for rotation
        float horizontalInput = rotationInput.x;
        
        // Skip if no significant input
        if (Mathf.Abs(horizontalInput) < 0.1f)
        {
            snapTurnReady = true;
            return;
        }
        
        if (useSnapTurning)
        {
            // Snap turning logic
            if (Mathf.Abs(horizontalInput) >= snapTurnThreshold && snapTurnReady)
            {
                float rotationAmount = snapTurnAngle * Mathf.Sign(horizontalInput);
                playerTransform.Rotate(0, rotationAmount, 0);
                snapTurnReady = false;
            }
            else if (Mathf.Abs(horizontalInput) < snapTurnThreshold * 0.5f)
            {
                // Reset snap turn when thumbstick returns to near-center
                snapTurnReady = true;
            }
        }
        else
        {
            // Smooth turning logic
            float rotationAmount = horizontalInput * rotationSpeed * Time.deltaTime;
            playerTransform.Rotate(0, rotationAmount, 0);
        }
    }
}