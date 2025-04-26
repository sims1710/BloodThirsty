using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject[] roomPrefabs;
    public GameObject finalRoomPrefab;
    public GameObject startingRoomPrefab;
    public GameObject doorPrefab;
    public GameObject player;

    private GameObject roomToUse;

    public GameObject[] enemies;
    //public Camera mainCamera;
    public int dungeonSize = 10;
    public float roomSize = 20f;

    private List<Room> rooms = new List<Room>();
    private List<Door> doors = new List<Door>();
    //public Room currentRoom;

    public int level = 1;

    public int minEnemies;
    public int maxEnemies;

    public GameObject portal;

    void Start()
    {
        GenerateDungeon();
        PlacePlayerInFirstRoom();
        //SetupCamera();
    }

    void GenerateDungeon()
    {
        rooms.Clear();
        doors.Clear();
        Room firstRoom = CreateRoom(Vector2.zero, startingRoomPrefab, -1);
        rooms.Add(firstRoom);
        //currentRoom = firstRoom;

        if (level == 1)
        {
            dungeonSize = Random.Range(5, 8);
        }
        else if (level == 2)
        {
            dungeonSize = Random.Range(9, 12);
        }
        else
        {
            dungeonSize = Random.Range(10, 14);
        }

        for (int i = 1; i < dungeonSize; i++)
        {
            if (i == dungeonSize - 1)
                CreateRoomAtRandomPosition(true);
            else
                CreateRoomAtRandomPosition(false);
        }

        ConnectRooms();
    }

    Room CreateRoom(Vector2 position, GameObject roomPrefab, int numSpawnEnemies)
    {
        GameObject roomObj = Instantiate(roomPrefab, position, Quaternion.identity);
        Room room = roomObj.AddComponent<Room>();
        room.SetPosition(position, roomSize);

        if (roomPrefab == finalRoomPrefab)
        {
            Instantiate(portal, position, Quaternion.identity);
        }

        for (int i = 0; i <= numSpawnEnemies; i++)
        {
            float xPos = Random.Range(room.Position.x, room.Position.x + roomSize - 20);
            float yPos = Random.Range(room.Position.y, room.Position.y + roomSize - 20);

            Vector3 randomPos = new Vector3(xPos, yPos, 0);

            if (room.ContainsPoint(randomPos))
            {
                GameObject enemyToSpawn = enemies[Random.Range(0, enemies.Length)];
                Instantiate(enemyToSpawn, randomPos, Quaternion.identity);

                Debug.Log("Enemy spawned");
            }
        }
        return room;
    }

    void CreateRoomAtRandomPosition(bool isLast)
    {
        bool roomCreated = false;
        Vector2 randomPosition = Vector2.zero;

        for (int attempt = 0; attempt < 100; attempt++)
        {
            int randomDirection = Random.Range(0, 4);
            Room existingRoom = rooms[Random.Range(0, rooms.Count)];

            //try spawning in a random direction
            switch (randomDirection)
            {
                case 0:
                    randomPosition = new Vector2(existingRoom.Position.x - roomSize, existingRoom.Position.y);
                    break;
                case 1:
                    randomPosition = new Vector2(existingRoom.Position.x + roomSize, existingRoom.Position.y);
                    break;
                case 2:
                    randomPosition = new Vector2(existingRoom.Position.x, existingRoom.Position.y + roomSize);
                    break;
                case 3:
                    randomPosition = new Vector2(existingRoom.Position.x, existingRoom.Position.y - roomSize);
                    break;
            }

            randomPosition = new Vector2(Mathf.Round(randomPosition.x / roomSize) * roomSize,
                                         Mathf.Round(randomPosition.y / roomSize) * roomSize);

            if (!IsPositionOccupied(randomPosition))
            {
                roomCreated = true;
                break;
            }
        }

        if (roomCreated)
        {
            Room newRoom;
            if (isLast)
            {
                newRoom = CreateRoom(randomPosition, finalRoomPrefab, 0);
            }
            else
            {
                int numEnemies = Random.Range(minEnemies, maxEnemies);
                roomToUse = roomPrefabs[(Random.Range(0, roomPrefabs.Length))];
                newRoom = CreateRoom(randomPosition, roomToUse, numEnemies);
            }

            rooms.Add(newRoom);
        }
        else
        {
            Debug.LogWarning("100 Attempts still cannot! Only got 4 sides! How did this happen???");
        }
    }

    bool IsPositionOccupied(Vector2 position)
    {
        foreach (Room room in rooms)
        {
            if (Vector2.Distance(room.Position, position) < roomSize)
            {
                return true;
            }
        }
        return false;
    }

    void ConnectRooms()
    {
        HashSet<string> connectedPairs = new HashSet<string>();

        foreach (var room in rooms)
        {
            Vector2 left = room.Position + new Vector2(-roomSize, 0);
            Vector2 right = room.Position + new Vector2(roomSize, 0);
            Vector2 up = room.Position + new Vector2(0, roomSize);
            Vector2 down = room.Position + new Vector2(0, -roomSize);

            foreach (var otherRoom in rooms)
            {
                if (room == otherRoom) continue;

                Vector2 otherPos = otherRoom.Position;

                if (otherPos == left || otherPos == right || otherPos == up || otherPos == down)
                {
                    string pairKey = GeneratePairKey(room.Position, otherRoom.Position);

                    if (!connectedPairs.Contains(pairKey))
                    {
                        ConnectTwoRooms(room, otherRoom);
                        connectedPairs.Add(pairKey);
                    }
                }
            }
        }
    }

    string GeneratePairKey(Vector2 a, Vector2 b)
    {
        string keyA = $"{a.x},{a.y}";
        string keyB = $"{b.x},{b.y}";

        return string.CompareOrdinal(keyA, keyB) < 0 ? keyA + "|" + keyB : keyB + "|" + keyA;
    }

    void ConnectTwoRooms(Room roomA, Room roomB)
    {
        Vector2 direction = roomB.Position - roomA.Position;

        //if they are horizontally connected
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            Vector2 doorPosition = new Vector2((roomA.Position.x + roomB.Position.x) / 2, roomA.Position.y);
            CreateDoor(roomA, roomB, doorPosition, Vector2.right);
        }
        //if they are vertically connected
        else
        {
            Vector2 doorPosition = new Vector2(roomA.Position.x, (roomA.Position.y + roomB.Position.y) / 2);
            CreateDoor(roomA, roomB, doorPosition, Vector2.up);
        }
    }

    void CreateDoor(Room roomA, Room roomB, Vector2 doorPosition, Vector2 direction)
    {
        GameObject doorObj = Instantiate(doorPrefab, doorPosition, Quaternion.identity);
        Door door = doorObj.AddComponent<Door>();
        doors.Add(door);

        door.SetOrientation(direction);
        door.SetConnectedRooms(roomA, roomB);

        float offset = 2.8f;
        Vector3 pointA = doorPosition - (Vector2)(direction * offset);
        Vector3 pointB = doorPosition + (Vector2)(direction * (0.5f + offset));

        door.SetTeleportPoints(pointA, pointB);

        roomA.AddDoor(door);
        roomB.AddDoor(door);
    }



    void PlacePlayerInFirstRoom()
    {
        player.transform.position = new Vector3(rooms[0].Position.x, rooms[0].Position.y, 0f);
    }

    /*void SetupCamera()
    {
        mainCamera.transform.position = new Vector3(currentRoom.Position.x, currentRoom.Position.y, -10f);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = roomSize / 2f;
    }*/

}
