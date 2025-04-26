using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CatSurgery : MonoBehaviour
{
    public BiteCoins biteCoins;
    public TextMeshProUGUI errorText; 
    public CatSaved catSaved;

    public void OnPurchase() {
        if (biteCoins.SubtractAmount(5000) == false)
        {
            setErrorMessageTimer("Not enough Bitecoins!");
            return;
        }
        catSaved.SetCatSaved(true);
        SceneManager.LoadScene("EndCutScene"); 
    }

    private void setErrorMessageTimer(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
        Invoke("HideErrorText", 2f);
    }

    private void HideErrorText()
    {
        errorText.gameObject.SetActive(false);
    }
}
