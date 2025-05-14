using System;
using System.Collections.Generic;
using UnityEngine;

public class ClipboardController : MonoBehaviour
{
    
    
    #region Serialized Variables
    // Add any [SerializeField] variables here if needed
    // Add Headers: [Header("Logging")]
    // Add Tips: [Tooltip("Runs the algorithm every X seconds.")]

    #endregion

    #region Public Variables
    // Add public variables here
    #endregion

    #region Private Variables
    IncrementController machineController;
    IncrementController velocityController;
    IncrementController bufferController;
    
    public List<GameObject> machines;
    private Rigidbody velocityMachine;
    private Vector3 originalPosition;
    public static float velocity = 0.07f;
    public static int numMachines = 1;
    private GameObject bufferHalo;
    public static float bufferScale = 1;
    #endregion

    #region Unity Methods

    private void Awake()
    {
        var controllers = gameObject.GetComponentsInChildren<IncrementController>();
        foreach (var controller in controllers)
        {
            if (controller.name.ToLower().Contains("machine"))
                machineController = controller;
            if (controller.name.ToLower().Contains("velocity"))
            {
                velocityController = controller;
                foreach (Transform child in controller.transform)
                    if (child.name.ToLower().Contains("forklift"))
                        velocityMachine = child.gameObject.GetComponent<Rigidbody>();
            }
            if (controller.name.ToLower().Contains("buffer"))
            {
                bufferController = controller;
                foreach (Transform child in controller.transform)
                    if (child.name.ToLower().Contains("halo"))
                        bufferHalo = child.gameObject;
            }
        }
        velocityMachine.constraints = RigidbodyConstraints.FreezePositionY;
        originalPosition = velocityMachine.transform.localPosition;
        machineController.OnIncrement += DisplayMachine;
        machineController.OnDecrement += HideMachine;
        
        velocityController.OnIncrement += IncreaseVelocity;
        velocityController.OnDecrement += DecreaseVelocity;

        bufferController.OnIncrement += ScaleBuffer;
        bufferController.OnDecrement += ShrinkBuffer;

    }

    private void Start()
    {
        // Initialization code
    }

    private bool left = true;
    private static int m = 2;
    private void Update()
    {
        if ((velocityMachine.transform.localPosition.x > (3.6f * SimulationManager.sceneScale.localScale.x) && !left) ||
            (velocityMachine.transform.localPosition.x < (3.55f * SimulationManager.sceneScale.localScale.x) && left))
            TurnMachine();
        velocityMachine.velocity = -velocityMachine.transform.up * velocity;
        if (Input.GetKeyDown(KeyCode.Space))
            DisplayMachine(m++);
    }
    #endregion

    #region Public Methods
    // Add public methods here
    #endregion

    #region Private Methods

    private void DisplayMachine(int i)
    {
        if (i < machines.Count)
        {
            machines[i].SetActive(true);
            numMachines++;
            ForkliftController.SpawnForklift();
            RunDataCollector.SetNumMachines(numMachines);
        }
    }

    private void HideMachine(int i)
    {
        if (i + 1 > 0)
        {
            machines[i + 1].SetActive(false);
            numMachines--;
            ForkliftController.DestroyForklift();
            RunDataCollector.SetNumMachines(numMachines);
        }
    }

    private void TurnMachine()
    {
        velocityMachine.transform.localEulerAngles = new Vector3(velocityMachine.transform.localEulerAngles.x, 
            velocityMachine.transform.localEulerAngles.y - 180, 
            velocityMachine.transform.localEulerAngles.z);
        left = !left;
    }

    private float scale = 1.2f;
    private void IncreaseVelocity(int i)
    {
        velocity *= 1.2f;
        RunDataCollector.SetVelocity(i + 1);
        RobotEntity.IncreaseVelocity(scale);
    }

    private void DecreaseVelocity(int i)
    {
        velocity /= 1.2f;
        RunDataCollector.SetVelocity(i + 1);
        RobotEntity.DecreaseVelocity(scale);
    }

    private void ScaleBuffer(int i)
    {
        bufferHalo.transform.localScale *= 1.2f;
        bufferScale *= 1.2f;
        RobotEntity.IncreaseClearance(scale);
        RunDataCollector.SetClearance(i + 1);
    }

    private void ShrinkBuffer(int i)
    {
        bufferHalo.transform.localScale /= 1.2f;
        bufferScale /= 1.2f;
        RobotEntity.DecreaseClearance(scale);
        RunDataCollector.SetClearance(i + 1);
    }
    #endregion
}
