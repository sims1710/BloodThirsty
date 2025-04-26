using UnityEngine.Rendering.Universal;
using UnityEngine;

public class FloatLight : MonoBehaviour
{
    public float amplitude = 0.1f;    
    public float frequency = 1f;       

    public float glowMin = 0.5f;       
    public float glowMax = 2.0f;       

    private Vector3 startPos;
    private Light2D light2D;
    private Material mat;

    void Start()
    {
        startPos = transform.position;

        light2D = GetComponent<Light2D>();
        if (light2D == null)
        {
            light2D = GetComponentInChildren<Light2D>();
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            mat = sr.material; 
            if (mat.HasProperty("_EmissionColor"))
                mat.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        float wave = Mathf.Sin(Time.time * frequency);
        float yOffset = wave * amplitude;
        float glow = Mathf.Lerp(glowMin, glowMax, (wave + 1f) / 2f);

        transform.position = startPos + new Vector3(0, yOffset, 0);

        if (light2D != null)
        {
            light2D.intensity = glow;
        }
    }
}
