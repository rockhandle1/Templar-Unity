using UnityEngine;
using UnityEngine.InputSystem;
using Templar.Player;

public class ExamplePlayer : Templar_CameraRelativePlayer
{
    public void OnMove(InputAction.CallbackContext context)
    {
        base.Movement(context.ReadValue<Vector2>());
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
