using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BarrelTutorial : MonoBehaviour
{
    
    
    #region Serialized Variables
    // Add any [SerializeField] variables here if needed
    // Add Headers: [Header("Logging")]
    // Add Tips: [Tooltip("Runs the algorithm every X seconds.")]
    ClipboardController clipboardController;
    #endregion

    #region Public Variables
    public UserMarkerController markerController;
    private BoxCollider collider;
    #endregion

    #region Private Variables
    AudioLibrary audioLibrary;
    
    #endregion

    #region Unity Methods
    private void Awake()
    {
        collider = GetComponent<BoxCollider>();
        audioLibrary = GameObject.Find("AudioLibrary").GetComponent<AudioLibrary>();
        clipboardController =
            FindObjectsByType<ClipboardController>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0];
    }

    private bool completed;

    private void Update()
    {
        if (!completed && IsRobotNearby())
        {
            if (audioLibrary)
                audioLibrary.PlayAudio(AudioLibrary.AudioType.Tutorial2);
            
            ForkliftController.ActivateLift();
            var mark = Instantiate(markerController.gameObject);
            mark.transform.position = new Vector3(5.32000017f, 0.02f, -0.660000026f);
            var control = mark.GetComponent<UserMarkerController>();
            control.SetMarkerType(UserMarkerController.MarkerType.Tutorial3);
            UserMarkerController.OnMarkerHit += type =>
            {
                // Barrel Mark
                if (type == UserMarkerController.MarkerType.Tutorial3)
                {
                    var mark = Instantiate(markerController.gameObject);
                    mark.transform.position = new Vector3(7.30000019f, 0.02f, -3.25f);
                    mark.GetComponent<UserMarkerController>().SetMarkerType(UserMarkerController.MarkerType.Tutorial4);
                    clipboardController.gameObject.SetActive(true);
                    ForkliftController.ActivateLift();
                }
                // Clipboard Mark
                else if (type == UserMarkerController.MarkerType.Tutorial4)
                {
                    var mark = Instantiate(markerController.gameObject);
                    mark.transform.position = new Vector3(6.30000019f, 0.02f, -3.25f);
                    mark.GetComponent<UserMarkerController>().SetMarkerType(UserMarkerController.MarkerType.StartSimulation);
                }
                // Start Simulation
                else if (type == UserMarkerController.MarkerType.StartSimulation)
                {
                    SimulationManager.Instance.ToggleAlgorithm();
                    clipboardController.gameObject.SetActive(false);
                }
            };
            completed = true;
        }
    }
    #endregion

    #region Public Methods
    // Add public methods here
    #endregion

    #region Private Methods
    public float robotNearbyDistance;
    /// <summary>
    /// Checks if any GameObject with tag "Robot" is within robotNearbyDistance meters of this object's SphereCollider.
    /// </summary>
    /// <returns>True if at least one Robot is nearby; otherwise, false.</returns>
    private bool IsRobotNearby()
    {
        if (collider == null)
        {
            Debug.LogWarning("SphereCollider not found.");
            return false;
        }

        Vector3 center = collider.transform.TransformPoint(collider.center);

        Collider[] hits = Physics.OverlapSphere(center, robotNearbyDistance);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject != this.gameObject && hit.CompareTag("Robot"))
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}
