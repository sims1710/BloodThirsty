using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//inherit the NPCPath for npc movement and creation of the path
public class NPCController : NPCPath
{
    private bool hasDrink = false;
    private QueueManager queueManager;
    [SerializeField]
    private GenerateOrder generateOrder;
    [SerializeField]
    private DisplayOrder displayOrder;
    [SerializeField]
    private CafeStatsManager cafeStatsManager;
    private Order currentOrder;
    private bool inQueue = false;
    private bool inBarQueue = false;
    private Vector3 queuePosition;
    private float wanderDuration = 8f;
    private float pickupDuration = 8f;
    private float idleDuration = 10f;
    private float patienceTimer = 0f;
    private float totalPatienceDuration = 0f;
    private Transform tableLocation;
    private int drinkPrice = 0;
    private bool hasReachQueue = false;
    private bool searchforDrink = false;
    [SerializeField] private Slider patienceBar;
    [SerializeField] private Image patienceBarImage;
    [SerializeField] private Gradient patienceGradient;

    protected override void Start()
    {
        //call start from NPCStart
        base.Start();
        queueManager = FindObjectOfType<QueueManager>();
        if (generateOrder == null)
        generateOrder = FindObjectOfType<GenerateOrder>();
        cafeStatsManager = FindObjectOfType<CafeStatsManager>();
        displayOrder = orderBubble.GetComponentInChildren<DisplayOrder>();
        GoToBar();
    }
    //states transition
    private void Update()
    {
        switch (currentState)
        {
            case NPCState.GoingToBar:
                MoveAlongPath();
                if (path.Count == 0 && HasReachedDestination(barLocation.position))
                {
                    currentState = NPCState.WaitingForOrder;
                }
                break;

            case NPCState.PickingUpBottle:
                if (!hasDrink)
                {
                    PickUpDrink();
                }
                else
                {
                    patienceBar.gameObject.SetActive(false);
                    patienceBarImage.gameObject.SetActive(false);
                    queueManager.AddToQueue(this);
                }
                break;

            case NPCState.Wandering:
                orderBubble.SetActive(false);
                totalPatienceDuration = wanderDuration;
                UpdatePatienceBar();
                MoveAlongPath();
                if (gameObject.CompareTag("NPC") && searchforDrink)
                {
                    Transform nearestTable = FindNearestTableWithDrink();
                    if (nearestTable != null)
                    {
                        CreatePath(nearestTable.position);
                        tableLocation = nearestTable;
                        currentState = NPCState.GoingToTable;
                        Debug.Log($"NPC State: {currentState}, Patience Timer: {patienceTimer}");
                    }
                }

                if (patienceTimer >= wanderDuration)
                {
                    patienceBar.gameObject.SetActive(false);
                    patienceBarImage.gameObject.SetActive(false);
                    LeaveCafe();
                    currentState = NPCState.LeaveCafe;
                    Debug.Log($"NPC State: {currentState}, Patience Timer: {patienceTimer}");
                }
                break;

            case NPCState.JoinQueue:
                MoveAlongPath();
                if (!hasReachQueue)
                {
                    hasReachQueue = true;
                    queueManager.RegisterAtQueue(this);
                }
                if (path.Count == 0 && HasReachedDestination(queuePosition))
                {
                    currentState = NPCState.Idle;
                    inQueue = true;
                    //payBubble.SetActive(true);
                }
                break;

            case NPCState.JoinBarQueue:
                MoveAlongPath();
                if (HasReachedDestination(queuePosition))
                {
                    patienceTimer += Time.deltaTime;
                    float remainingPatience = Mathf.Clamp01(1 - (patienceTimer / idleDuration));
                    UpdatePatienceBarValues(remainingPatience);
                    if (patienceTimer >= idleDuration)
                    {
                        queueManager.RemoveFromBarQueue(this);
                        inBarQueue = false;
                        currentState = NPCState.Wandering;
                    }
                }

                break;

            case NPCState.WaitingForOrder:
                orderBubble.SetActive(true);
                if (!inQueue && !hasDrink)
                {
                    if (generateOrder != null)
                    {
                        currentOrder = generateOrder.CreateOrder();

                        if (displayOrder != null)
                        {
                            displayOrder.ShowOrder(currentOrder);
                        }
                        else
                        {
                            Debug.LogError("DisplayOrder script is not assigned to NPC2.");
                        }
                    }
                    else
                    {
                        Debug.LogError("GenerateOrder script is not assigned to NPC2.");
                    }
                    Debug.Log("Adding NPC to bar queue.");
                    queueManager.AddToBarQueue(this);
                    inBarQueue = true;
                }
                break;

            case NPCState.Idle:
                break;

            case NPCState.LeaveCafe:
                MoveAlongPath();
                if (path.Count == 0 && HasReachedDestination(startingLocation.position))
                {
                    Destroy(gameObject);
                }
                break;

            case NPCState.GoingToTable:
                totalPatienceDuration = pickupDuration;
                UpdatePatienceBar();
                MoveAlongPath();
                if (patienceTimer >= pickupDuration)
                {
                    Debug.Log("Patience timer expired. Leaving cafe.");
                    currentState = NPCState.Wandering;
                }
                else if (path.Count == 0 && HasReachedDestination(tableLocation.position))
                {
                    Debug.Log("Reached my table!");
                    currentState = NPCState.PickingUpBottle;
                    searchforDrink = false;
                }
                break;
        }
    }

