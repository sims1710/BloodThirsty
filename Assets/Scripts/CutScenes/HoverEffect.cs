using UnityEngine;

public class HoverEffect : MonoBehaviour
{
    [Header("Hover Settings")]
    public float amplitude = 0.5f;  
    public float frequency = 1f;    

    [Header("Glow Settings (optional)")]
    public float glowMin = 1f;
    public float glowMax = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float wave = Mathf.Sin(Time.time * frequency);
        float yOffset = wave * amplitude;
        float glow = Mathf.Lerp(glowMin, glowMax, (wave + 1f) / 2f); 

        transform.position = startPos + new Vector3(0f, yOffset, 0f);
    }
}
