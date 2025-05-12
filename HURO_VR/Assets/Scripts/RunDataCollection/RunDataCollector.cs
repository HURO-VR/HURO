using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;
using Newtonsoft.Json;
using RunDataCollection.Classes;

/// <summary>
/// A static data collector that manages run metadata, robot data, and simulation data for the current session.
/// </summary>
public static class RunDataCollector
{
    # region Public Members
    /// <summary>
    /// Gets the metadata for the current run.
    /// </summary>
    public static RunMetadata runMetadata { get; private set; }
    public static CentennialData centennialData { get; private set; }
    #endregion

    # region Private Members
    /// <summary>
    /// The simulation start time in milliseconds.
    /// </summary>
    private static float simulationStartTime;
    private static Storage db = GameObject.FindObjectOfType<Storage>();
    /// <summary>
    /// Flag indicating whether the simulation data collector has been initialized.
    /// </summary>
    private static bool initalized = false;

    private static bool isCentennialData = true;
    public static bool isLogging { get; private set; } = false;
    #endregion

    # region Serializable Fields
    // (If there are any [SerializeField] members, add them here.)
    #endregion

    # region Public Functions

    /// <summary>
    /// Initializes the simulation by setting up the run metadata and capturing the starting time.
    /// </summary>
    /// <param name="sceneData">The scene data output containing obstacles and other scene information.</param>
    public static void InitializeSimulation(SceneDataManager.SceneDataOutput sceneData)
    {
        if (isCentennialData)
        {
            centennialData = new CentennialData();
        }
        else
        {
                runMetadata = new RunMetadata
                {
                    dateCreated = System.DateTime.UtcNow.ToString("o"),
                    status = RunStatus.success.ToString(),
                    starred = false,
                    runID = System.Guid.NewGuid().ToString(),
                    name = $"Run ..", // Set in Session Controller
                    data = new RunData
                    {
                        robotData = InitAllRobotData(),
                        obstacleData = InitObstacles(sceneData),
                        totalCollisions = new List<XYZ>(),
                        deadlock = false
                    }
                };
                isLogging = true;
        }

        if (db == null) db = GameObject.FindAnyObjectByType<Storage>();
        simulationStartTime = Time.time * 1000; // Convert to milliseconds
        initalized = true;
        Debug.Log("HURO: Initalized Data Collector");
    }

    public static void UploadLogData()
    {
        if (!isCentennialData) return;
        centennialData.timeCreated = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        db.UploadMetadata($"centennial/{centennialData.timeCreated}", centennialData.ToJson(), b =>
        {
            Debug.Log("Centennial Data Upload complete.");
        });
        isLogging = false;
    }

    /// <summary>
    /// Updates the robot data by checking for robot positions and marking when a robot reaches its goal.
    /// </summary>
    public static void UpdateRobotData()
    {
        if (isCentennialData) return;
        GameObject[] robots = GameObject.FindGameObjectsWithTag("Robot");

        foreach (var robotData in runMetadata.data.robotData)
        {
            foreach (var go in robots)
            {
                if (go.name == robotData.name)
                {
                    RobotEntity robotController = go.GetComponent<RobotEntity>();
                    robotData.robotPath.Add(go.transform.position);

                    if (robotController.IsGoalReached() && !robotData.goalReached)
                    {
                        robotData.goalReached = true;
                        robotData.robotEnd = robotData.goalPosition;
                        robotData.robotPath.Add(robotData.goalPosition);
                    }
                }
            }
        }
    }
    /// <summary>
    /// Logs a hit from the server by incrementing the server hit count.
    /// </summary>
    public static void LogServerHit()
    {
        if  (isCentennialData) return;
        runMetadata.serverHits++;
    }
    public static void StartLogging() => isLogging = true;
    public static void LogCollision() {
        if (isLogging)
        {
            centennialData.collisions++;
            Debug.Log($"Logged collision. Now: {centennialData.collisions}");
        }
    }
    public static void LogRoundTrip() {
        if (isLogging) centennialData.roundTrips++;
    }

    public static void SetNumMachines(int numMachines)
    {
        centennialData.numMachines = numMachines;
    }

    public static void SetVelocity(float velocity)
    {
        centennialData.maxVelocity = velocity;
    } 
    

