using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DungeonHubDoorController : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [Tooltip("Name of the dungeon hub scene")]
    public string dungeonHubScene = "DungeonHub";
    public TMP_Text indicatorText;
    public DayNight dayNight;
    private bool isPlayerInRange = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInRange = true;
            indicatorText.color = Color.white;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInRange = false;
            indicatorText.color = Color.black;
        }
    }

    public void Interact()
    {
        if (isPlayerInRange)
        {
            // Save player stats, inventory etc. if needed
            SavePlayerState();
            dayNight.SetTime("Night");
            SceneManager.LoadScene(dungeonHubScene);
        }
    }

    private void SavePlayerState()
    {
        PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);
    }
}
