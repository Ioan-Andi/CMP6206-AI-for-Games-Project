

using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 2f;

    [Header("Mouse")]
    public float mouseSensitivity = 0.1f;

    private float rotationX;
    private float rotationY;

    void Start()
    {
        rotationX = transform.eulerAngles.x;
        rotationY = transform.eulerAngles.y;

        LockCursor();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard == null || mouse == null)
            return;

        // Mouse look
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();

            rotationY += mouseDelta.x * mouseSensitivity;
            rotationX -= mouseDelta.y * mouseSensitivity;

            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            transform.rotation =
                Quaternion.Euler(rotationX, rotationY, 0f);
        }

        // Movement input
        Vector3 direction = Vector3.zero;

        if (keyboard.wKey.isPressed)
            direction += transform.forward;

        if (keyboard.sKey.isPressed)
            direction -= transform.forward;

        if (keyboard.aKey.isPressed)
            direction -= transform.right;

        if (keyboard.dKey.isPressed)
            direction += transform.right;

        if (keyboard.spaceKey.isPressed)
            direction += Vector3.up;

        if (keyboard.leftCtrlKey.isPressed)
            direction -= Vector3.up;

        direction = Vector3.ClampMagnitude(direction, 1f);

        // Sprint
        float speed = moveSpeed;

        if (keyboard.leftShiftKey.isPressed)
            speed *= sprintMultiplier;

        // Apply movement
        transform.position +=
            direction * speed * Time.deltaTime;

        // Unlock cursor
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Lock cursor
        if (mouse.leftButton.wasPressedThisFrame &&
            Cursor.lockState == CursorLockMode.None)
        {
            LockCursor();
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
