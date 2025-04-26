using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public Vector2 Position { get; private set; }
    private List<Door> doors = new List<Door>();
    private float roomSize;

    public void SetPosition(Vector2 position, float roomSize)
    {
        Position = position;
        this.roomSize = roomSize;
        transform.position = position;
    }

    public void AddDoor(Door door)
    {
        doors.Add(door);
    }

    public bool ContainsPoint(Vector3 point)
    {
        return point.x > Position.x - roomSize / 2f && point.x < Position.x + roomSize / 2f &&
               point.y > Position.y - roomSize / 2f && point.y < Position.y + roomSize / 2f;
    }
}
