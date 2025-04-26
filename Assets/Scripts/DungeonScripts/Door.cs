using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Room roomA;
    private Room roomB;

    private Transform pointA; 
    private Transform pointB; 

    private bool teleporting = false;

    public void SetConnectedRooms(Room room1, Room room2)
    {
        roomA = room1;
        roomB = room2;
    }

    public void SetTeleportPoints(Vector3 a, Vector3 b)
    {
        pointA = new GameObject("PointA").transform;
        pointA.position = a;
        pointA.parent = transform;

        pointB = new GameObject("PointB").transform;
        pointB.position = b;
        pointB.parent = transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !teleporting)
        {
            teleporting = true;

            float distToA = Vector2.Distance(other.transform.position, pointA.position);
            float distToB = Vector2.Distance(other.transform.position, pointB.position);

            Debug.Log("Door: " + distToA + distToB);

            if (distToA < distToB)
            {
                other.transform.position = pointB.position;
            }
            else
            {
                other.transform.position = pointA.position;
            }

            StartCoroutine(ResetTeleport());
        }
    }

    private IEnumerator ResetTeleport()
    {
        yield return new WaitForSeconds(0.5f);
        teleporting = false;
    }

    public void SetOrientation(Vector2 direction)
    {
        if (direction == Vector2.right)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (direction == Vector2.up)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
