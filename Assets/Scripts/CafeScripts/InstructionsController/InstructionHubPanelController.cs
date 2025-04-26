using UnityEngine;
using System.Collections;

public class InstructionHubPanelController : MonoBehaviour
{
    public InstructionsTracker instructionsTracker;
    private bool isNewPlayer;

    public GameObject startPanel;
    public GameObject secondPanel;
    public GameObject thirdPanel;
    public GameObject forthPanel;
    public GameObject fifthPanel;
    public GameObject sixthPanel;

    void Start()
    {
        isNewPlayer = instructionsTracker.GetIsNewPlayerDungeonHub();

        if (isNewPlayer)
        {
            StartCoroutine(ShowPanels());
        }
    }

    private IEnumerator ShowPanels()
    {
        Time.timeScale = 0;

        yield return StartCoroutine(ShowPanel(startPanel, 3f));
        yield return StartCoroutine(ShowPanel(secondPanel, 3f));
        yield return StartCoroutine(ShowPanel(thirdPanel, 5f));
        yield return StartCoroutine(ShowPanel(forthPanel, 5f));
        yield return StartCoroutine(ShowPanel(fifthPanel, 3f));
        yield return StartCoroutine(ShowPanel(sixthPanel, 3f));

        Time.timeScale = 1;
    }

    private IEnumerator ShowPanel(GameObject panelName, float time)
    {
        Time.timeScale = 0;

        panelName.SetActive(true);
        yield return new WaitForSecondsRealtime(time);
        panelName.SetActive(false);

        Time.timeScale = 1;
        instructionsTracker.SetIsNewPlayerDungeonHub(false);
    }
}
