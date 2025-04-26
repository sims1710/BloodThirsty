using UnityEngine;

public class UnitRoot : MonoBehaviour
{
    private EnemyController enemyController;

    void Start()
    {
        // Get reference to the EnemyController
        enemyController = GetComponentInParent<EnemyController>();
    }

    // This method will be called by the animation event
    public void OnAttackAnimationEvent()
    {
        Debug.Log("Attack animation event trying to call enemy controller");
        if (enemyController != null)
        {
            enemyController.OnAttackAnimationEvent();
            Debug.Log("Attack animation event");
        }
    }
}