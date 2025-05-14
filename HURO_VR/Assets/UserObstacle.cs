using UnityEngine;

public class UserObstacle : MonoBehaviour
{
    
    Transform cameraOffset;
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
    private void Awake()
    {
        cameraOffset = transform.parent;
    }

    private void Update()
    {
        return;
        Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        // Early out if there's no input
        if (input.sqrMagnitude < 0.01f)
            return;

        // Calculate direction relative to camera's forward, ignoring vertical
        Vector3 forward = cameraOffset.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = cameraOffset.transform.right;
        right.y = 0;
        right.Normalize();

        Vector3 moveDirection = forward * input.y + right * input.x;
        cameraOffset.position += moveDirection * 1f * Time.deltaTime;
    }
    #endregion

    #region Public Methods
    // Add public methods here
    #endregion

    #region Private Methods
    // Add private methods here
    #endregion
}
