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


        if (Ytimer > 3f)
        {
            
        }
    }
    #endregion

    #region Public Methods
    // Add public methods here
    #endregion

    #region Private Methods
    // Add private methods here
    #endregion
}
