using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TutorialPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI pageText;

    [Header("Tutorial Settings")]
    [SerializeField] private List<GameObject> tutorialPanels;
    [SerializeField] private KeyCode toggleKey = KeyCode.Alpha0;
    [SerializeField] private bool showOnStart = false;

    private int currentPage = 0;
    private bool isVisible = false;

    private void Start()
    {
        // Initialize UI
        if (panel == null) panel = gameObject;
        if (previousButton != null) previousButton.onClick.AddListener(PreviousPage);
        if (nextButton != null) nextButton.onClick.AddListener(NextPage);

        // Hide all tutorial panels initially
        foreach (var tutorialPanel in tutorialPanels)
        {
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }
        }

        // Hide main panel and buttons
        panel.SetActive(false);
        if (previousButton != null) previousButton.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);

        // Show panel on start if configured
        if (showOnStart)
        {
            isVisible = true;
            panel.SetActive(true);
        }

        UpdatePage();
    }

    private void Update()
    {
        // Toggle panel visibility
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePanel();
        }

        // Navigate with arrow keys
        if (isVisible)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                PreviousPage();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                NextPage();
            }
        }
    }

    public void TogglePanel()
    {
        isVisible = !isVisible;
        panel.SetActive(isVisible);
        if (isVisible)
        {
            UpdatePage();
        }
        else
        {
            // Hide all tutorial panels when closing
            foreach (var tutorialPanel in tutorialPanels)
            {
                if (tutorialPanel != null)
                {
                    tutorialPanel.SetActive(false);
                }
            }
            // Hide buttons when panel is closed
            if (previousButton != null) previousButton.gameObject.SetActive(false);
            if (nextButton != null) nextButton.gameObject.SetActive(false);
        }
    }

    public void NextPage()
    {
        if (currentPage < tutorialPanels.Count - 1)
        {
            // Hide current panel
            if (tutorialPanels[currentPage] != null)
            {
                tutorialPanels[currentPage].SetActive(false);
            }

            currentPage++;
            UpdatePage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            // Hide current panel
            if (tutorialPanels[currentPage] != null)
            {
                tutorialPanels[currentPage].SetActive(false);
            }

            currentPage--;
            UpdatePage();
        }
    }

    private void UpdatePage()
    {
        if (tutorialPanels == null || tutorialPanels.Count == 0) return;

        // Hide all panels first
        foreach (var tutorialPanel in tutorialPanels)
        {
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }
        }

        // Show current panel
        if (currentPage < tutorialPanels.Count && tutorialPanels[currentPage] != null)
        {
            tutorialPanels[currentPage].SetActive(true);
        }

        // Update page text
        if (pageText != null)
        {
            pageText.text = $"{currentPage + 1}/{tutorialPanels.Count}";
        }

        // Update button visibility
        if (previousButton != null)
        {
            previousButton.gameObject.SetActive(currentPage > 0);
        }
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(currentPage < tutorialPanels.Count - 1);
        }
    }
}