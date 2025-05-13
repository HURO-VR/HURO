using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxCollider))]
public class ForkliftController : RobotEntity
{
    
    
    #region Serialized Variables
    // Add any [SerializeField] variables here if needed
    // Add Headers: [Header("Logging")]
    // Add Tips: [Tooltip("Runs the algorithm every X seconds.")]
    #endregion

    #region Public Variables
    public bool PalletIsDown => lift.transform.position.y <= palletDownLimit;
    public bool PalletIsUp => lift.transform.position.y >= palletDownLimit + upAmount;
    public static bool CanBreakDown = false;
    #endregion

    #region Private Variables

    private GameObject lift;
    private bool movePalletUp = false;
    private bool movePalletDown => !movePalletUp;
    private static float palletDownLimit = -100;
    private float palletUpLimit => palletDownLimit + upAmount;
    private float upAmount = 0.94f;
    protected GameObject currGoal;
    List<Vector3> goalPositions;
    [SerializeField] List<Transform> entryPoints;
    private int goalIndex = 0;
    private Rigidbody body;
    private BeaconController beaconController;
    GameObject emptyPallet;
    private int emptyPalletIndex = 0;
    private int palletIndex = -1;
    private Vector3 originalRotation;
    private Vector3 originalPosition;
    private BoxCollider boxCollider;
    private SphereCollider sphereCollider;
    public bool isStuck { get; private set; }
    
    #endregion

    #region Unity Methods

    private void Awake()
    {
        base.Awake();
        boxCollider = GetComponent<BoxCollider>();
        sphereCollider = GetComponent<SphereCollider>();
        foreach (Transform child in transform)
            if (child.name == "Lift")
                lift = child.gameObject;
        if (palletDownLimit == -100) palletDownLimit = lift.transform.position.y;
        base.OnGoalReached += () =>
        {
            if (PalletIsUp) LowerPallet();
            else if (PalletIsDown) LiftPallet();
            RotateGoal();
        };
        goalPositions = new List<Vector3>();
        foreach (Transform child in entryPoints)
            goalPositions.Add(child.position);
        base.body.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezePositionY;
        RotateGoal();

        body = gameObject.GetComponent<Rigidbody>();
        for (int i = 0; i < lift.transform.childCount; i++)
            if (lift.transform.GetChild(i).gameObject.activeSelf)
            {
                emptyPallet = lift.transform.GetChild(i).gameObject;
                emptyPalletIndex = i;
                break;
            }
        originalRotation = body.transform.rotation.eulerAngles;
        originalPosition = body.transform.position;
        palletIndex = emptyPalletIndex + 1;
        palletIndex %= lift.transform.childCount;
        beaconController = gameObject.GetComponentInChildren<BeaconController>();
        if (_initialForkliftControllers == null) _initialForkliftControllers = FindObjectsByType<ForkliftController>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
    }
    
    

