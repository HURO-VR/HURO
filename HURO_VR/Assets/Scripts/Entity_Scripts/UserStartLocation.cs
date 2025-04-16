using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity_Scripts
{
    public class UserStartLocation : MonoBehaviour
    {
        public Vector3 startLocation;
        private GameObject userLocation;
        private float timer;
        private bool isCountingDown = false;
        public List<GameObject> numbers;
        private event Action OnFinishCountDown;
        
        private void Awake()
        {
            userLocation = FindObjectOfType<UserObstacle>().gameObject;

            OnFinishCountDown += () =>
            {
                SimulationManager.Instance.StartAlgorithm();
            };
        }

        private void Update()
        {
            bool locationTrigger = IsInXZBox(transform, userLocation.transform.position);
            if ((locationTrigger && !isCountingDown) || Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("Start countdown");
                StartCountDown();
            }
            else if (!locationTrigger)
            {
                //StopCountdown();
            }
            
            
        }

        void StartCountDown()
        {
            isCountingDown = true;
            StartCoroutine(CountdownShrink());
        }

        void StopCountdown()
        {
            isCountingDown = false;
        }
        
        /// <summary>
        /// Sequentially shows each GameObject, makes it look at the camera, and scales it down to zero over time.
        /// </summary>
        /// <param name="numbers">List of GameObjects to display and shrink.</param>
        /// <param name="shrinkDuration">Time in seconds for each object to shrink.</param>
        IEnumerator CountdownShrink(float shrinkDuration = 1f)
        {
            Camera cam = Camera.main;
            yield return new WaitForSeconds(2f); // Delay countdown start.
            foreach (GameObject _obj in numbers)
            {
                if (!isCountingDown) break;
                Debug.Log("Countdown obj create.");
                var obj = Instantiate(_obj, transform);
                obj.SetActive(true);
                obj.transform.localScale = Vector3.one;
                obj.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z + 0.7f);
                //obj.transform.localPosition = cam.transform.position + cam.transform.forward * 0.7f;
                obj.transform.eulerAngles = new Vector3(-90, 180, 0); // Face number right direction
                
                float elapsed = 0f;
                Vector3 initialScale = obj.transform.localScale;

                while (elapsed < shrinkDuration && isCountingDown)
                {
                    Debug.Log("Countdwn scaling.");
                    // Make the object look at the camera
                    Vector3 lookDirection = cam.transform.position - obj.transform.position;
                    lookDirection.y = 0f; // Optional: keep it level
                    //obj.transform.rotation = Quaternion.LookRotation(lookDirection);

                    float t = elapsed / shrinkDuration;
                    obj.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                obj.transform.localScale = Vector3.zero;
                obj.SetActive(false);
            }
            if (isCountingDown) OnFinishCountDown?.Invoke();
        }


        
        /// <summary>
        /// Checks if a transform's XZ position is within a 0.5 x 0.5 box around a target point.
        /// </summary>
        /// <param name="t">Transform to check.</param>
        /// <param name="center">Center of the box in world space.</param>
        /// <returns>True if inside the box, false otherwise.</returns>
        bool IsInXZBox(Transform t, Vector3 center)
        {
            Vector3 pos = t.position;
            return Mathf.Abs(pos.x - center.x) <= 0.25f && Mathf.Abs(pos.z - center.z) <= 0.25f;
        }

    }
}