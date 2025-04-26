using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonPortalController : MonoBehaviour, IInteractable
{
    [Header("Portal Settings")]
    [Tooltip("Name of the scene to load when interacted with")]
    public string targetScene;

    [Tooltip("Optional: Specific spawn point ID within the target scene")]
    public string spawnPointID = "DefaultSpawn";

    [Header("Portal Type")]
    [Tooltip("Which dungeon does this Portal lead to?")]
    public DungeonType dungeonType = DungeonType.Easy;

    private bool isPlayerInRange = false;
    public GameObject indicator;
    public DayNight dayNight;

    // Define different dungeon types
    public enum DungeonType
    {
        Easy,
        Medium,
        Hard
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInRange = true;
            indicator.SetActive(true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInRange = false;
            indicator.SetActive(false);
        }
    }

    public void Interact()
    {
        if (isPlayerInRange)
        {
            Debug.Log($"Entering {dungeonType} dungeon");

            // Save the dungeon type and spawn point to PlayerPrefs for the next scene
            PlayerPrefs.SetString("LastDungeonType", dungeonType.ToString());
            PlayerPrefs.SetString("SpawnPointID", spawnPointID);

            dayNight.SetTime("Day");
            SceneManager.LoadScene(targetScene);
        }
    }
}
