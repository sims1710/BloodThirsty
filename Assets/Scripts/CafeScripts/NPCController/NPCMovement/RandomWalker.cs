using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class RandomWalker : MonoBehaviour
{
    public Node nodePrefab;
    public List<Node> nodeList;
    public GameObject[] npcPrefabs;
    //private NPCController vampire;
    private bool canDrawGizmos;
    public Tilemap ground;
    public Tilemap wall;
    public SpawnSettings spawnSettings;
    public Transform spawnLocation;
    private Coroutine spawnCoroutine;
    public UnlockedItems unlockedItems;
    [SerializeField] private InstructionsTracker instructionsTracker;

    void Start()
    {
        nodeList = new List<Node>();
        spawnSettings.ResetWithCustomMax(20);
        UpdateTotalVampires();
        CreateNodes();
        CreateConnections();
    }

    void CreateNodes()
    {
        BoundsInt bounds = ground.cellBounds;
        
        // loop through all tiles in the ground tilemap
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                // if there is ground tile
                if (ground.HasTile(tilePos))
                {
                    //if no game objects with colliders at this position
                    Vector2 worldPosition = ground.CellToWorld(tilePos) + new Vector3(0.5f, 0.5f, 0);
                    // if there are no wall tiles or decoration tiles and no overlap with gameobject
                    if (Physics2D.OverlapPoint(worldPosition)== null)
                    {
                        // create node at this position
                        Node node = Instantiate(nodePrefab, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                        node.transform.parent = transform; 
                        nodeList.Add(node);
                    }
                }
            }
        }
    }
    
    void CreateConnections()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            for (int j = i+1; j < nodeList.Count; j++)
            {
                //check if nodes are very close ot each other then connect them together
                if (Vector2.Distance(nodeList[i].transform.position, nodeList[j].transform.position)<= 1.3f)
                {
                    ConnectNodes(nodeList[i], nodeList[j]);
                    ConnectNodes(nodeList[j], nodeList[i]);
                }
            }
        }
        canDrawGizmos = true;
    }

    void ConnectNodes(Node from, Node to)
    {
        if (from == to) {return; }
        from.connections.Add(to);
    }

    IEnumerator SpawnAIRoutine()
    {
        if (nodeList.Count > 0)
        {
            //bool newPlayer = instructionsTracker.GetIsNewPlayerCafe();

            // if (newPlayer)
            // {
            Debug.Log("New Player");
            // 85% vampire NPC
            int npc0 = Mathf.FloorToInt(spawnSettings.maxVampires * 0.85f);
            // 25% unique NPC 2
            int npc1 = spawnSettings.maxVampires - npc0;

            // List with all NPC to spawn
            List<int> npcTypes = new List<int>() { 0, 1 };

            for (int i = 0; i < npc0 +1; i++)
            {
                npcTypes.Add(0);
            }

            // Calculate minimum spacing between two types of NPC
            int minSpacing = Mathf.Max(2, npc0 / npc1);
            for (int i = 0; i < npc1 + 1; i++)
            {
                int position;
                if (i == 0)
                {
                    position = Random.Range(2, minSpacing + 2);
                }
                else
                {
                    int basePosition = npcTypes.LastIndexOf(1) + minSpacing;
                    int maxOffset = Mathf.Min(minSpacing / 2, spawnSettings.maxVampires - basePosition - (npc1 - i - 1));
                    position = basePosition + Random.Range(0, Mathf.Max(1, maxOffset));
                }
                position = Mathf.Min(position, spawnSettings.maxVampires - 1);
                if (position < npcTypes.Count)
                {
                    npcTypes[position] = 1;
                }
            }

            for (int i = 0; i < npcTypes.Count; i++)
            {
                float waitTime = Random.Range(spawnSettings.minSpawnTime, spawnSettings.maxSpawnTime);
                yield return new WaitForSeconds(waitTime);
                int npcType = npcTypes[i];
                // Create the NPC
                GameObject npcObject = Instantiate(npcPrefabs[npcType], spawnLocation.position, Quaternion.identity);
                spawnSettings.currentAmount++;
                NPCController newNPC = npcObject.GetComponent<NPCController>();
                Node closestNode = GetClosestNodeToPosition(spawnLocation.position);
                newNPC.currentNode = closestNode;
            }
            
            // else
            // {
            //     Debug.Log("Old Player");
            //     // 85% vampire NPC
            //     int npc0 = Mathf.FloorToInt(spawnSettings.maxVampires * 0.85f);
            //     // 25% unique NPC 2
            //     int npc1 = spawnSettings.maxVampires - npc0;

            //     // List with all NPCs to spawn
            //     List<int> npcTypes = new List<int>();
            //     for (int i = 0; i < npc0; i++) // Add all npc0
            //     {
            //         npcTypes.Add(0);
            //     }

            //     // Calculate minimum spacing between two types of NPC
            //     int minSpacing = Mathf.Max(2, npc0 / npc1);

            //     for (int i = 0; i < npc1; i++)
            //     {
            //         int position;
            //         if (i == 0)
            //         {
            //             position = Random.Range(2, minSpacing + 2);
            //         }
            //         else
            //         {
            //             int basePosition = npcTypes.LastIndexOf(1) + minSpacing;
            //             int maxOffset = Mathf.Min(minSpacing / 2, spawnSettings.maxVampires - basePosition - (npc1 - i - 1));
            //             position = basePosition + Random.Range(0, Mathf.Max(1, maxOffset));
            //         }
            //         position = Mathf.Min(position, spawnSettings.maxVampires - 1);
            //         if (position < npcTypes.Count)
            //         {
            //             npcTypes[position] = 1;
            //         }
            //         else
            //         {
            //             npcTypes.Add(1);
            //         }
            //     }

            // for (int i = 0; i < npcTypes.Count; i++)
            // {
            //     float waitTime = Random.Range(spawnSettings.minSpawnTime, spawnSettings.maxSpawnTime);
            //     yield return new WaitForSeconds(waitTime);
            //     int npcType = npcTypes[i];
            //     // Create the NPC
            //     GameObject npcObject = Instantiate(npcPrefabs[npcType], spawnLocation.position, Quaternion.identity);
            //     spawnSettings.currentAmount++;
            //     NPCController newNPC = npcObject.GetComponent<NPCController>();
            //     Node closestNode = GetClosestNodeToPosition(spawnLocation.position);
            //     newNPC.currentNode = closestNode;
            // }
            
        }
    }
    void UpdateTotalVampires()
    {
        if (unlockedItems != null)
        {
            int currentUnlockedOrganCount = unlockedItems.GetUnlockedOrgans().Count;
            int newMax = ((currentUnlockedOrganCount - 1) * 5) + spawnSettings.GetMaxVampires();
            spawnSettings.ResetWithCustomMax(newMax);
            Debug.Log("Total number of customers: " + spawnSettings.GetMaxVampires());
        }

    }
    public void StartSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnAIRoutine());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    Node GetClosestNodeToPosition(Vector3 position)
    {
        Node closestNode = null;
        float closestDistance = float.MaxValue;

        foreach (Node node in nodeList)
        {
            float distance = Vector3.Distance(position, node.transform.position);
            if (distance < closestDistance)
            {
                closestNode = node;
                closestDistance = distance;
            }
        }
        return closestNode;
    }

    private void OnDrawGizmos()
    {
        if (canDrawGizmos)
        {
            Gizmos.color = Color.blue;
            for(int i =0; i < nodeList.Count; i++)
            {
                for(int j = 0; j < nodeList[i].connections.Count; j++)
                {
                    Gizmos.DrawLine(nodeList[i].transform.position, nodeList[i].connections[j].transform.position);
                }
            }
        }
    }
}
