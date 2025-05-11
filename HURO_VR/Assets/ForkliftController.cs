using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

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
    private float palletDownLimit;
    private float palletUpLimit => palletDownLimit + upAmount;
    private float upAmount = 0.94f;
    private GameObject currGoal;
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
    private bool isStuck;
    
    #endregion

    #region Unity Methods

    private void Awake()
    {
        base.Awake();
        foreach (Transform child in transform)
            if (child.name == "Lift")
                lift = child.gameObject;
        palletDownLimit = lift.transform.position.y;
        base.OnGoalReached += () =>
        {
            if (PalletIsUp) LowerPallet();
            else if (PalletIsDown) LiftPallet();
            base.ResetGoal(RotateGoal());
        };
        goalPositions = new List<Vector3>();
        foreach (Transform child in entryPoints)
            goalPositions.Add(child.position);
        base.body.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezePositionY;
        this.ResetGoal(RotateGoal());

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
        _forkliftControllers = FindObjectsByType<ForkliftController>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
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

    private GoalEntity RotateGoal()
    {
        Destroy(currGoal);
        currGoal = new GameObject();
        var entity = currGoal.AddComponent<GoalEntity>();
        
        currGoal.tag = "Goal";
        currGoal.transform.position = goalPositions[goalIndex];
        goalIndex++;
        if (goalIndex >= goalPositions.Count) goalIndex = 0;
        return entity;
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

    private static ForkliftController[] _forkliftControllers;

    public static void ActivateLift()
    {
        GameObject spawn = null;
        foreach (var lift in _forkliftControllers)
            if (lift.gameObject.activeSelf == false)
                spawn = lift.gameObject;
        if (spawn) spawn.SetActive(true);
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

    public void LowerPallet()
    {
        movePalletUp = false;
    }

    void EmptyPallet()
    {
        foreach (Transform child in lift.transform)
            if (child.gameObject.activeSelf)
                child.gameObject.SetActive(false);
        emptyPallet.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<UserObstacle>())
        {
            isStuck = false;
            body.isKinematic = false;
            beaconController.DisableBeacons();
            beaconController.SetStuck(isStuck);
            timeout = true;
            rollDice = 0;
        }
    }

    #endregion

    #region Private Methods

    void ManageBeacons()
    {
        if (beaconController == null) return;
        if (base.IsRobotNearby() && beaconController.IsFlashing == false)
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
