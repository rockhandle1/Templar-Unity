using UnityEngine;
using UnityEngine.InputSystem;
using Templar.Player;

public class ExamplePlayer : Templar_CameraRelativePlayer
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnMove(InputAction.CallbackContext context)
    {
        LeftStick(context.ReadValue<Vector2>());
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
