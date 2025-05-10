using System;
using System.Collections;
using System.Collections.Generic;
using Entity_Scripts;
using UnityEngine;

/// <summary>
/// Represents a robot entity that navigates towards a goal within the simulation.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class RobotEntity : MonoBehaviour
{
    #region Private Variables

    /// <summary>
    /// The goal GameObject assigned to this robot.
    /// </summary>
    [SerializeField]
    private GameObject _goal;
    private GameObject goal;
    
    /// <summary>
    /// Indicates whether the robot has reached its goal.
    /// </summary>
    private bool goalReached = false;

    /// <summary>
    /// The Rigidbody component of the robot.
    /// </summary>
    protected Rigidbody body;

    private SphereCollider collider;

    /// <summary>
    /// Reference to the simulation manager.
    /// </summary>
    private SimulationManager algorithmRunner;

    /// <summary>
    /// Timer used to track potential deadlock duration.
    /// </summary>
    private float timer = 0f;
    
    private Vector3 velocity = Vector3.zero;

    #endregion

    #region Public Variables

    /// <summary>
    /// Maximum velocity for the robot.
    /// </summary>
    public float maxVelocity;

    /// <summary>
    /// Gets a value indicating whether the robot is stuck (in deadlock).
    /// </summary>
    public bool stuck { get; private set; }
    
    public event Action OnGoalReached;

    /// <summary>
    /// Initial rotation to be applied to the robot.
    /// </summary>
    [SerializeField]
    protected Vector3 initialRotation;

    /// <summary>
    /// Time limit in seconds to determine deadlock.
    /// </summary>
    [SerializeField]
    float deadlockLimit = 10f;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes required components and sets the GameObject tag.
    /// </summary>
    protected void Awake()
    {
        stuck = false;
        algorithmRunner = FindAnyObjectByType<SimulationManager>();
        body = GetComponent<Rigidbody>();
        collider = GetComponent<SphereCollider>();
        gameObject.tag = "Robot";
    }

    /// <summary>
    /// Called on the frame when the script is enabled.
    /// Initializes the goal if needed and sets initial properties.
    /// </summary>
    protected void Start()
    {
        InitGoal();
        if (maxVelocity == 0)
        {
            maxVelocity = 2;
        }
        gameObject.transform.eulerAngles = initialRotation;
    }

    [SerializeField] private float slowRate;
    /// <summary>
    /// Called once per frame.
    /// Monitors deadlock conditions and updates the robot state.
    /// </summary>
    protected void Update()
    {
        if (algorithmRunner.IsRunning() == false && !body.isKinematic)
            body.velocity = Vector3.zero;
        else if (!goalReached && !body.isKinematic) 
            body.velocity = velocity;
        // Slow down rate.
        velocity *= slowRate;
        if (IsStuck())
        {
            timer += Time.deltaTime;
            if (timer > deadlockLimit)
            {
                stuck = true;
                Debug.Log("STUCK: " + body.velocity.magnitude);
            }
        }
        else
        {
            stuck = false;
            timer = 0;
        }
    }



    /// <summary>
    /// Called when a collision occurs. Adds collision data unless colliding with the floor.
    /// </summary>
    /// <param name="collision">Collision information.</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.name.ToLower().Contains("floor"))
            RunDataCollector.AddCollision(gameObject, collision);
    }

    #endregion

    #region Public Functions

    /// <summary>
    /// Sets the robot's velocity on the x and z axes if the goal has not been reached.
    /// </summary>
    /// <param name="x">Velocity component along the x-axis.</param>
    /// <param name="z">Velocity component along the z-axis.</param>
    public void SetVelocity(float x, float z)
    {
        if (body && !goalReached && !body.isKinematic)
        {
            velocity = new Vector3(x, 0, z);
        }
        else
        {
            //Debug.Log(gameObject.name + " not responding to further input");
        }
    }

    public void ResetGoal(GoalEntity goal)
    {
        this.goal = goal.gameObject;
        this._goal = goal.gameObject;
        goal.SetRobot(gameObject);
        this.goalReached = false;
        body.isKinematic = false;
    }

    /// <summary>
    /// Marks the goal as reached, stops the robot, and disables further movement.
    /// </summary>
    public void GoalReached()
    {
        //Debug.LogWarning(gameObject.name + " reached goal.");
        goalReached = true;
        velocity = Vector3.zero;
        body.velocity = Vector3.zero;
        body.isKinematic = true;
        OnGoalReached?.Invoke();
    }
    

    /// <summary>
    /// Retrieves the goal GameObject assigned to the robot.
    /// If no goal is set, initializes one.
    /// </summary>
    /// <returns>The goal GameObject.</returns>
    public Vector3 GetGoal()
    {
        if (goal == null)
        {
            InitGoal();
        }
        if (goal == null)
        {
            return transform.position;
        }
        return goal.transform.position;
    }

    /// <summary>
    /// Checks if the goal has been reached by the robot.
    /// </summary>
    /// <returns>True if the goal is reached; otherwise, false.</returns>
    public bool IsGoalReached()
    {
        return goalReached;
    }

    /// <summary>
    /// Sets the deadlock time limit for the robot.
    /// </summary>
    /// <param name="deadlockLimit">Deadlock time limit in seconds.</param>
    public void SetDeadlockTimer(float deadlockLimit)
    {
        this.deadlockLimit = deadlockLimit;
    }

    #endregion

    #region Private Functions

    /// <summary>
    /// Initializes the goal by finding an unassigned GoalEntity and linking it to this robot.
    /// </summary>
    private void InitGoal()
    {
        if (_goal != null)
        {
            var control = _goal.GetComponent<GoalEntity>();
            control.SetRobot(gameObject);
            goal = _goal;
        }
        else
        {
            GoalEntity[] goals = FindObjectsByType<GoalEntity>(FindObjectsSortMode.InstanceID);
            foreach (GoalEntity goal in goals)
            {
                if (!goal.HasRobot())
                {
                    goal.SetRobot(gameObject);
                    this.goal = goal.gameObject;
                    return;
                }
            }
            if (gameObject.activeSelf) 
                Debug.LogWarning("Robot " + gameObject.name + " does not have goal.");

        }
    }
    
    public float robotNearbyDistance;
    /// <summary>
    /// Checks if any GameObject with tag "Robot" is within robotNearbyDistance meters of this object's SphereCollider.
    /// </summary>
    /// <returns>True if at least one Robot is nearby; otherwise, false.</returns>
    public bool IsRobotNearby()
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
                //Debug.Log("Nearby: " + hit.gameObject.name);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the robot is stuck (i.e., moving below a minimal velocity threshold)
    /// when the simulation is running.
    /// </summary>
    /// <returns>True if the robot is considered stuck; otherwise, false.</returns>
    bool IsStuck()
    {
        return false; // TODO: Fix deadlock bug. Disabling deadlock detection for now. 
        if (!algorithmRunner.IsRunning()) return false;
        const float deadlockVelocity = 5e-3f;
        return body.velocity.magnitude < deadlockVelocity;
    }

    #endregion
}
