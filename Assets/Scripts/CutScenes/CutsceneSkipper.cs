using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneSkipper : MonoBehaviour
{
    [Tooltip("Name of the scene to load after the cutscene.")]
    public string sceneToLoad;

    void Start()
    {
        Time.timeScale = 1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SkipCutscene();
        }
    }

    public void SkipCutscene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Scene name is not assigned!");
        }
    }
}

