using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;  // The player to follow
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);  // Camera offset from player

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;  // How smoothly the camera follows
    [SerializeField] private bool followX = true;     // Whether to follow on X axis
    [SerializeField] private bool followY = true;     // Whether to follow on Y axis
    [SerializeField] private bool followZ = false;    // Whether to follow on Z axis

    [Header("Boundaries")]
    [SerializeField] private bool useBoundaries = false;  // Whether to use camera boundaries
    [SerializeField] private float minX = -10f;          // Minimum X position
    [SerializeField] private float maxX = 10f;           // Maximum X position
    [SerializeField] private float minY = -10f;          // Minimum Y position
    [SerializeField] private float maxY = 10f;           // Maximum Y position

    private void Start()
    {
        // If no target is assigned, try to find the player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("Camera target set to Player");
            }
            else
            {
                Debug.LogError("No target assigned to CameraFollow and no GameObject with tag 'Player' found!");
                enabled = false;
                return;
            }
        }

        // Set initial position
        Vector3 desiredPosition = target.position + offset;
        transform.position = desiredPosition;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;

        // Apply axis constraints
        if (!followX) desiredPosition.x = transform.position.x;
        if (!followY) desiredPosition.y = transform.position.y;
        if (!followZ) desiredPosition.z = transform.position.z;

        // Apply boundaries if enabled
        if (useBoundaries)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // Smoothly move camera to desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}