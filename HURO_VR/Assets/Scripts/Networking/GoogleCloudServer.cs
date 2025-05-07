using UnityEngine;
using System;
using System.Collections;
using Renci.SshNet;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
using Debug = UnityEngine.Debug;

public class GoogleCloudServer : MonoBehaviour
{
    // HURO VM Compute IP.
    string serverIP = "104.154.36.87";

    // SSH username
    string username = "accou";

    string privateKeyPath = "/google_compute_engine";

    // Path to python script on VM.
    string remoteScriptPath = "/home/accou/Python_Scripts/Python/main.py"; 
    public bool Connected = false;

    [Header("Logging")]
    [Tooltip("Enable detailed logging")]
    public bool enableLogging = true;

    // Callback for when script execution is complete
    public delegate void ScriptExecutionCompleted(string result);
    public event ScriptExecutionCompleted OnScriptExecutionComplete;
    SshClient client;

    private void Awake()
    {
        privateKeyPath = Application.persistentDataPath + privateKeyPath;
    }
    // Callback for logging
    private void Log(string message)
    {
        if (enableLogging)
        {
            Debug.Log($"[RemoteScriptExecutor] {message}");
        }
    }

    public void SetSimulationID(string simulationID)
    {
        remoteScriptPath = $"/home/accou/Python_Scripts/{simulationID}/main.py";
    }

    // Public method to trigger SSH script execution
    public void ExecuteRemoteScript(string json_param)
    {
        StartCoroutine(ExecuteRemoteScriptCoroutine(json_param));
    }

    public void OpenSSHConnection()
    {
        var privateKeyFile = new PrivateKeyFile(privateKeyPath);
        var authMethod = new PrivateKeyAuthenticationMethod(username, privateKeyFile);
        var connectionInfo = new ConnectionInfo(serverIP, username, authMethod);
        // Create SSH connection
        client = new SshClient(connectionInfo);
        // Connect to the SSH server
        client.Connect();
        Connected = true;
        Log("SSH connection established.");
    }

    public void CloseSSHConnection()
    {
        if (client != null)
        {
            client.Disconnect();
            Connected = false;
        }
        client = null;
    }
    
    
    /// <summary>
    /// Executes a remote Python script with the given parameter asynchronously to avoid blocking the main thread.
    /// </summary>
    /// <param name="param">The parameter to pass to the Python script.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    public IEnumerator ExecuteCommandCoroutine(string param)
    {
        if (remoteScriptPath == null)
        {
            Debug.LogWarning("Must set Simulation ID before Executing Script.");
            OnScriptExecutionComplete?.Invoke("[]");
            yield break;
        }

        string commandToExecute = $"python3 {remoteScriptPath} '{param}'";
        string result = string.Empty;

        // Execute the SSH command in a separate thread to avoid blocking the main thread.
        bool isDone = false;
        Thread executionThread = new Thread(() =>
        {
            try
            {
                using (var cmd = client.CreateCommand(commandToExecute))
                {
                    cmd.Execute();

                    if (cmd.ExitStatus == 0)
                    {
                        result = cmd.Result.Trim();
                    }
                    else
                    {
                        Debug.LogError($"Command failed. Command:{commandToExecute.Substring(0, 20)} Error: {cmd.Error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while executing command: {ex.Message}");
            }

            isDone = true;
        });

        executionThread.Start();

        // Wait until the thread completes
        while (!isDone)
            yield return null;

        OnScriptExecutionComplete?.Invoke(result);
    }


    private IEnumerator ExecuteRemoteScriptCoroutine(string param)
    {
        var timer = new Stopwatch();
        // Placeholder to allow for async operations if needed
        yield return null;
        // Load the private key
        var privateKeyFile = new PrivateKeyFile(privateKeyPath);
        var authMethod = new PrivateKeyAuthenticationMethod(username, privateKeyFile);
        var connectionInfo = new ConnectionInfo(serverIP, username, authMethod);


        // Create SSH connection
        using (var client = new SshClient(connectionInfo))
        {
            // Connect to the SSH server
            client.Connect();
            Log("SSH connection established.");
                
            // Prepare command to execute remote Python script
            string commandToExecute = $"python3 {remoteScriptPath} '{param}'";

            // Execute the remote script
            
            string result = ExecuteCommand(client, commandToExecute);

            // Log runtime
            var runtime = timer.Elapsed;
            Log($"Algo Runtime: {runtime} seconds");

            // Parse the result
            Log($"Script Output: {result}");

            // Invoke completion callback
            OnScriptExecutionComplete?.Invoke(result);
            // Disconnect
            client.Disconnect();
            Log("SSH connection closed.");
        }

    }

    // Helper method to execute SSH commands and return output
    private string ExecuteCommand(SshClient client, string command)
    {
        
        using (var cmd = client.CreateCommand(command))
        {
            cmd.Execute();

            if (cmd.ExitStatus == 0)
            {
                Log($"Command executed successfully: {command}");
                return cmd.Result.Trim();
            }
            else
            {
                Debug.LogError($"Command failed. Command: {command}, Error: {cmd.Error}");
                return string.Empty;
            }
        }
    }

    // Example usage method
    public void StartRemoteScriptExecution()
    {
        // Add a listener to handle the script execution result
        OnScriptExecutionComplete += HandleScriptExecutionResult;

        // Trigger the script execution
       // ExecuteRemoteScript("");
    }

    // Example result handler
    private void HandleScriptExecutionResult(string result)
    {
        if (result != null)
        {
            try
            {
                // Optionally parse the JSON result
                var parsedResult = JsonConvert.DeserializeObject<dynamic>(result);
                Debug.Log($"Parsed Result: {parsedResult}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse result: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Script execution failed or returned null result.");
        }

        // Remove the listener to prevent multiple calls
        OnScriptExecutionComplete -= HandleScriptExecutionResult;
    }
}