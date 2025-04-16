using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using TMPro;
using Microsoft.Scripting.Hosting;
using System.Runtime.CompilerServices;
using IronPython.Hosting;
using Meta.XR.MRUtilityKit;

[RequireComponent(typeof(SceneDataManager))]
[RequireComponent(typeof(GoogleCloudServer))]
public class SimulationManager : MonoBehaviour
{
    #region Public Variables

    /// <summary>
    /// True to display algorithm view of scene (Scene View Only).
    /// </summary>
    [Tooltip("True to display algorithm view of scene (Scene View Only).")]
    [SerializeField] bool displayGizmos;

    /// <summary>
    /// Text Mesh to add Debug Logs.
    /// </summary>
    [Tooltip("Text Mesh to add Debug Logs.")]
    [SerializeField] TextMeshProUGUI debugLogs;

    /// <summary>
    /// Mark true if running on GCP VM.
    /// </summary>
    [Tooltip("Mark true if running on GCP VM.")]
    [SerializeField] bool runOnServer;

    /// <summary>
    /// Robot and Goal Spawners.
    /// </summary>
    [Tooltip("Robot and Goal Spawners.")]
    [SerializeField] List<FindSpawnPositions> spawners;

    /// <summary>
    /// Runs the algorithm every X seconds.
    /// </summary>
    [Tooltip("Runs the algorithm every X seconds.")]
    [SerializeField] float interval = 0.25f; // Run every x seconds

    /// <summary>
    /// Will end simulation after X seconds.
    /// </summary>
    [Tooltip("Will end simulation after X seconds.")]
    [SerializeField] float simulationTimeout = 30f;

    #endregion

    #region Private Variables

    private ScriptEngine engine;
    private dynamic algorithm;
    private bool initAlgorithm = false;
    private bool algorithmRunning = false;
    private bool hitServerAgain = true;
    private bool restart = false;
    private SceneDataManager sceneData;
    private AudioLibrary audioLibrary;
    private GoogleCloudServer remoteScriptExecutor;
    private StreamingAssetsManager fileTransfer;
    private SessionManager sessionManager;
    private float timer = 0f;
    private float totalTime = 0f;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Called when the script instance is being loaded. Initializes required components.
    /// </summary>
    private void Awake()
    {
        // Ensure server is enabled at Runtime on Quest.
#if UNITY_ANDROID
        runOnServer = true;
#endif
        try
        {
            if (!sceneData) sceneData = GetComponent<SceneDataManager>();
            audioLibrary = FindAnyObjectByType<AudioLibrary>();
            remoteScriptExecutor = GetComponent<GoogleCloudServer>();
            fileTransfer = FindAnyObjectByType<StreamingAssetsManager>();
            sessionManager = FindAnyObjectByType<SessionManager>();
        }
        catch (Exception e)
        {
            DebugLogs(e.ToString());
        }
    }

    /// <summary>
    /// Called once per frame to update simulation state, process algorithm steps, and handle input.
    /// </summary>
    private void Update()
    {
        DetectKeyBoardActivation();

        if (!algorithmRunning || (fileTransfer != null && !fileTransfer.IsCopyingComplete()))
            return;
        else if (!initAlgorithm)
            InitAlgorithm();

        IncrementTime();
        if (ShouldTerminate())
            EndSimulation();

        if (ShouldStep())
        {
            sceneData.UpdateSceneData();
            AlgorithmStep();
        }
        RunDataCollector.UpdateRobotData();
    }

