using UnityEngine;
using TMPro;

public class BiteCoinController : MonoBehaviour
{
    public BiteCoins biteCoins;
    public TextMeshProUGUI amountText;
    void Start()
    {
        amountText.text = biteCoins.GetCurrentAmount().ToString();
    }

    // Update is called once per frame
    void Update()
    {
        amountText.text = biteCoins.GetCurrentAmount().ToString();
    }
}
