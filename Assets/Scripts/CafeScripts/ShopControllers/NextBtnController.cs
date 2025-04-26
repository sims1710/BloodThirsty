using UnityEngine;

public class NextBtnController : MonoBehaviour
{
    public GameObject cardDisplay1;
    public GameObject cardHide1, cardHide2, cardHide3;
    public GameObject previousBtn;

    public void onClick()
    {
        cardHide1.SetActive(false);
        cardHide2.SetActive(false);
        cardHide3.SetActive(false);
        cardDisplay1.SetActive(true);
        previousBtn.SetActive(true);
        gameObject.SetActive(false);
    }
}
