using Entity_Scripts;
using UnityEngine;

/// <summary>
/// Represents the goal entity that robots can interact with during the simulation.
/// </summary>
///
[RequireComponent(typeof(SphereCollider))]
public class GoalEntity : MonoBehaviour
{
    #region Private Variables

    /// <summary>
    /// The robot GameObject associated with this goal.
    /// </summary>
    private RobotEntity robot;

    /// <summary>
    /// The initial rotation to apply to the goal.
    /// </summary>
    [SerializeField]
    private Vector3 initialRotation;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Sets the game object's tag to "Goal".
    /// </summary>
    void Awake()
    {
        gameObject.tag = "Goal";
        var collider = gameObject.GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = Robot.DEFAULT_GOAL_RADIUS;
    }

    /// <summary>
    /// Called on the frame when the script is enabled.
    /// Sets the initial rotation of the goal.
    /// </summary>
    private void Start()
    {
        gameObject.transform.eulerAngles = initialRotation;
    }
    
    protected bool inGoal = false;
    /// <summary>
    /// Called once per frame.
    /// </summary>
    void Update()
    {
    }

    /// <summary>
    /// Called when another collider enters the trigger collider attached to the goal.
    /// If the collider belongs to the associated robot, marks the goal as reached.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (robot == null) return;
        if (other.gameObject == this.robot.gameObject && other.isTrigger == false)
        {
            robot.GoalReached();
        }
    }

    #endregion

    #region Public Functions

    /// <summary>
    /// Associates a robot GameObject with this goal.
    /// </summary>
    /// <param name="robot">The robot GameObject to associate.</param>
    public void SetRobot(GameObject robot)
    {
        this.robot = robot.GetComponent<RobotEntity>();
    }

    /// <summary>
    /// Checks whether a robot is associated with this goal.
    /// </summary>
    /// <returns>True if a robot is associated; otherwise, false.</returns>
    public bool HasRobot()
    {
        return this.robot != null;
    }

    #endregion
}
