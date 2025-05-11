using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls incrementing and decrementing a list of numbers using up/down arrows.
/// </summary>
public class IncrementController : MonoBehaviour
{
    [SerializeField] private List<GameObject> numberList;
    private GameObject currentNumber;
    private int currentIndex = 0;
    public event Action OnIncrement;
    public event Action OnDecrement;

    private void Awake()
    {
        var arrows = GetComponentsInChildren<ArrowController>();
        foreach (var arrow in arrows)
        {
            if (arrow.type == ArrowController.Type.Increment)
                arrow.OnTrigger += Increment;
            else if (arrow.type == ArrowController.Type.Decrement)
                arrow.OnTrigger += Decrement;
        }

        currentNumber = numberList[currentIndex];
    }

    
    private void Start()
    {
        UpdateDisplay();
    }

    public void Increment()
    {
        if (currentIndex < numberList.Count - 1)
        {
            currentIndex++;
            UpdateDisplay();
            OnIncrement?.Invoke();
        }
    }

    public void Decrement()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateDisplay();
            OnDecrement?.Invoke();
        }
    }

    private void UpdateDisplay()
    {
        currentNumber.SetActive(false);
        numberList[currentIndex].SetActive(true);
        currentNumber = numberList[currentIndex];
    }
}