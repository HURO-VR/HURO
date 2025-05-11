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
    public event Action<int> OnIncrement;
    public event Action<int> OnDecrement;

    
    private void Start()
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
        UpdateDisplay();
    }

    public void Increment()
    {
        if (currentIndex < numberList.Count - 1)
        {
            currentIndex++;
            UpdateDisplay();
            OnIncrement?.Invoke(currentIndex);
        }
    }

    public void Decrement()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateDisplay();
            OnDecrement?.Invoke(currentIndex);
        }
    }

    private void UpdateDisplay()
    {
        currentNumber.SetActive(false);
        numberList[currentIndex].SetActive(true);
        currentNumber = numberList[currentIndex];
    }
}