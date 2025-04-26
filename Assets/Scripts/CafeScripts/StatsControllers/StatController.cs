using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatController : MonoBehaviour
{
    public ScriptableObject scriptableObject;
    private int level;
    public TextMeshProUGUI levelText;
    public Image lvl1Bar, lvl2Bar, lvl3Bar;
    private IUpgradeable upgradeable;

    void Start()
    {
        upgradeable = scriptableObject as IUpgradeable;
        level = upgradeable.GetCurrentLevel();
        levelText.text = level.ToString();

        Color activeColor = new Color(0.808f, 0.682f, 0.6118f, 1f);
        Color inactiveColor = new Color(0.37f, 0.31f, 0.27f, 1f);

        lvl1Bar.color = (level >= 1) ? activeColor : inactiveColor;
        lvl2Bar.color = (level >= 2) ? activeColor : inactiveColor;
        lvl3Bar.color = (level == 3) ? activeColor : inactiveColor;
    }

}