    void RotateToVelocity()
    {
        Vector3 horizontalVelocity = new Vector3(body.velocity.x, 0, body.velocity.z);
        if (horizontalVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity);
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, targetRotation.eulerAngles.y + base.initialRotation.y, transform.eulerAngles.z);
        }
    }

    private void Start()
    {
        base.Start();
    }

    private void RotateGoal(bool reset = true)
    {
        Destroy(currGoal);
        currGoal = new GameObject();
        currGoal.name = name + "_Goal";
        var entity = currGoal.AddComponent<GoalEntity>();
        
        currGoal.tag = "Goal";
        currGoal.transform.position = goalPositions[goalIndex];
        goalIndex++;
        if (goalIndex >= goalPositions.Count) goalIndex = 0;
        if (reset) base.ResetGoal(entity);
    }

    private float rollDice = 0;
    private bool timeout = false;
    private void Update()
    {
        Random.InitState(Mathf.FloorToInt(Time.time));
        ManagePalletMovement();
        RotateToVelocity();
        ManageBeacons();
        
        if (transform.eulerAngles.x != originalRotation.x || transform.eulerAngles.z != originalRotation.z)
            transform.eulerAngles = new Vector3(originalRotation.x, transform.eulerAngles.y, originalRotation.z);
        if (transform.position.y != originalPosition.y)
            transform.position = new Vector3(transform.position.x, originalPosition.y, transform.position.z);
        if (CanBreakDown && !timeout && rollDice > 1f)
        {
            var ran = Random.Range(0, 20);
            if (ran == 1)
            {
                isStuck = true;
                body.isKinematic = true;
                beaconController.SetStuck(isStuck);
                beaconController.FlashBeacons();
            }
            rollDice = 0;
        } else if (CanBreakDown && !isStuck && SimulationManager.Instance.IsRunning()) rollDice += Time.deltaTime;
        if (rollDice > 10f) timeout = false;
        base.Update();
    }
    #endregion

    #region Public Methods
    public void LiftPallet()
    {
        movePalletUp = true;
        FillPallet();
    }

    private static ForkliftController[] _initialForkliftControllers;

    public static void ActivateForklift()
    {
        GameObject spawn = null;
        foreach (var lift in _initialForkliftControllers)
            if (lift.gameObject.activeSelf == false)
                spawn = lift.gameObject;
        if (spawn) spawn.SetActive(true);
    }

    public static void HideForklifts()
    {
        foreach (var forkliftController in _initialForkliftControllers)
        {
            forkliftController.gameObject.SetActive(false);
            forkliftController.transform.position = new Vector3(30, forkliftController.transform.position.y, 30);
        }
    }

    public static List<ForkliftController> SpawnedForklifts = new List<ForkliftController>();

    public static void SpawnForklift()
    {
        var scene = GameObject.Find("Scene");
        var floor = GameObject.FindGameObjectWithTag("Floor");
        var numForklifts = FindObjectsByType<ForkliftController>(FindObjectsSortMode.None).Length;
        var index = numForklifts % _initialForkliftControllers.Length;
        var position = new Vector3((-6.5f + (3 * numForklifts)), 0.244f, 0);
        var spawn = Instantiate(_initialForkliftControllers[index].gameObject);
        spawn.transform.position = position;
        spawn.transform.eulerAngles = new Vector3(0, 270, 90);
        spawn.gameObject.SetActive(true);
        spawn.name = spawn.name + " " + numForklifts;
        spawn.transform.SetParent(scene.transform);
        spawn.GetComponent<Rigidbody>().velocity = Vector3.zero;
        SpawnedForklifts.Add(spawn.GetComponent<ForkliftController>());
        if (numForklifts >= 3)
        {
            SpawnedForklifts[SpawnedForklifts.Count - 1].LiftPallet();
            SpawnedForklifts[SpawnedForklifts.Count - 1].RotateGoal(false);

        } else SpawnedForklifts[SpawnedForklifts.Count - 1].LowerPallet(true);
        SceneDataManager.Instance.AddRobot(spawn);
    }

    public static void DestroyForklift()
    {
        var lift = SpawnedForklifts[SpawnedForklifts.Count - 1];
        SpawnedForklifts.RemoveAt(SpawnedForklifts.Count - 1);
        lift.gameObject.SetActive(false);
        if (lift.currGoal) Destroy(lift.currGoal);
        Destroy(lift.gameObject);
    }

    public static void DestroyInitialForklifts()
    {
        foreach (var forkliftController in _initialForkliftControllers)
        {
            Destroy(forkliftController.currGoal);
            Destroy(forkliftController.gameObject);
        }
    }

    private void FillPallet()
    {
        emptyPallet.SetActive(false);
        lift.transform.GetChild(palletIndex).gameObject.SetActive(true);
        palletIndex++;
        palletIndex %= lift.transform.childCount;
        if (palletIndex == emptyPalletIndex) palletIndex++;
        palletIndex %= lift.transform.childCount;
        return;
    }

    private void LightBeacons()
    {
        
    }

    public void LowerPallet(bool emptyImmediate = false)
    {
        movePalletUp = false;
        if (emptyImmediate) EmptyPallet();
        RunDataCollector.LogRoundTrip();
    }

    void EmptyPallet()
    {
        foreach (Transform child in lift.transform)
            if (child.gameObject.activeSelf)
                child.gameObject.SetActive(false);
        emptyPallet.SetActive(true);
    }

    private void OnCollisionEnter(Collision other)
    {
        bool isUser = other.gameObject.GetComponent<UserObstacle>();
        if (isUser && isStuck)
        {
            isStuck = false;
            body.isKinematic = false;
            beaconController.DisableBeacons();
            beaconController.SetStuck(isStuck);
            timeout = true;
            rollDice = 0;
        }
        else
        {
            var robot = other.gameObject.GetComponent<ForkliftController>();
            bool isRobot = robot != null ? robot.isStuck : false;
            bool isPriority = robot != null ? this.ID > robot.ID : true;
            bool isOther = other.gameObject.CompareTag("Obstacle");
            if ((isUser || isOther || isRobot) && isPriority && !isStuck) RunDataCollector.LogCollision();
        }

    }

    #endregion

    #region Private Methods

    void ManageBeacons()
    {
        if (beaconController == null) return;
        if (beaconController.IsFlashing == false && (base.IsRobotNearby() || base.IsObstacleNearby()))
            beaconController.FlashBeacons();
        else if (beaconController.IsFlashing == true && !isStuck)
            beaconController.DisableBeacons();

    }
    void ManagePalletMovement()
    {
        if (movePalletDown && lift.transform.position.y > palletDownLimit)
        {
            lift.transform.position += Vector3.down * 1 * Time.deltaTime;
            if (body.velocity != Vector3.zero) 
                body.velocity = Vector3.zero;
            
            body.isKinematic = true;
            if (!(lift.transform.position.y > palletDownLimit)) // Fully Lowered.
            {
                body.isKinematic = false;
                EmptyPallet();
            }
        }
        else if (movePalletUp && lift.transform.position.y < palletUpLimit)
        {
            lift.transform.position -= Vector3.down * 1 * Time.deltaTime;
        }
    }
    #endregion
}
