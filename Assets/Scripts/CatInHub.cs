using TMPro;
using UnityEngine;

public class CatInHub : MonoBehaviour
{
    public BiteCoins bitecoins;
    public TextMeshProUGUI bitecoinCounter;
    public CatSaved catSaved;
    public float bitecoinsNeeded = 5000;
    private float bitecoinsLeft;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (catSaved.GetCatSaved())
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check difference from what is needed
        bitecoinsLeft = bitecoinsNeeded - bitecoins.GetCurrentAmount();
        if (bitecoinsLeft <= 0)
        {
            bitecoinCounter.text = "You can save me now!";
        }
        else
        {
            bitecoinCounter.text = bitecoinsLeft + "\nbitecoins more...";
        }
    }


}
