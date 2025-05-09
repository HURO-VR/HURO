using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.MRUtilityKit;
using Newtonsoft.Json;
using UnityEngine;

public partial class SceneDataManager : MonoBehaviour
{
    #region Serialized Variables

    // Add any [SerializeField] variables here if needed

    #endregion

    #region Public Variables

    public MRUKAnchor.SceneLabels mrukObstacleLabel;

    #endregion

    #region Private Variables

    private bool initalized = false;
    private MRUKRoom mruk;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Initializes the SceneDataManager and finds the MRUKRoom instance.
    /// </summary>
    void Start()
    {
        mruk = FindObjectOfType<MRUKRoom>();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes scene data including robots, obstacles, and boundaries.
    /// Must be called during the initialization phase of the scene.
    /// </summary>
    public void InitSceneData()
    {
        if (mruk) LabelMRObjects();

        robots = InitRobotData();
        Debug.Log("Initalized Robots: " + robots.Length);

        Robot[] goals = robots.Where((robot) => robot.goal.x != 0).ToArray();
        Debug.Log("Initalized Goals: " + goals.Length );

        obstacles = InitObstacles();
        Debug.Log("Initalized Obstacles: " + obstacles.Length);

        boundary = new Boundary(GameObject.FindGameObjectWithTag("Floor"));
        Debug.Log("Initalized Boundary of size " + boundary.length + " by " + boundary.width + " and position: " + boundary.position);

        robot_radius = robots[0].radius;
        Debug.Log("Initalized Robot Radius: " + robot_radius);

        initalized = true;
    }

    /// <summary>
    /// Updates the scene data including robots and obstacles.
    /// </summary>
    public void UpdateSceneData()
    {
        UpdateRobotData();
        UpdateObstacleData();
    }

    /// <summary>
    /// Draws gizmos for visual representation of obstacles, boundaries, and robots.
    /// </summary>
    public void DrawGizmos()
    {
        if (obstacles != null)
            foreach (var obstacle in obstacles)
            {
                obstacle.DrawGizmo();
            }

        if (boundary != null) boundary.DrawGizmo();
        if (robots != null) foreach (var robot in robots)
            {
                robot.DrawGizmo();
            }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Labels the objects in the MR environment as obstacles or floors.
    /// </summary>
    private void LabelMRObjects()
    {
        bool floor = false;
        foreach (MRUKAnchor anchor in mruk.Anchors)
        {
            Renderer mesh = anchor.gameObject.GetComponentInChildren<Renderer>();
            if ((mrukObstacleLabel & anchor.Label) != 0)
            {
                if (mesh != null) mesh.gameObject.tag = "Obstacle";
            }

            if ((anchor.Label & MRUKAnchor.SceneLabels.FLOOR) != 0)
            {
                mesh.gameObject.tag = "Floor";
                floor = true;
            }
        }

        if (!floor)
        {
            mruk.FloorAnchor.gameObject.GetComponentInChildren<Renderer>().gameObject.tag = "Floor";
        }
    }

    /// <summary>
    /// Initializes robot data from GameObjects tagged as "Robot".
    /// </summary>
    /// <returns>An array of Robot instances.</returns>
    private Robot[] InitRobotData()
    {
        GameObject[] robot_gos = GameObject.FindGameObjectsWithTag("Robot");
        Robot[] robots = new Robot[robot_gos.Length];
        int i = 0;
        foreach (GameObject robot_go in robot_gos)
        {
            robots[i] = new Robot(robot_go);
            i++;
        }
        return robots;
    }

    /// <summary>
    /// Updates the data of existing robots.
    /// </summary>
    private void UpdateRobotData()
    {
        GameObject[] robots_gos = GameObject.FindGameObjectsWithTag("Robot");
        foreach (var robot in robots)
        {
            foreach (var go in robots_gos)
            {
                if (go.name == robot.name)
                {
                    robot.SetData(go);
                }
            }
        }
    }

    /// <summary>
    /// Initializes obstacle data from GameObjects tagged as "Obstacle".
    /// </summary>
    /// <returns>An array of Obstacle instances.</returns>
    private Obstacle[] InitObstacles()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Obstacle");
        Obstacle[] obstacles = new Obstacle[gos.Length];
        int i = 0;
        foreach (GameObject go in gos)
        {
            obstacles[i] = new Obstacle(go);
            i++;
        }
        return obstacles;
    }

    /// <summary>
    /// Updates the data of dynamic obstacles.
    /// </summary>
    private void UpdateObstacleData()
    {
        foreach (Obstacle obstacle in obstacles)
        {
            if (obstacle.isDynamic)
            {
                obstacle.position = obstacle.go.transform.position;
                if (obstacle.circleAbstraction != null)
                {
                    obstacle.CreateCircleAbstraction();
                }
            }
        }
    }

    #endregion
}
