using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathFindingManager : MonoBehaviour
{
    public static PathFindingManager instance;

    void Awake()
    {
        instance = this;
    }

    public List<Node> GeneratePath(Node start, Node end)
    {
        List<Node> openList = new List<Node>();
        foreach(Node n in FindObjectsOfType<Node>())
        {
            n.gScore = float.MaxValue;
        }
        start.gScore = 0;
        start.hScore = Vector3.Distance(start.transform.position, end.transform.position);
        openList.Add(start);

        while (openList.Count > 0)
        {
            int lowestF = 0;
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fScore() < openList[lowestF].fScore())
                {
                    lowestF = i;
                }
            }

            Node currentNode = openList[lowestF];
            openList.Remove(currentNode);

            //check if current node is the end node, if yes then is the optimal path
            if(currentNode == end)
            {
                List<Node> path = new List<Node>();
                path.Insert(0, end);
                while (currentNode != start)
                {
                    currentNode = currentNode.cameFromNode;
                    path.Add(currentNode);
                }
                path.Reverse();
                Debug.Log("Path generated successfully, path count: " + path.Count);
                return path;
            }

            foreach (Node connectedNode in currentNode.connections)
            {
                float heldGScore = currentNode.gScore + Vector2.Distance(currentNode.transform.position, connectedNode.transform.position);
                if (heldGScore < connectedNode.gScore)
                {
                    connectedNode.cameFromNode = currentNode;
                    connectedNode.gScore = heldGScore;
                    connectedNode.hScore = Vector2.Distance(connectedNode.transform.position, end.transform.position);
                    if (!openList.Contains(connectedNode))
                    {
                        openList.Add(connectedNode);
                    }
                }
            }
        }
        return null;
    }
}