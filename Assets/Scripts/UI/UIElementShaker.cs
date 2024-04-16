using System.Collections;
using UnityEngine;

public class UIElementShaker : MonoBehaviour
{
    public RectTransform uiElement; 
    public float shakeDuration = 0.5f; 
    public float shakeIntensity = 0.1f;
    public float shakeSpeed = 50f;

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = uiElement.position;
    }

    public void Shake()
    {
        StartCoroutine(ShakeCoroutine());
    }

    IEnumerator ShakeCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            // Calculate random position within shake intensity
            Vector3 randomPos = originalPosition + Random.insideUnitSphere * shakeIntensity;

            // Apply the shake effect to the UI element
            uiElement.position = Vector3.Lerp(uiElement.position, randomPos, Time.deltaTime * shakeSpeed);

            // Increment time
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Reset UI element to original position after shake
        uiElement.position = originalPosition;
    }
}