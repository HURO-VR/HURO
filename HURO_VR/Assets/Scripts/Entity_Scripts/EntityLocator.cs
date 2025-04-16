using System;
using UnityEngine;

namespace Entity_Scripts
{
    public class EntityLocator : MonoBehaviour
    {
        [SerializeField] GameObject spotLightPrefab;
        private GameObject spotLight;
        private bool isRobot;
        private void Start()
        {
            if (GetComponent<RobotEntity>() != null) isRobot = true;
            spotLight = Instantiate(spotLightPrefab);
            spotLight.transform.position = gameObject.transform.position;
            spotLight.transform.SetParent(gameObject.transform);
            spotLight.SetActive(true);
            SimulationManager.OnSimulationStart += () => spotLight.SetActive(false);
        }
    }
}