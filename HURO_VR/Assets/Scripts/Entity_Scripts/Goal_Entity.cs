using UnityEngine;

/// <summary>
/// Represents the goal entity that robots can interact with during the simulation.
/// </summary>
public class GoalEntity : MonoBehaviour
{
    #region Private Variables

    /// <summary>
    /// The robot GameObject associated with this goal.
    /// </summary>
    private GameObject robot;

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
    }

    /// <summary>
    /// Called on the frame when the script is enabled.
    /// Sets the initial rotation of the goal.
    /// </summary>
    private void Start()
    {
        gameObject.transform.eulerAngles = initialRotation;
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    void Update()
    {
        // No update functionality required.
    }

    /// <summary>
    /// Called when another collider enters the trigger collider attached to the goal.
    /// If the collider belongs to the associated robot, marks the goal as reached.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name + " and " + robot.name);
        if (other.gameObject == this.robot)
        {
            var controller = this.robot.GetComponent<RobotEntity>();
            controller.GoalReached();
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
        this.robot = robot;
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