    public static void SetClearance(float clearance)
    {
        centennialData.clearance = clearance;
    }

    /// <summary>
    /// Logs a warning by setting the error message and updating the run status to Warning.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    public static void LogWarning(string message)
    {
        runMetadata.errorMessage = message;
        runMetadata.status = RunStatus.warning.ToString();
    }

    /// <summary>
    /// Logs a failure by setting the error message and updating the run status to Failed.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    public static void LogFailed(string message)
    {
        runMetadata.errorMessage = message;
        runMetadata.status = RunStatus.failed.ToString();
    }

    /// <summary>
    /// Adds a collision event by recording the collision point for both the overall run data and the specific robot.
    /// </summary>
    /// <param name="robot">The robot GameObject involved in the collision.</param>
    /// <param name="collision">The collision data (unused, but kept for potential future use).</param>
    public static void AddCollision(GameObject robot, Collision collision)
    {
        if (!initalized)
        {
            return;
        }
        XYZ collisionPoint = robot.transform.position;
        runMetadata.data.totalCollisions.Add(collisionPoint);
        foreach (var ro in runMetadata.data.robotData)
        {
            if (ro.name == robot.name)
            {
                ro.collisions.Add(collisionPoint);
            }
        }
    }

    /// <summary>
    /// Ends the simulation by finalizing robot data, calculating the simulation time, and logging the run metadata.
    /// </summary>
    public static void EndSimulation()
    {
        if (!initalized)
        {
            Debug.LogWarning("unitialized sim data");
            return;
        }

        if (isCentennialData)
        {
            
        }
        else
        {
            foreach (var ro in runMetadata.data.robotData)
            {
                if (!ro.goalReached)
                {
                    ro.robotEnd = ro.robotPath[ro.robotPath.Count - 1];
                }
            }
            runMetadata.data.timeToComplete = (int)((Time.time * 1000) - simulationStartTime);
            runMetadata.data.deadlock = !CheckAllRobotsReachedGoal();
        }

        Debug.Log("HURO: Simulation Ended");
        initalized = false;
    }

    /// <summary>
    /// Checks if all robots have reached their designated goals.
    /// </summary>
    /// <returns>
    /// True if all robots have reached their goals; otherwise, false.
    /// </returns>
    public static bool CheckAllRobotsReachedGoal()
    {
        if (!initalized)
            return false;

        foreach (var robot in runMetadata.data.robotData)
        {
            if (!robot.goalReached)
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    # region Private Functions

    /// <summary>
    /// Initializes data for all robots currently in the scene.
    /// </summary>
    /// <returns>A list of RobotData for each robot found in the scene.</returns>
    static List<RobotData> InitAllRobotData()
    {
        GameObject[] robot_gos = GameObject.FindGameObjectsWithTag("Robot");
        List<RobotData> robots = new List<RobotData>();

        foreach (GameObject robot_go in robot_gos)
        {
            robots.Add(InitRobotData(robot_go));
        }
        return robots;
    }

    /// <summary>
    /// Initializes data for a single robot.
    /// </summary>
    /// <param name="robot_go">The robot GameObject to initialize data for.</param>
    /// <returns>A RobotData object with initial values set based on the robot's current state.</returns>
    static RobotData InitRobotData(GameObject robot_go)
    {
        RobotEntity robotController = robot_go.GetComponent<RobotEntity>();

        return new RobotData
        {
            name = robot_go.name,
            robotStart = robot_go.transform.position,
            robotPath = new List<XYZ>(),
            goalPosition = robotController.GetGoal(),
            collisions = new List<XYZ>(),
            goalReached = false
        };
    }

    /// <summary>
    /// Initializes obstacle data from the scene data.
    /// </summary>
    /// <param name="sceneData">The scene data output containing obstacle information.</param>
    /// <returns>A list of ObstacleData objects representing the obstacles in the scene.</returns>
    static List<ObstacleData> InitObstacles(SceneDataManager.SceneDataOutput sceneData)
    {
        List<ObstacleData> obstacles = new List<ObstacleData>();
        foreach (Circle go in sceneData.obstacles)
        {
            obstacles.Add(new ObstacleData
            {
                position = go.position,
                radius = go.radius
            });
        }
        return obstacles;
    }

    #endregion
}
