using System.Collections;
using UnityEngine;

public class ShakeManager : MonoBehaviour
{
    public static ShakeManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public void ShakeCamera(float intensity, float duration)
    {
        StartCoroutine(Shake(intensity, duration));
    }
    private IEnumerator Shake(float intensity, float duration)
    {
        float elapsed = 0.0f;
        Vector3 originalPos = Camera.main.transform.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float offsetX = (Mathf.PerlinNoise(Time.time * 10f, 0f) - 0.5f) * intensity;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * 10f) - 0.5f) * intensity;
            Camera.main.transform.localPosition = new Vector3(originalPos.x + offsetX, originalPos.y + offsetY, originalPos.z);

            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }
}