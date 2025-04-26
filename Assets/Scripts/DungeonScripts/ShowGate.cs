using UnityEngine;
using TMPro;

public class ShowGate : MonoBehaviour, IInteractable
{
    public bool isGate;
    public int levelBefore;
    public TextMeshProUGUI placeholderText;
    private bool isPlayerInRange = false;
    [SerializeField] private LevelProgress levelProgress;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (levelProgress.IsLevelCompleted(levelBefore)){
            this.gameObject.SetActive(!isGate);
        }
        else
        {
            this.gameObject.SetActive(isGate);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    public void Interact()
    {
        if (isPlayerInRange && isGate)
        {
            setErrorMessageTimer("Complete the previous level to unlock this gate!");
        }
    }

    private void setErrorMessageTimer(string message)
    {
        placeholderText.text = message;
        placeholderText.gameObject.SetActive(true);
        Invoke("HideErrorText", 2f);
    }

    private void HideErrorText()
    {
        placeholderText.gameObject.SetActive(false);
    }

}
