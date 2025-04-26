using UnityEngine;

public class DungeonHubController : MonoBehaviour
{
    [Header("Door References")]
    public DungeonPortalController easyDungeonDoor;
    public DungeonPortalController mediumDungeonDoor;
    public DungeonPortalController hardDungeonDoor;

    [Header("Return Door")]
    public ReturnToCafeDoor cafeDoor;

    void Start()
    {
        // Set up the dungeon doors to point to the appropriate scenes
        if (easyDungeonDoor != null)
        {
            easyDungeonDoor.targetScene = "EasyDungeon";
            easyDungeonDoor.dungeonType = DungeonPortalController.DungeonType.Easy;
        }

        if (mediumDungeonDoor != null)
        {
            mediumDungeonDoor.targetScene = "MediumDungeon";
            mediumDungeonDoor.dungeonType = DungeonPortalController.DungeonType.Medium;
        }

        if (hardDungeonDoor != null)
        {
            hardDungeonDoor.targetScene = "HardDungeon";
            hardDungeonDoor.dungeonType = DungeonPortalController.DungeonType.Hard;
        }
    }
}