    /// <summary>
    /// Draws gizmos in the scene view if enabled.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (displayGizmos && sceneData)
        {
            sceneData.DrawGizmos();
        }
    }

    /// <summary>
    /// Called when the object is destroyed. Closes any active SSH connections.
    /// </summary>
    private void OnDestroy()
    {
        remoteScriptExecutor?.CloseSSHConnection();
    }

    #endregion

    #region Public Functions

    /// <summary>
    /// Checks whether the algorithm is currently running.
    /// </summary>
    /// <returns>True if the algorithm is running; otherwise, false.</returns>
    public bool IsRunning()
    {
        return algorithmRunning;
    }

    /// <summary>
    /// Initializes the algorithm and related simulation data.
    /// </summary>
    public void InitAlgorithm()
    {
        if (runOnServer)
        {
            remoteScriptExecutor.OpenSSHConnection();
        }
        else
        {
            engine = Python.CreateEngine();
            SetImportPaths(engine);
            algorithm = engine.ExecuteFile(Application.streamingAssetsPath + @"/Python/main.py");
        }

        if (!sceneData)
            sceneData = GetComponent<SceneDataManager>();

        sceneData.InitSceneData();
        RunDataCollector.InitializeSimulation(sceneData.LoadOutput());
        initAlgorithm = true;
    }

    /// <summary>
    /// Starts the algorithm execution and plays audio if applicable.
    /// </summary>
    /// <param name="start">If set to true, starts the algorithm; otherwise, does nothing.</param>
    public void StartAlgorithm(bool start = true)
    {
        if (!start)
            return; // This will be false when being called from the menu.
        if (audioLibrary && !algorithmRunning)
            audioLibrary.PlayAudio(AudioLibrary.AudioType.StartSimulation);
        algorithmRunning = true;
        if (restart)
            RandomizeObjectLocations();
        Debug.Log("HURO: Starting simulation");
    }

    /// <summary>
    /// Toggles the algorithm between running and paused states.
    /// </summary>
    public void ToggleAlgorithm()
    {
        bool start = !algorithmRunning;
        Debug.Log("Toggle with " + start);
        if (start)
            StartAlgorithm();
        else
            PauseAlgorithm();
    }

    /// <summary>
    /// Randomizes the positions of all objects managed by spawners.
    /// </summary>
    /// <remarks>
    /// Destroys all existing child objects of each spawner, triggers a new spawn sequence, and resets the restart flag.
    /// </remarks>
    public void RandomizeObjectLocations()
    {
        if (algorithmRunning)
        {
            Debug.LogWarning("May not randomize locatons in middle of simulation.");
            return;
        }
        foreach (var spawner in spawners)
        {
            for (int i = 0; i < spawner.transform.childCount; i++)
            {
                Destroy(spawner.transform.GetChild(i).gameObject);
            }
            spawner.StartSpawn();
        }
        restart = false;
    }

    /// <summary>
    /// Ends the simulation, uploads run data, and plays ending audio.
    /// </summary>
    /// <param name="stop">If set to true, ends the simulation.</param>
    public void EndSimulation(bool stop = true)
    {
        if (algorithmRunning && stop)
        {
            RunDataCollector.EndSimulation();
            PauseAlgorithm();
            sessionManager?.UploadSimulationRunData(RunDataCollector.runMetadata);
            audioLibrary.PlayAudio(AudioLibrary.AudioType.EndSimulation);
            restart = true;
        }
    }

    #endregion

    #region Private Functions

    /// <summary>
    /// Sets the search paths for the IronPython engine.
    /// </summary>
    /// <param name="engine">The IronPython script engine.</param>
    void SetImportPaths(ScriptEngine engine)
    {
        ICollection<string> searchPaths = engine.GetSearchPaths();
        // Path to the folder of filename.
        searchPaths.Add(Application.streamingAssetsPath + @"/Python/");
        // Path to the Python standard library.
        searchPaths.Add(Application.dataPath + @"/Plugins/IronPy/Lib/");
        engine.SetSearchPaths(searchPaths);
    }

    /// <summary>
    /// Pauses the algorithm by stopping all robot movement and resetting the timer.
    /// </summary>
    void PauseAlgorithm()
    {
        algorithmRunning = false;
        GameObject[] robots = GameObject.FindGameObjectsWithTag("Robot");
        foreach (GameObject robot in robots)
        {
            Rigidbody rigidbody = robot.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.velocity = Vector3.zero;
            }
        }
        timer = 0f;
    }

    /// <summary>
    /// Executes the algorithm locally by passing the input and processing the output.
    /// </summary>
    /// <param name="input">The input string for the algorithm.</param>
    void RunAlgorithmLocally(string input)
    {
        string output = algorithm.main(input);
        OnScriptComplete(output);
    }

    /// <summary>
    /// Called when the algorithm script completes execution. Processes the output.
    /// </summary>
    /// <param name="output">The output string from the algorithm.</param>
    void OnScriptComplete(string output)
    {
        if (output.Length == 0)
        {
            Debug.LogWarning("SSH Command did not output any ");
        }
        else
        {
            var newVelocities = JsonConvert.DeserializeObject<float[][]>(output);
            SetNewVelocities(newVelocities);
            if (runOnServer)
                hitServerAgain = true;
        }
    }

    /// <summary>
    /// Executes the algorithm on the remote server.
    /// </summary>
    /// <param name="param">The parameter string to pass to the server command.</param>
    void RunAlgorithmOnServer(string param)
    {
        remoteScriptExecutor.OnScriptExecutionComplete += OnScriptComplete;
        remoteScriptExecutor.ExecuteCommand(param);
    }

    /// <summary>
    /// Sets the new velocities for each robot based on the algorithm output.
    /// </summary>
    /// <param name="newVelocities">A jagged array containing velocity vectors for each robot.</param>
    void SetNewVelocities(float[][] newVelocities)
    {
        int stopSim = 0;
        GameObject[] robots_go = GameObject.FindGameObjectsWithTag("Robot");
        foreach (var go in robots_go)
        {
            int i = 0;
            foreach (var robot in sceneData.robots)
            {
                if (newVelocities[i][0] + newVelocities[i][1] == 0)
                {
                    stopSim++;
                    continue;
                }
                if (go.name == robot.name)
                {
                    go.GetComponent<RobotEntity>().SetVelocity(newVelocities[i][0], newVelocities[i][1]);
                }
                i++;
            }
        }
        if (stopSim == robots_go.Length)
        {
            Debug.LogWarning("Robots stopped");
        }
    }

    /// <summary>
    /// Determines whether the simulation should terminate based on timeout, goal completion, or deadlock.
    /// </summary>
    /// <returns>True if simulation should terminate; otherwise, false.</returns>
    bool ShouldTerminate()
    {
        bool timeout = totalTime > simulationTimeout;
        bool reachedGoals = RunDataCollector.CheckAllRobotsReachedGoal();
        bool deadlock = true;
        RobotEntity[] robots = FindObjectsByType<RobotEntity>(FindObjectsSortMode.InstanceID);
        foreach (var robot in robots)
        {
            if (!robot.stuck)
                deadlock = false;
        }

        if (deadlock)
        {
            RunDataCollector.LogFailed("Deadlock - Robots were not moving.");
            Debug.LogWarning("Terminating due to deadlock.");
        }
        if (timeout)
        {
            RunDataCollector.LogFailed($"Timeout - Robots failed to reach the goals in {simulationTimeout} seconds.");
            Debug.LogWarning($"Terminating due to timeout. {totalTime} seconds.");
        }
        if (reachedGoals)
        {
            Debug.LogWarning("Terminating because robots reached goals.");
        }
        return (timeout || reachedGoals || deadlock);
    }

    /// <summary>
    /// Increments the simulation timer.
    /// </summary>
    void IncrementTime()
    {
        timer += Time.deltaTime;
        totalTime += Time.deltaTime;
    }

    /// <summary>
    /// Checks if the simulation should perform a step based on the set interval.
    /// </summary>
    /// <returns>True if enough time has passed for a simulation step; otherwise, false.</returns>
    bool ShouldStep()
    {
        return timer >= interval;
    }

    /// <summary>
    /// Executes a single algorithm step by processing scene input and updating robot velocities.
    /// </summary>
    void AlgorithmStep()
    {
        timer = 0f; // Reset timer
        try
        {
            if ((runOnServer && hitServerAgain) || !runOnServer)
            {
                string input = sceneData.GetAlgorithmInput();
                if (runOnServer && hitServerAgain)
                {
                    hitServerAgain = false;
                    RunDataCollector.LogServerHit();
                    RunAlgorithmOnServer(input);
                }
                else if (!runOnServer)
                {
                    RunAlgorithmLocally(input);
                }
            }
        }
        catch (Exception e)
        {
            DebugLogs(e.ToString());
            RunDataCollector.LogWarning(e.ToString());
            EndSimulation();
            DebugLogs("Stopping Simulation");
        }
    }

    /// <summary>
    /// Logs debug messages to the TextMeshPro component and Unity console.
    /// </summary>
    /// <param name="message">The debug message.</param>
    /// <param name="file">The caller file path (automatically provided).</param>
    /// <param name="line">The caller line number (automatically provided).</param>
    void DebugLogs(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        debugLogs.text += "\n\n" + message;
        Debug.Log($"HURO: {file} @line:{line}: " + message);
    }

    /// <summary>
    /// Detects keyboard inputs to trigger various simulation actions.
    /// </summary>
    void DetectKeyBoardActivation()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            ToggleAlgorithm();
        }

        if (Input.GetKeyUp(KeyCode.I))
        {
            InitAlgorithm();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomizeObjectLocations();
        }
    }

    #endregion
}
