using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class NPCPath : MonoBehaviour
{
    public Node currentNode;
    protected NPCState currentState = NPCState.Wandering;
    public List<Node> path = new List<Node>();
    protected PathFindingManager pathFindingManager;
    protected Transform barLocation;
    protected Transform cashierLocation;
    protected Transform startingLocation;
    protected List<Transform> tables;
    protected List<Transform> allTable;
    protected SpriteRenderer spriteRenderer;
    public GameObject payBubble;
    public GameObject sadBubble;
    public GameObject orderBubble;

    //protected is used so tht it can only be accessed by this class or class tht uses it
    //virtual is used so tht it can be overriden in derived classes.
    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        barLocation = GameObject.Find("Bar").transform;
        startingLocation = GameObject.Find("StartingPoint").transform;
        cashierLocation = GameObject.Find("Cashier").transform;
        GameObject[] tableObjects = GameObject.FindGameObjectsWithTag("Table");
        allTable = new List<Transform>();
        foreach (GameObject table in tableObjects)
        {
            allTable.Add(table.transform);
        }
        pathFindingManager = GetComponent<PathFindingManager>();
        payBubble.SetActive(false);
        sadBubble.SetActive(false);
        orderBubble.SetActive(false);
        
        spriteRenderer.sortingLayerName = "Item";
        SetBubbleSortingLayer(payBubble, "Item");
        SetBubbleSortingLayer(sadBubble, "Item");
        SetBubbleSortingLayer(orderBubble, "Item");
    }

    protected void CreatePath(Vector3 destination)
    {
        Node[] nodes = FindObjectsOfType<Node>();
        Node closestNode = null;
        float closestDistance = float.MaxValue;

        foreach (Node node in nodes)
        {
            float distance = Vector3.Distance(node.transform.position, destination);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        if (closestNode != null)
        {
            path = PathFindingManager.instance.GeneratePath(currentNode, closestNode);
        }
    }

    protected void MoveAlongPath()
    {
        if (path.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(path[0].transform.position.x, path[0].transform.position.y, -2), 3 * Time.deltaTime);
            Vector3 targetPosition = new Vector3(path[0].transform.position.x, path[0].transform.position.y, -2);

            if (Vector2.Distance(transform.position, path[0].transform.position) < 0.1f)
            {
                currentNode = path[0];
                path.RemoveAt(0);
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = targetPosition.x < transform.position.x;
            }
        }
        else if (currentState == NPCState.Wandering)
        {
            // Only generate random paths when character is wandering
            GenerateRandomPath();

        }
    }

    protected bool HasReachedDestination(Vector3 destination)
    {
        return Vector3.Distance(transform.position, destination) < 3f;
    }

    protected void GenerateRandomPath()
    {
        Node[] nodes = FindObjectsOfType<Node>();
        if (nodes.Length > 0)
        {
            Node randomNode = nodes[Random.Range(0, nodes.Length)];
            path = PathFindingManager.instance.GeneratePath(currentNode, randomNode);

            if (path == null)
            {
                path = new List<Node>();
            }
        }
    }

    private void SetBubbleSortingLayer(GameObject bubble, string sortingLayerName)
    {
        if (bubble != null)
        {
            SpriteRenderer bubbleRenderer = bubble.GetComponent<SpriteRenderer>();
            if (bubbleRenderer != null)
            {
                bubbleRenderer.sortingLayerName = sortingLayerName;
                bubbleRenderer.sortingOrder = 2;
            }
        }
    }
}
