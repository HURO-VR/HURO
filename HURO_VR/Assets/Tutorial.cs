using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Tutorial : MonoBehaviour
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
    private int sessionLength = 60;
    private float sessionTimer = 0;
    private ForkliftController firstLift;
    public static Tutorial instance;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        instance = this;
        collider = GetComponent<BoxCollider>();
        audioLibrary = GameObject.Find("AudioLibrary").GetComponent<AudioLibrary>();
        clipboardController =
            FindObjectsByType<ClipboardController>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0];
        firstLift = FindObjectsByType<ForkliftController>(FindObjectsSortMode.None)[0];
    }

    private void Start()
    {
        robotNearbyDistance *= SimulationManager.sceneScale.transform.localScale.z;
    }

    private bool completed;
    private bool simCompleted;
    private bool setUp = false;

    private void Update()
    {
        if (!completed && setUp && !firstLift.isStuck)
        {
            InstantiateMarkers();
            completed = true;
        }
        
        if (sessionTimer >= sessionLength && !simCompleted)
        {
            simCompleted = true;
            RunDataCollector.UploadLogData();
            audioLibrary.PlayAudio(AudioLibrary.AudioType.SimFinsh);
        }
        else if (RunDataCollector.isLogging) sessionTimer += Time.deltaTime;
    }

    public void SetUp()
    {
        Invoke("BreakDownRobot", 7f);
    }

    public void BreakDownRobot()
    {
        firstLift.BreakDownForklift();
        setUp = true;
    }
    #endregion

    #region Public Methods
    // Add public methods here
    #endregion

    #region Private Methods

    private void InstantiateMarkers()
    {
        if (audioLibrary)
                audioLibrary.PlayAudio(AudioLibrary.AudioType.Tutorial2);
            
            ForkliftController.ActivateForklift();
            var mark = Instantiate(markerController.gameObject);
            mark.transform.localScale *= SimulationManager.sceneScale.transform.localScale.z;
            mark.transform.position = new Vector3(-5.32000017f, 0.02f, -0.660000026f);
            var control = mark.GetComponent<UserMarkerController>();
            control.SetMarkerType(UserMarkerController.MarkerType.Tutorial3);
            UserMarkerController.OnMarkerHit += type =>
            {
                // Barrel Mark
                if (type == UserMarkerController.MarkerType.Tutorial3)
                {
                    var mark = Instantiate(markerController.gameObject);
                    mark.transform.position = new Vector3(3.06999993f,1.00484836f,-1.11000001f);
                    mark.transform.localScale *= SimulationManager.sceneScale.transform.localScale.z;
                    mark.GetComponent<UserMarkerController>().SetMarkerType(UserMarkerController.MarkerType.Tutorial4);
                    ForkliftController.ActivateForklift();
                }
                // Clipboard Mark
                else if (type == UserMarkerController.MarkerType.Tutorial4)
                {
                    var mark = Instantiate(markerController.gameObject);
                    mark.transform.localScale *= SimulationManager.sceneScale.transform.localScale.z;
                    mark.transform.position = new Vector3(7.0f, 0.02f, -4.5f);
                    mark.GetComponent<UserMarkerController>().SetMarkerType(UserMarkerController.MarkerType.StartSimulation);
                    ForkliftController.HideForklifts();
                    ForkliftController.SpawnForklift();
                    SceneDataManager.Instance.InitSceneData();
                    SimulationManager.Instance.PauseAlgorithm();
                    clipboardController.gameObject.SetActive(true);
                    var pokes = GameObject.FindObjectsByType<Poke>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (var poke in pokes)
                        poke.gameObject.SetActive(true);
                    clipboardController.transform.position = new Vector3(clipboardController.transform.position.x, Camera.main.transform.position.y - 0.4f, clipboardController.transform.position.z);
                    var cam = FindAnyObjectByType<OVRCameraRig>();
                    cam.transform.LookAt(clipboardController.transform);
                }
                // Start Simulation
                else if (type == UserMarkerController.MarkerType.StartSimulation)
                {
                    ForkliftController.CanBreakDown = true;
                    ForkliftController.DestroyInitialForklifts();
                    SceneDataManager.Instance.InitSceneData();
                    SimulationManager.Instance.StartAlgorithm();
                    RunDataCollector.StartLogging();
                    var pokes = GameObject.FindObjectsByType<Poke>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (var poke in pokes)
                        poke.gameObject.SetActive(false);
                    clipboardController.gameObject.SetActive(false);
                }
            };
    }
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
