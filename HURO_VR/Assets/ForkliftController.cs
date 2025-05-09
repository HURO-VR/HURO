using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
    #endregion

    #region Private Variables

    private GameObject lift;
    private bool movePalletUp = false;
    private bool movePalletDown => !movePalletUp;
    private float palletDownLimit;
    private float palletUpLimit => palletDownLimit + upAmount;
    private float upAmount = 0.94f;
    private GameObject currGoal;
    [SerializeField] List<Vector3> goalPositions;
    private int goalIndex = 0;
    private Rigidbody body;
    GameObject emptyPallet;
    private int emptyPalletIndex = 0;
    private int palletIndex = -1;
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
        goalPositions.Add(lift.transform.position);
        this.ResetGoal(RotateGoal());
        body = gameObject.GetComponent<Rigidbody>();
        for (int i = 0; i < lift.transform.childCount; i++)
            if (lift.transform.GetChild(i).gameObject.activeSelf)
            {
                emptyPallet = lift.transform.GetChild(i).gameObject;
                emptyPalletIndex = i;
                break;
            }

        palletIndex = emptyPalletIndex + 1;
        palletIndex %= lift.transform.childCount;
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

    private void Update()
    {
        ManagePalletMovement();
        RotateToVelocity();
        base.Update();
    }
    #endregion

    #region Public Methods
    public void LiftPallet()
    {
        movePalletUp = true;
        FillPallet();
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
    #endregion

    #region Private Methods

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
