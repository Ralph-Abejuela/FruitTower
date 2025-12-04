using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Drag your Player here

    [Header("Settings")]
    public float smoothTime = 0.2f; // How "laggy" the camera is (0 is instant, 0.5 is slow)
    public Vector3 offset = new Vector3(0f, 2f, -10f); // Adjusts position relative to player

    [Header("Jump King Tower Mode")]
    [Tooltip("If true, the camera moves Up/Down but stays centered horizontally.")]
    public bool lockXAxis = true;

    private Vector3 velocity = Vector3.zero;

    // LateUpdate is used for cameras to ensure the player has finished moving for the frame
    private void LateUpdate()
    {
        if (target == null) return;

        // 1. Determine the goal position based on the player's position + offset
        Vector3 targetPosition = target.position + offset;

        // 2. If 'Tower Mode' is on, ignore the player's X movement and keep the camera's current X
        if (lockXAxis)
        {
            targetPosition.x = transform.position.x;
        }

        // 3. Smoothly move from current position to target position
        // SmoothDamp is better than Lerp for cameras because it handles velocity better
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
