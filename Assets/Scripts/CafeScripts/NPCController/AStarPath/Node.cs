using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Node : MonoBehaviour
{
    public Node cameFromNode;
    public List<Node> connections;
    public float gScore;
    public float hScore;
    public float fScore()
    {
        return gScore + hScore;
    }

    //checking if the nodes are connected properly
    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     if (connections.Count > 0)
    //     {
    //         for (int i = 0; i < connections.Count; i++)
    //         {
    //             Gizmos.DrawLine(transform.position, connections[i].transform.position);
    //         }
    //     }
    // }
}
