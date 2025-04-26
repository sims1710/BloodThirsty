using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireFlickering : MonoBehaviour
{
    private Light2D fireLight;
    private float baseIntensity;
    private Color baseColor;

    void Start()
    {
        fireLight = GetComponent<Light2D>();
        baseIntensity = fireLight.intensity;
        baseColor = fireLight.color; 
    }

    void Update()
    {
        float flicker = Mathf.PerlinNoise(Time.time * 4f, 0f) * 0.5f;
        fireLight.intensity = baseIntensity + flicker - 0.2f;
        fireLight.color = baseColor + new Color(0f, flicker * 0.1f, 0f);
    }
}
