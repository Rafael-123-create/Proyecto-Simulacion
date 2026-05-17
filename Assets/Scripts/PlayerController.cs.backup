using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                input.x -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                input.x += 1f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                input.y -= 1f;
            }

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                input.y += 1f;
            }
        }

        if (Gamepad.current != null)
        {
            input += Gamepad.current.leftStick.ReadValue();

            if (Gamepad.current.dpad.left.isPressed)
            {
                input.x -= 1f;
            }

            if (Gamepad.current.dpad.right.isPressed)
            {
                input.x += 1f;
            }

            if (Gamepad.current.dpad.down.isPressed)
            {
                input.y -= 1f;
            }

            if (Gamepad.current.dpad.up.isPressed)
            {
                input.y += 1f;
            }
        }

        Vector3 movement = new Vector3(input.x, input.y, 0f).normalized;
        transform.Translate(movement * speed * Time.deltaTime);
    }
}
