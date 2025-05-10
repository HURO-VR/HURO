using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

// Part of SceneDataManager that defines how the algorithm receives input.
public partial class SceneDataManager : MonoBehaviour
{
    /// <summary>
    /// A stripped-down version of SceneData for algorithm output.
    /// </summary>
    public struct SceneDataOutput
    {
        public float robot_radius;
        public Circle[] obstacles;
        public Boundary boundary;
        public Robot[] robots;
    }

    public float robot_radius { get; private set; }
    public Obstacle[] obstacles { get; private set; }
    public Boundary boundary { get; private set; }
    public Robot[] robots { get; private set; }
    private SceneDataOutput output;

    /// <summary>
    /// Loads and returns the output data structure for the scene.
    /// </summary>
    /// <returns>A SceneDataOutput struct containing relevant scene information.</returns>
    public SceneDataOutput LoadOutput()
    {
        output.robot_radius = robot_radius;
        
        UpdateRobotGoals();
        output.robots = robots;
        output.boundary = boundary;
        output.obstacles = Obstacle.UnpackAbstractions(obstacles);
        return output;
    }

    private void UpdateRobotGoals()
    {
        var robotControllers = GameObject.FindObjectsByType<RobotEntity>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        for (int i = 0; i < robots.Length; i++)
        {
            var controller = robotControllers.First(r => r.name == robots[i].name);
            if (controller) robots[i].goal = controller.GetGoal();
        }
    }

    /// <summary>
    /// Serializes the scene data into a JSON string for algorithm input.
    /// </summary>
    /// <returns>A JSON string representing the scene data.</returns>
    public string GetAlgorithmInput()
    {
        return JsonConvert.SerializeObject(LoadOutput());
    }
}
