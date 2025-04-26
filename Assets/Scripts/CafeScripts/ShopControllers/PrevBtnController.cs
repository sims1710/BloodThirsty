using UnityEngine;

public class PrevBtnController : MonoBehaviour
{
    public GameObject cardDisplay1, cardDisplay2, cardDisplay3;
    public GameObject cardHide1;
    public GameObject nextBtn;

    public void onClick()
    {
        cardHide1.SetActive(false);
        cardDisplay1.SetActive(true);
        cardDisplay2.SetActive(true);
        cardDisplay3.SetActive(true);
        nextBtn.SetActive(true);
        gameObject.SetActive(false);
    }
}
