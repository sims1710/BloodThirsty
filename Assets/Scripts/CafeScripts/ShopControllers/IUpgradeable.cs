public interface IUpgradeable
{
    void UpgradeNextLevel();
    (float multiplier, int cost, string description) GetNextUpgrade();
    int GetCurrentLevel();
}