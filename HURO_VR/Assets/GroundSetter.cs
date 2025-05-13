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

    private float timer;
    #endregion

    #region Unity Methods
    private void Start()
    {
        // Initialization code
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
            var lateral = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x * 0.3f;
            if (lateral < 0) transform.localScale *= (1 - lateral);
            else transform.localScale *= (1 + lateral);
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
