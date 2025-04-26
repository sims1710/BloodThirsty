using UnityEngine;

public class DebugAudioDistance : MonoBehaviour
{
    private Transform player;
    private AudioSource audioSource;

    public float maxDistance = 10f;
    public float minVolume = 0.1f;
    public float maxVolume = 1.0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Find the player dynamically
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure your player prefab has the 'Player' tag.");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Adjust volume based on distance
        float normalizedDistance = Mathf.Clamp01(distance / maxDistance);
        audioSource.volume = Mathf.Lerp(maxVolume, minVolume, normalizedDistance);

        // Print the volume for each fire
        // Debug.Log(gameObject.name + " volume: " + audioSource.volume);
    }
}
