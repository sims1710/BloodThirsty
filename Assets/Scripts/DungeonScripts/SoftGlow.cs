using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SoftGlow : MonoBehaviour
{
    public float glowMin = 0.9f;       
    public float glowMax = 1.2f;      
    public float glowSpeed = 3f;       

    private Light2D light2D;

    void Start()
    {
        light2D = GetComponent<Light2D>();
    }

    void Update()
    {
        if (light2D == null) return;

        float wave = Mathf.Sin(Time.time * glowSpeed); 
        float intensity = Mathf.Lerp(glowMin, glowMax, (wave + 1f) / 2f); 
        light2D.intensity = intensity;
    }
}

