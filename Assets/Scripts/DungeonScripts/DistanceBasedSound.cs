using UnityEngine;

public class DistanceBasedSound : MonoBehaviour
{
    private Transform player;
    private AudioSource audioSource;

    public float maxDistance = 10f;
    public float minVolume = 0.1f;
    public float maxVolume = 3f;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        InvokeRepeating(nameof(FindPlayer), 0f, 1f); 
    }

    void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                CancelInvoke(nameof(FindPlayer));
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        float normalizedDistance = Mathf.Clamp01(distance / maxDistance);

        audioSource.volume = Mathf.Lerp(maxVolume, minVolume, normalizedDistance) * 3;
        audioSource.pitch = Mathf.Lerp(maxPitch, minPitch, normalizedDistance) * 2;

        // Debug.Log(gameObject.name + " | Volume: " + audioSource.volume + " | Pitch: " + audioSource.pitch);
    }
}
