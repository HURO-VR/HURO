using UnityEngine;

public class Robot : Entity
{
    public XYZ position;
    public XYZ goal;
    public float goal_radius;
    public XYZ curr_velocity;
    public float max_velocity;
    public float radius;
    public string name;

    public static float DEFAULT_GOAL_RADIUS = 0.05f;

    /// <summary>
    /// Initializes a Robot instance using data from the given GameObject.
    /// </summary>
    /// <param name="go">The GameObject representing the robot.</param>
    public Robot(GameObject go)
    {
        SetData(go);
    }

    /// <summary>
    /// Sets the robot's data based on the provided GameObject's transform and components.
    /// </summary>
    /// <param name="go">The GameObject representing the robot.</param>
    public void SetData(GameObject go)
    {
        Transform robot_transform = go.transform;
        Vector3 robot_velocity = go.GetComponent<Rigidbody>().velocity;
        RobotEntity robotController = go.GetComponent<RobotEntity>();
        SphereCollider sphereCollider = go.GetComponent<SphereCollider>();

        Vector3 goalPos = robotController.GetGoal();

        Renderer renderer = robot_transform.GetComponent<Renderer>();
        this.name = go.name;

        this.position.x = robot_transform.position.x;
        this.position.y = robot_transform.position.y;
        this.position.z = robot_transform.position.z;

        this.curr_velocity.x = robot_velocity.x;
        this.curr_velocity.y = robot_velocity.y;
        this.curr_velocity.z = robot_velocity.z;

        this.goal.x = goalPos.x;
        this.goal.y = goalPos.y;
        this.goal.z = goalPos.z;

        this.max_velocity = robotController.maxVelocity;
        this.radius = sphereCollider.radius * robot_transform.localScale.x;
        this.goal_radius = DEFAULT_GOAL_RADIUS;
    }

    /// <summary>
    /// Draws gizmos to visually represent the robot and its goal in the scene.
    /// </summary>
    public void DrawGizmo()
    {
        if (radius == 0) return;
        SceneDataUtils.DrawCircleGizmo(position, radius, Color.red);
        SceneDataUtils.DrawCircleGizmo(goal, goal_radius, Color.red);
    }
}
