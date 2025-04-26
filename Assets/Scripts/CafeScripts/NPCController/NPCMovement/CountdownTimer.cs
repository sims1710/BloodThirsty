using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public float countdownTime = 300f; 
    public TMP_Text timerText;
    private float currentTime;
    private bool timerRunning = false;
    private RandomWalker randomWalker;
    private ShopOpenController shopOpenController;
    public GameObject summaryPanelUI;

    private void Start()
    {
        currentTime = countdownTime;
        randomWalker = FindObjectOfType<RandomWalker>();
        shopOpenController = FindObjectOfType<ShopOpenController>();
    }

    private void Update()
    {
        if (timerRunning)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                StopTimer();
            }

            UpdateTimerDisplay();
        }
    }

    public void StartTimer()
    {
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
        if (randomWalker != null)
        {
            randomWalker.StopSpawning();
        }
        if (shopOpenController != null)
        {
            shopOpenController.CloseShopUI();
        }
        timerText.text = string.Format("Shop is closed!");
        summaryPanelUI.SetActive(true);
        Invoke("HideSummaryPanelAfterDelay", 4f);
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            if (currentTime <= 0)
            {
                timerText.text = string.Format("Shop is closed!");
                return;
            }
            else
            {
                int minutes = Mathf.FloorToInt(currentTime / 60);
                int seconds = Mathf.FloorToInt(currentTime % 60);
                timerText.text = string.Format("Shop is opened! : " + "{0:00}:{1:00}", minutes, seconds);
            }
        }
    }

    private void HideSummaryPanelAfterDelay()
    {
        summaryPanelUI.SetActive(false);
    }
}