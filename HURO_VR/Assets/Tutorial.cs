using System;
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

    private bool completedTutorial;
    private bool setUp = false;
    public static int numSessionsCompleted = 0;
    private bool tenSecWarning = false;
    private void Update()
    {
        if (!completedTutorial && setUp && !firstLift.isStuck)
        {
            InstantiateMarkers();
            completedTutorial = true;
        }
        
        if (sessionTimer >= sessionLength)
        {
            sessionTimer = 0;
            tenSecWarning = false;
            SimulationManager.Instance.PauseAlgorithm();
            try
            {
                RunDataCollector.UploadLogData(numSessionsCompleted == 1);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            
            if (numSessionsCompleted == 1) audioLibrary.PlayAudio(AudioLibrary.AudioType.FirstRound_Complete); // First
            //if (numSessionsCompleted == 2) audioLibrary.PlayAudio(AudioLibrary.AudioType.SecondRound_Complete);
            if (numSessionsCompleted == 3)
            {
                ForkliftController.HideForklifts();
                audioLibrary.PlayAudio(AudioLibrary.AudioType.LastRound_Complete);
                return;
            }
            Time.timeScale = 1;
            SimulationParameterMark();
        }
        else if (RunDataCollector.isLogging)
        {
            sessionTimer += Time.deltaTime;
            if (sessionTimer >= 50 && !tenSecWarning)
            {
                AudioLibrary.instance.PlayAudio(AudioLibrary.AudioType.TenSecondWarning);
                tenSecWarning = true;
            }
        }
    }

    public void SetUp()
    {
        FaceCamera(firstLift.transform);
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
            
            var lift = ForkliftController.ActivateForklift();
            UserMarkerController.TryActivateMarker(UserMarkerController.MarkerType.Tutorial3);
            FaceCamera(lift.transform);
            UserMarkerController.OnMarkerHit += type =>
            {
                if (type == UserMarkerController.MarkerType.Tutorial3)
                {
                    UserMarkerController.TryActivateMarker(UserMarkerController.MarkerType.Tutorial4);
                    ForkliftController.ActivateForklift();
                    FaceCamera(firstLift.transform);
                }
                // Clipboard Mark
                else if (type == UserMarkerController.MarkerType.Tutorial4)
                {
                    SimulationParameterMark();
                }
                // Start Simulation
                else if (type == UserMarkerController.MarkerType.StartSimulation)
                {
                    FindAnyObjectByType<UserObstacle>().GetComponent<CapsuleCollider>().isTrigger = false;
                    ForkliftController.CanBreakDown = true;
                    SceneDataManager.Instance.InitSceneData();
                    SimulationManager.Instance.StartAlgorithm();
                    RunDataCollector.StartLogging();
                    Time.timeScale = 2;
                    var pokes = GameObject.FindObjectsByType<Poke>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (var poke in pokes)
                        poke.gameObject.SetActive(false);
                    clipboardController.gameObject.SetActive(false);
                    numSessionsCompleted++;
                }
            };
    }

    public void SimulationParameterMark()
    {
        var mark = UserMarkerController.TryActivateMarker(UserMarkerController.MarkerType.StartSimulation);
        if (numSessionsCompleted == 0) mark.SetAudioType(AudioLibrary.AudioType.FirstRound_Start);
        //if (numSessionsCompleted == 1) mark.SetAudioType(AudioLibrary.AudioType.SecondRound_Start);
        if (numSessionsCompleted == 2) mark.SetAudioType(AudioLibrary.AudioType.LastRound_Start);
        
        ForkliftController.HideForklifts();
        for (int i = 0; i < ClipboardController.numMachines; i++)
            ForkliftController.SpawnForklift();
        
        SceneDataManager.Instance.InitSceneData();
        SimulationManager.Instance.PauseAlgorithm();
        clipboardController.gameObject.SetActive(true);
        var pokes = GameObject.FindObjectsByType<Poke>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var poke in pokes)
            poke.gameObject.SetActive(true);
        if (numSessionsCompleted > 0)
        {
            float markY = mark.transform.position.y;
            Vector3 targetPos = Camera.main.transform.position + Camera.main.transform.forward * 0.7f;
            Vector3 markTargetPos = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            targetPos.y = Camera.main.transform.position.y - 0.1f;
            clipboardController.transform.position = targetPos;
            
            Vector3 direction = Camera.main.transform.position - clipboardController.transform.position;
            direction.y = 0; // Ignore vertical difference
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Vector3 eulerAngles = targetRotation.eulerAngles;
                eulerAngles.y += 90;
                clipboardController.transform.eulerAngles = eulerAngles;
            }

            markTargetPos.y = markY;
            mark.transform.position = markTargetPos;

        } else
        {
            clipboardController.transform.position = new Vector3(
                clipboardController.transform.position.x,
                Camera.main.transform.position.y - 0.1f,
                clipboardController.transform.position.z);
        }
        
        FaceCamera(clipboardController.transform);
        FindAnyObjectByType<UserObstacle>().GetComponent<CapsuleCollider>().isTrigger = true;
    }

    private void FaceCamera(Transform target)
    {
        var cam = FindAnyObjectByType<OVRCameraRig>();
        Vector3 direction = target.transform.position - cam.transform.position;
        direction.y = 0f; // Ignore vertical difference
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            cam.transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
        }
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
