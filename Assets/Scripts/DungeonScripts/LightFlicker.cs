using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    private Light2D bulbLight;
    private float baseIntensity;
    private float flickerTimer;

    void Start()
    {
        bulbLight = GetComponent<Light2D>();
        baseIntensity = bulbLight.intensity;
        flickerTimer = Random.Range(0.5f, 2f);
    }

    void Update()
    {
        flickerTimer -= Time.deltaTime;

        if (flickerTimer <= 0)
        {
            float flickerChance = Random.value;

            if (flickerChance < 0.05f) 
            {
                bulbLight.intensity = 0;
            }
            else if (flickerChance < 0.2f) 
            {
                bulbLight.intensity = baseIntensity * Random.Range(0.3f, 0.7f);
            }
            else
            {
                bulbLight.intensity = baseIntensity;
            }

            flickerTimer = Random.Range(1f, 3f); 
        }
    }
}

