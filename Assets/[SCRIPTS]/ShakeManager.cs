using System.Collections;
using UnityEngine;

public class ShakeManager : MonoBehaviour
{
    public static ShakeManager instance;
    private Camera _camera;

    [SerializeField] private float _intensity = 0.1f;
    [SerializeField] private float _duration = 0.1f;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        _camera = Camera.main;
    }
    public void ShakeCamera(float intensity, float duration)
    {
        if (_camera == null)
        {
        }
        StartCoroutine(Shake(intensity, duration));
    }
    private IEnumerator Shake(float intensity, float duration)
    {
        float elapsed = 0.0f;
        Vector3 originalPos = _camera.transform.localPosition;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;

            float offsetX = (Mathf.PerlinNoise(Time.time * 10f, 0f) - 0.5f) * _intensity;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * 10f) - 0.5f) * _intensity;
            _camera.transform.localPosition = new Vector3(originalPos.x + offsetX, originalPos.y + offsetY, originalPos.z);

            yield return null;
        }
        _camera.transform.localPosition = originalPos;
    }
}