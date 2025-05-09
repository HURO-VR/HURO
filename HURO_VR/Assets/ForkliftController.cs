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
    public bool PalletIsDown => lift.transform.position.y <= palletDown;
    public bool PalletIsUp => lift.transform.position.y >= palletDown + upAmount;
    #endregion

    #region Private Variables

    private GameObject lift;
    private bool movePalletUp = false;
    private bool movePalletDown => !movePalletUp;
    private float palletDown;
    private float upAmount = 0.94f;
    private GameObject currGoal;
    [SerializeField] List<Vector3> goalPositions;
    private int goalIndex = 0;
    private Rigidbody body;
    #endregion

    #region Unity Methods

    private void Awake()
    {
        base.Awake();
        foreach (Transform child in transform)
            if (child.name == "Lift")
                lift = child.gameObject;
        palletDown = lift.transform.position.y;
        base.OnGoalReached += () =>
        {
            if (PalletIsUp) LowerPallet();
            else if (PalletIsDown) LiftPallet();
            base.ResetGoal(RotateGoal());
        };
        this.ResetGoal(RotateGoal());
        body = gameObject.GetComponent<Rigidbody>();
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
        if (movePalletDown && lift.transform.position.y > palletDown)
            lift.transform.position += Vector3.down * 1 * Time.deltaTime;
        else if (movePalletUp && lift.transform.position.y < palletDown + upAmount)
            lift.transform.position -= Vector3.down * 1 * Time.deltaTime;
        RotateToVelocity();
        base.Update();
    }
    #endregion

    #region Public Methods

    public void LiftPallet()
    {
        movePalletUp = true;
        Debug.Log($"LiftPallet {palletDown + upAmount}");
    }

    public void LowerPallet()
    {
        movePalletUp = false;
    }
    #endregion

    #region Private Methods
    // Add private methods here
    #endregion
}