    private void UpdatePatienceBar()
    {
        // Increment the patience timer
        patienceTimer += Time.deltaTime;

        // Update the patience bar based on the remaining time
        float patienceRatio = Mathf.Clamp01(1 - (patienceTimer / totalPatienceDuration));
        UpdatePatienceBarValues(patienceRatio);
    }

    private void UpdatePatienceBarValues(float patienceRatio)
    {
        patienceBar.value = patienceRatio;
        Color newColor = patienceGradient.Evaluate(patienceRatio);
        patienceBar.fillRect.GetComponent<Image>().color = newColor;
    }

    private void GoToBar()
    {
        if (gameObject.CompareTag("NPC"))
        {
            Transform nearestTable = FindNearestTableWithDrink();
            //if there is a drink
            if (nearestTable != null)
            {
                CreatePath(nearestTable.position);
                tableLocation = nearestTable;
                currentState = NPCState.GoingToTable;
            }
            else
            {
                currentState = NPCState.Wandering;
                searchforDrink = true;
            }

        }
        else if (gameObject.CompareTag("NPC2"))
        {
            CreatePath(barLocation.position);
            currentState = NPCState.GoingToBar;
            
        }
    }

    private void PickUpDrink()
    {
        if (gameObject.CompareTag("NPC2"))
        {
            currentState = NPCState.Wandering;
        }
        else
        {
            if (tableLocation.childCount > 0)
            {
                Transform drink = tableLocation.GetChild(0);
                drink.SetParent(transform);
                drink.localPosition = new Vector3(0.5f, 0.5f, -0.2f);
                hasDrink = true;
                //add to cafe stats manager
                cafeStatsManager.IncrementRedNPCDrinkPickup();

                //get drink price from  drink placer controller
                DrinkPlacerController drinkPlacer = tableLocation.GetComponent<DrinkPlacerController>();
                if (drinkPlacer != null)
                {
                    SetDrinkPrice(drinkPlacer.GetDrinkPrice());
                    cafeStatsManager.AddEarnings(drinkPlacer.GetDrinkPrice());
                }
                //allow table to be empty again
                drinkPlacer.TakeDrink();
            }
            else
            {
                currentState = NPCState.Wandering;
            }
        }
    }

    private Transform FindNearestTableWithDrink()
    {
        Transform nearestTable = null;
        float closestDistance = float.MaxValue;
        foreach (Transform table in allTable)
        {
            //check if table has any drinks on it, if hv the customer will go to the table
            if (table.childCount > 0)
            {
                float distance = Vector3.Distance(transform.position, table.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestTable = table;
                }
            }
            else
            {
                currentState = NPCState.Wandering;
            }
        }
        return nearestTable;
    }

    private void LeaveCafe()
    {
        if (!hasDrink)
        {
            sadBubble.SetActive(true);
        }
        orderBubble.SetActive(false);
        //npc going to starting position and destroy game object
        CreatePath(startingLocation.position);
    }

    public void JoinQueue()
    {
        Debug.Log("Join queue");
        //if not in queue then vampire is added to the queue, and payBubble for paying will be active
        if (!inQueue)
        {
            inQueue = true;
            queueManager.AddToQueue(this);
            //payBubble.SetActive(true);
        }
    }

    public void ReceiveDrink(GameObject drink, int price)
    {
        if (!hasDrink)
        {
            hasDrink = true;
            inBarQueue = false;
            drinkPrice = price * 3;
            SetDrinkPrice(drinkPrice);
            Debug.Log("NPC2 received the drink!");
            //add to cafe stats manager
            cafeStatsManager.IncrementVIPDrinkReceive();
            cafeStatsManager.AddEarnings(drinkPrice);

            // Attach the drink to the NPC
            drink.transform.SetParent(transform);
            drink.transform.localPosition = new Vector3(0.5f, 0.5f, -0.2f);
            JoinQueue();
            // Hide the order bubble
            orderBubble.SetActive(false);
        }
        else
        {
            Debug.Log("NPC2 has a drink.");
        }
    }

    public void SetQueuePosition(Vector3 position)
    {
        //create a path to the queue position and each position is unique
        queuePosition = position;
        CreatePath(queuePosition);
        currentState = NPCState.JoinQueue;
    }

    public void SetBarQueuePosition(Vector3 position)
    {
        if (hasDrink) return;
        //create a path to the queue position and each position is unique
        queuePosition = position;
        CreatePath(queuePosition);
        currentState = NPCState.JoinBarQueue;
    }

    public void LeaveQueue()
    {
        Debug.Log("Leave queue");
        //vampire will leave when payment is accepted
        inQueue = false;
        //payBubble.SetActive(false);
        if (gameObject.CompareTag("NPC2"))
        {
            currentState = NPCState.Wandering;
        }
        else
        {
            CreatePath(tableLocation.position);
            currentState = NPCState.Wandering;
        }
    }

    public bool HasReachedQueuePosition()
    {
        return inQueue && HasReachedDestination(queuePosition);
    }

    //drink prices stored in npc
    public void SetDrinkPrice(int price)
    {
        drinkPrice = price;
    }
    
    public int GetDrinkPrice()
    {
        return drinkPrice;
    }

    public bool HasDrink()
    {
        return hasDrink;
    }

    public NPCState CurrentState
    {
        get { return currentState; }
    }

}
