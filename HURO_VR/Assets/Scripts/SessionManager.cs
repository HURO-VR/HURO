using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.MRUtilityKit;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages simulation sessions by loading simulation metadata, updating the simulation UI, 
/// and uploading simulation run data to the database.
/// </summary>
public class SessionManager : MonoBehaviour
{
    #region Public Variables

    /// <summary>
    /// Array containing simulation metadata bundles retrieved from the database.
    /// </summary>
    public SimulationMetaData[] simulationMetaData = null;

    /// <summary>
    /// Dropdown UI element used for selecting simulations.
    /// </summary>
    [SerializeField] TMP_Dropdown simulationDropdown;

    #endregion

    #region Private Variables

    /// <summary>
    /// Organization ID obtained from demo credentials.
    /// </summary>
    string Organization_ID = DemoCredentials.organizationID;

    /// <summary>
    /// User ID obtained from demo credentials.
    /// </summary>
    string userID = DemoCredentials.nikitaID;

    /// <summary>
    /// Currently selected simulation metadata.
    /// </summary>
    SimulationMetaData selectedSimulation;

    /// <summary>
    /// Number of runs recorded for the selected simulation.
    /// </summary>
    int numberRuns = 1;

    /// <summary>
    /// Reference to the storage component used for database interactions.
    /// </summary>
    Storage db;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes the Storage and sets the organization ID.
    /// </summary>
    void Awake()
    {
        db = FindAnyObjectByType<Storage>();
        db.SetOrganization(Organization_ID);
    }

    /// <summary>
    /// Called on the frame when the script is enabled.
    /// Loads simulation metadata from the database and populates the simulation dropdown.
    /// </summary>
    private void Start()
    {
        _ = db.GetAllSimulationBundles((data) =>
        {
            simulationMetaData = data;
            List<string> options = new List<string>();

            // Reverse the simulation metadata so that the earliest simulation is at the top.
            var reversed = simulationMetaData.Reverse();
            foreach (var item in reversed)
            {
                options.Add(item.name);
            }
            simulationDropdown.ClearOptions();
            simulationDropdown.AddOptions(options);

            // Set the first simulation as selected if none is selected.
            if (selectedSimulation.ID == null || selectedSimulation.ID.Length == 0)
                SetSelectedSimulation(0);

            Debug.Log($"HURO: Loaded {data?.Length} bundles into menu.");
        });
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    void Update()
    {
        // No update functionality required at the moment.
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Selects a simulation based on the provided index.
    /// Updates the currently selected simulation and retrieves the count of existing runs.
    /// </summary>
    /// <param name="index">Index of the simulation to select (default is 0).</param>
    public void SetSelectedSimulation(int index = 0)
    {
        Debug.Log($"HURO: Selecting {index} simulation");
        if (index < simulationMetaData?.Length)
        {
            selectedSimulation = simulationMetaData[index];
            db.GetFirestoreCollection($"organizations/{Organization_ID}/simulations/{selectedSimulation.ID}/runs", (data) =>
            {
                numberRuns = data.Count;
            });
        }
        else
        {
            Debug.LogWarning($"HURO: Simulation {index} out of range of {simulationMetaData.Length} simulations");
        }
    }

    /// <summary>
    /// Uploads simulation run data to the database.
    /// Sets the user ID, simulation ID, and constructs a unique run name before uploading.
    /// </summary>
    /// <param name="data">The simulation run metadata to upload.</param>
    public void UploadSimulationRunData(RunMetadata data)
    {
        data.uid = userID;
        data.simID = selectedSimulation.ID;
        name = $"Run {numberRuns + 1}";
        data.runID = name + "_" + data.runID;
        db.UploadMetadata($"organizations/{Organization_ID}/simulations/{selectedSimulation.ID}/runs/{data.runID}", data.ToDictionary(), OnCompleteUpload);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Callback method that is called upon completion of the upload process.
    /// Increments the run count if the upload was successful.
    /// </summary>
    /// <param name="completed">True if the upload was successful; otherwise, false.</param>
    private void OnCompleteUpload(bool completed)
    {
        if (completed)
        {
            numberRuns++;
        }
        else
        {
            Debug.LogWarning("Failed to upload simulation run data");
        }
    }

    #endregion
}
