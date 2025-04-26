using UnityEngine;
using System.Collections;

public class InstructionCafePanelController : MonoBehaviour
{
    public InstructionsTracker instructionsTracker;
    private bool isNewPlayer;

    // Starting Panels
    public GameObject startPanel;
    public GameObject secondPanel;
    public GameObject thirdPanel;

    // Blood Storage Panel
    public GameObject bloodStoragePanel;
    private BloodStorageManager bloodStorageManager;
    private bool bloodStorageFound = false;

    // Organ Storage Panel
    public GameObject organStoragePanel;
    private OrganStorageManager organStorageManager;
    private bool organStorageFound = false;

    // Mix Panel
    public GameObject forthPanel;
    private HeldStorageManager heldStorageManager;
    private bool mixerShown = false;

    // Bar Table Panel
    public GameObject fifthPanel;
    private bool tableShown = false;

    // Open Close Sign Panel
    public GameObject sixthPanel;
    private DrinkPlacerController[] drinkPlacers;
    private bool drinkOnTable = false;
    private bool signShown = false;

    // Customer Panel
    public GameObject customerPanel;
    private GameObject customer;
    private bool customerShown = false;

    // VIP Customer Panel
    public GameObject specialCustomerPanelP1;
    public GameObject specialCustomerPanelP2;
    private GameObject specialCustomer;
    private BarController barController;
    private bool specialCustomerShownP1 = false;
    private bool specialCustomerShownP2 = false;

    // Cashier Panel
    public GameObject seventhPanel;
    private QueueManager queueManager;
    private bool cashierShown = false;

    // Price, Time, Bin, End Day Panels
    public GameObject eighthPanel;
    public GameObject ninthPanel;
    public GameObject tenthPanel;
    public GameObject eleventhPanel;
    private bool lastShown = false;


    void Start()
    {
        isNewPlayer = instructionsTracker.GetIsNewPlayerCafe();
        heldStorageManager = FindObjectOfType<HeldStorageManager>();
        drinkPlacers = FindObjectsOfType<DrinkPlacerController>();
        queueManager = FindObjectOfType<QueueManager>();
        barController = FindObjectOfType<BarController>();

        if (isNewPlayer)
        {
            StartCoroutine(ShowStartingPanels());
        }
    }

    void Update()
    {
        if (isNewPlayer)
        {
            // Check for Blood Storage
            bloodStorageManager = FindObjectOfType<BloodStorageManager>();
            if (bloodStorageManager != null)
            {
                if (!bloodStorageFound)
                {
                    bloodStorageFound = true;
                    StartCoroutine(ShowStoragePanel(bloodStoragePanel));
                }
            }

            // Check for Organ Storage
            organStorageManager = FindObjectOfType<OrganStorageManager>();
            if (organStorageManager != null)
            {
                if (!organStorageFound)
                {
                    organStorageFound = true;
                    StartCoroutine(ShowStoragePanel(organStoragePanel));
                }
            }

            // Check for Mixer Panel
            if (!mixerShown && bloodStorageManager == null && organStorageManager == null)
            {
                if (heldStorageManager.organCount > 0 && heldStorageManager.bloodVialCount > 0)
                {
                    mixerShown = true;
                    StartCoroutine(ShowPanel(forthPanel));
                }
            }

            // Check for Tables Panel
            if (!tableShown)
            {
                GameObject player = GameObject.FindWithTag("Player");
                Transform drink = player.transform.Find("Blood(Clone)");
                if (drink != null)
                {
                    tableShown = true;
                    StartCoroutine(ShowPanel(fifthPanel));
                }
            }

            // Check for Sign Panel
            if (!signShown)
            {
                foreach (var drinkPlacer in drinkPlacers)
                {
                    if (drinkPlacer.HasDrinkOnTable())
                    {
                        drinkOnTable = true;
                        break;
                    }
                }
                if (drinkOnTable)
                {
                    signShown = true;
                    StartCoroutine(ShowPanel(sixthPanel));
                }
            }

            // Check for Normal Customer
            customer = GameObject.Find("NPC1(Clone)");
            if (!customerShown)
            {
                if (customer != null)
                {
                    customerShown = true;
                    StartCoroutine(ShowPanel(customerPanel));
                }
            }

            // Check for Special Customer
            specialCustomer = GameObject.Find("NPC2(Clone)");
            if (!specialCustomerShownP1)
            {
                if (specialCustomer != null)
                {
                    specialCustomerShownP1 = true;
                    StartCoroutine(ShowPanel(specialCustomerPanelP1));
                }
            }
            if (!specialCustomerShownP2)
            {
                if (barController.FindNPC2WaitingForOrder() != null)
                {
                    specialCustomerShownP2 = true;
                    StartCoroutine(ShowPanel(specialCustomerPanelP2));
                }
            }

            // Check for Cashier Panel
            if (!cashierShown)
            {
                if (queueManager.QueueNotEmpty())
                {
                    cashierShown = true;
                    StartCoroutine(ShowPanel(seventhPanel));
                }
            }

            if (!lastShown)
            {
                if (cashierShown && !queueManager.QueueNotEmpty() && specialCustomerShownP2 && barController.FindNPC2WaitingForOrder() == null)
                {
                    lastShown = true;
                    StartCoroutine(ShowEndingPanels());
                }
            }
        }
    }

    private IEnumerator ShowStartingPanels()
    {
        Time.timeScale = 0;

        yield return StartCoroutine(ShowPanel(startPanel));
        yield return StartCoroutine(ShowPanel(secondPanel));
        yield return StartCoroutine(ShowPanel(thirdPanel));

        Time.timeScale = 1;
    }

    private IEnumerator ShowStoragePanel(GameObject panelName)
    {
        panelName.SetActive(true);
        yield return new WaitForSecondsRealtime(3f);
        panelName.SetActive(false);
    }

    private IEnumerator ShowPanel(GameObject panelName)
    {
        Time.timeScale = 0;

        panelName.SetActive(true);
        yield return new WaitForSecondsRealtime(3f);
        panelName.SetActive(false);

        Time.timeScale = 1;
    }

    private IEnumerator ShowEndingPanels()
    {
        Time.timeScale = 0;

        yield return StartCoroutine(ShowPanel(eighthPanel));
        yield return StartCoroutine(ShowPanel(ninthPanel));
        yield return StartCoroutine(ShowPanel(tenthPanel));
        yield return StartCoroutine(ShowPanel(eleventhPanel));

        Time.timeScale = 1;
        instructionsTracker.SetIsNewPlayerCafe(false);
    }
}
