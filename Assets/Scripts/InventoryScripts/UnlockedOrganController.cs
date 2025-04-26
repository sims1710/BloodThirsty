using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnlockedOrganController : MonoBehaviour
{
    public GameObject panel;
    public Image icon;
    public TextMeshProUGUI placeholderText;

    public void SetNewOrgan(string newOrgan)
    {
        panel.SetActive(true);
        placeholderText.text = "Unlocked " + newOrgan + "!";
        icon.sprite = Resources.Load<Sprite>("Organs/" + newOrgan.ToLower());
    }
}
