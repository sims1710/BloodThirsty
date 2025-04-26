using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class BarController : MonoBehaviour, IInteractable
{
    private bool isPlayerInCollision = false; 
    [SerializeField]
    private OrderValidator orderValidator;
    [SerializeField]
    private GenerateOrder generateOrder;
    public BiteCoins biteCoins;
    public GameObject indicator;

    [SerializeField]

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInCollision = true;
            indicator.SetActive(true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInCollision = false;
            indicator.SetActive(false);
        }
    }

    public void Interact()
    {
       if (isPlayerInCollision)
       {
           // if there no order
           Order currentOrder = generateOrder.GetCurrentOrder();
           if (currentOrder == null)
           {
               Debug.Log("No current order to validate.");
               return;
           }

           // if there is an order then check the order with the mixed drink
           MixerController mixerController = FindObjectOfType<MixerController>();  
           if (mixerController.HasDrink())
           {
                List<string> mixedItems = mixerController.GetMixedItems();
                //validate order
                bool drinkValid = orderValidator.ValidateOrder(currentOrder, mixedItems);
                if (drinkValid)
                {
                    GameObject drink = mixerController.GetCurrentDrink();
                    int drinkPrice = mixerController.GetDrinkPrice();
                    NPCController npc = FindNPC2WaitingForOrder();
                    npc.ReceiveDrink(drink, drinkPrice);
                    mixerController.ClearDrink();
                    generateOrder.ClearCurrentOrder();
                    Debug.Log("Drink valid.");
                } 
                else
                {
                        Debug.Log("Check failed.");
                }
            }
            else
            {
                Debug.Log("No drink to validate.");
            }
       }
       else
       {
            Debug.Log("Player did not collide");
       }
    }

    public NPCController FindNPC2WaitingForOrder()
    {
        NPCController[] npcs = FindObjectsOfType<NPCController>();
        
        foreach (NPCController npc in npcs)
        {
            Debug.Log(npc.CurrentState);
            if (npc.CompareTag("NPC2") && npc.CurrentState == NPCState.JoinBarQueue)
            {
                return npc;
            }
        }
        return null;
    }
}