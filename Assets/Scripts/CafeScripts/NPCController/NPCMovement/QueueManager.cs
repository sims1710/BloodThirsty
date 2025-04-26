using System.Collections.Generic;
using UnityEngine;

public class QueueManager : MonoBehaviour
{
    public Transform cashierPosition;
    public Transform barPosition;
    public float barSpacing = 6f;
    public float spaceBtwQueue = 5.5f;
    private List<NPCController> vampireQueue = new List<NPCController>();
    private List<NPCController> queueOrderByArrival = new List<NPCController>();
    private List<NPCController> barQueue = new List<NPCController>();
    private bool isProcessingQueue = false;
    private bool isPlayerInCollision = false;

    public void RegisterAtQueue(NPCController vampire)
    {
        if (!queueOrderByArrival.Contains(vampire))
        {
            queueOrderByArrival.Add(vampire);
            SortQueueByOrder();
        }
    }

    private void SortQueueByOrder()
    {
        List<NPCController> sortedQueue = new List<NPCController>();
        foreach (NPCController vampire in queueOrderByArrival)
        {
            if (vampireQueue.Contains(vampire))
            {
                sortedQueue.Add(vampire);
            }
        }

        foreach (NPCController vampire in vampireQueue)
        {
            if (!sortedQueue.Contains(vampire))
            {
                sortedQueue.Add(vampire);
            }   
        }
        //replace queue with sorted queue
        vampireQueue = sortedQueue;
        UpdateQueuePosition();
    }

    public void AddToQueue(NPCController vampire)
    {
        vampireQueue.Add(vampire);
        //if vampire reach queue area before check if queue is properly sorted
        if (queueOrderByArrival.Contains(vampire))
        {
            SortQueueByOrder();
        }
        else
        {
            UpdateQueuePosition();
        }
    }

    public int ProcessQueue()
    {
        int totalEarnings = 0;
        if (!isProcessingQueue && vampireQueue.Count > 0)
        {
            NPCController vampire = vampireQueue[0];
            vampireQueue.RemoveAt(0);
            //add drink price to total bitecoin
            if (vampire.HasDrink())
            {
                totalEarnings += vampire.GetDrinkPrice();
            }
            vampire.LeaveQueue();
            Invoke(nameof(FinishProcessingQueue), 0.5f);
        }
        return totalEarnings;
    }

    private void FinishProcessingQueue()
    {
        isProcessingQueue = false;
        UpdateQueuePosition();
    }

    private void UpdateQueuePosition()
    {
        int index = 0;
        foreach (NPCController vampire in vampireQueue)
        {
            Vector3 queuePosition = cashierPosition.position + new Vector3(0, -spaceBtwQueue - index, 0);
            vampire.SetQueuePosition(queuePosition);
            index++;
        }
    }

    public void AddToBarQueue(NPCController vampire)
    {
        barQueue.Add(vampire);
        UpdateBarQueuePosition();
    }

    public void RemoveFromBarQueue(NPCController vampire)
    {
        barQueue.Remove(vampire);
        UpdateBarQueuePosition();
    }

    // private void UpdateBarQueuePosition()
    // {
    //     int index = 0;
    //     foreach (NPCController vampire in barQueue)
    //     {
    //         Vector3 barQueuePosition = barPosition.position + new Vector3(2 * index - barSpacing, 0, 0);
    //         Debug.Log("Bar Queue Position: " + barQueuePosition);
    //         vampire.SetBarQueuePosition(barQueuePosition);
    //         index++;
    //     }
    // }
    private void UpdateBarQueuePosition()
    {
        for (int i = 0; i < barQueue.Count; i++)
        {
            // Assign positions based on index in the queue
            Vector3 barQueuePosition = barPosition.position + new Vector3(2 * i - barSpacing, 0, 0);
            Debug.Log($"Updating bar queue position for NPC {i}: {barQueuePosition}");
            barQueue[i].SetBarQueuePosition(barQueuePosition);
        }
    }

    public bool QueueNotEmpty()
    {
        foreach (NPCController vampire in vampireQueue)
        {
            if (vampire.HasReachedQueuePosition())
            {
                return true;
            }
        }
        return false;
    }
}