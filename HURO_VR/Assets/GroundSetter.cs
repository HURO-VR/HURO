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
    
    private void Update()
    {
        if (OVRInput.Get(OVRInput.RawButton.X))
        {
            timer += Time.deltaTime;
        }
        else timer = 0;

        if (timer > 3f)
        {
            timer = 0;
            var anchor = GameObject.Find("LeftHandAnchor");
            transform.position = new Vector3(transform.position.x, anchor.transform.position.y, transform.position.z);
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
