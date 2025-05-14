using UnityEngine;

public class GroundSetter : MonoBehaviour
{
    
    
    #region Serialized Variables
    // Add any [SerializeField] variables here if needed
    // Add Headers: [Header("Logging")]
    // Add Tips: [Tooltip("Runs the algorithm every X seconds.")]

    #endregion

    #region Public Variables
    // Add public variables here
    #endregion

    #region Private Variables

    private Renderer floor;
    private Material floorMaterial;
    private float timer;
    private GameObject cameraOffset;
    #endregion

    #region Unity Methods
    private void Start()
    {
        floor = GameObject.Find("Floor").GetComponent<Renderer>();
        floorMaterial = floor.GetComponent<Renderer>().material;
        cameraOffset = GameObject.Find("CameraOffset");
    }

    private float Ytimer = 0;
    private void Update()
    {
        if (OVRInput.Get(OVRInput.RawButton.X))
        {
            timer += Time.deltaTime;
        } else timer = 0;

        if (OVRInput.Get(OVRInput.RawButton.Y)) Ytimer += Time.deltaTime;
        else Ytimer = 0;

        if (timer > 3f)
        {
            timer = 0;
            var anchor = GameObject.Find("LeftHandAnchor");
            transform.position = new Vector3(anchor.transform.position.x, anchor.transform.position.y, anchor.transform.position.z);
        }

        if (Ytimer > 3f)
        {
            floor.material.color = Color.red;
            float input = Mathf.Max(
                Mathf.Abs(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x),
                Mathf.Abs(OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x)
            );

            float direction = Mathf.Sign(
                OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x +
                OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x
            );

            float scaleSpeed = 0.5f; // Adjust for faster/slower scaling
            float scaleFactor = 1 + direction * input * scaleSpeed * Time.deltaTime;

            cameraOffset.transform.position = new Vector3(cameraOffset.transform.position.x, cameraOffset.transform.position.y * scaleFactor, cameraOffset.transform.position.z);

            // Optional: Clamp the scale to prevent going too small or too large
            float minScale = 0.05f;
            float maxScale = 1f;
            //transform.localScale = Vector3.Max(Vector3.one * minScale, Vector3.Min(transform.localScale, Vector3.one * maxScale));
            //AudioLibrary.instance.PlayAudio(AudioLibrary.AudioType.Beep);
        } else floor.material.color = Color.gray;
    }
    #endregion

    #region Public Methods
    // Add public methods here
    #endregion

    #region Private Methods
    // Add private methods here
    #endregion
}
