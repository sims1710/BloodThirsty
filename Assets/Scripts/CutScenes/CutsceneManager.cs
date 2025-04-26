using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load after the cutscene.")]
    public string nextSceneName;
    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Next scene name not assigned in inspector.");
        }
    }
}

