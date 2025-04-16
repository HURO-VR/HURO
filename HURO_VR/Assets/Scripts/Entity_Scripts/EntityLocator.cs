using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entity_Scripts
{
    public class EntityLocator : MonoBehaviour
    {
        [FormerlySerializedAs("spotLightPrefab")] [SerializeField] GameObject locatorPrefab;
        private GameObject locator;
        private bool isRobot;
        private void Start()
        {
            if (GetComponent<RobotEntity>() != null) isRobot = true;
            locator = Instantiate(locatorPrefab);
            locator.transform.position = new Vector3(gameObject.transform.position.x, 1.8f, gameObject.transform.position.z);
            locator.transform.SetParent(gameObject.transform);
            //spotLight.GetComponent<Light>().color = lightColor;
            locator.SetActive(true);
            SimulationManager.OnSimulationStart += () => locator.SetActive(false);
        }
    }
}