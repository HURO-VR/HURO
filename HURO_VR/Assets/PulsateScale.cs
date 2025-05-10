using UnityEngine;

/// <summary>
/// Smoothly scales the GameObject between its original size and half size in a loop.
/// </summary>
public class PulsateScale : MonoBehaviour
{
    public float speed;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool shrinking = true;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale * 0.5f;
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        if (shrinking)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, step);
            if (transform.localScale == targetScale)
                shrinking = false;
        }
        else
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, originalScale, step);
            if (transform.localScale == originalScale)
                shrinking = true;
        }
    }
}