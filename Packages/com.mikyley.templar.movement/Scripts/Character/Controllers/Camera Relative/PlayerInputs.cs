using UnityEngine;

namespace Templar.Player
{
    public abstract class TemplarPlayerInputs : MonoBehaviour
    {
        [Header("Inputs"), SerializeField] protected float deadzone = 0;

        protected virtual void LeftStick(Vector2 input) { }
        protected virtual void RightStick(Vector2 input) { }
        protected virtual void LeftTrigger(float input) { }
        protected virtual void RightTrigger(float input) { }

        #region Find Input Axis
        protected Vector2 FindInput(Vector2 axes)    //If the player's intention is to move the analog stick all the way in a direction, assist with reaching the maximum axis value
        {
            Vector2 setAxes = axes;
            setAxes.y = SharedFunctions.MakePositive(axes.y, axes.x, MakePositiveOptions.InvertFloatCompared);
            setAxes.x = SharedFunctions.MakePositive(axes.x, axes.y, MakePositiveOptions.InvertFloatCompared);
            //Debug.Log(axis + axis2);
            float Checks(Vector2 axes)
            {
                return axes.x < -0.85f || axes.x > 0.85f ? Mathf.Clamp(axes.x + axes.y, -1, 1) : axes.x;
            }
            return Vector2.ClampMagnitude(new Vector2(Checks(new Vector2(axes.x, setAxes.y)), Checks(new Vector2(axes.y, setAxes.x))), 1);
        }
        #endregion Find Input Axis

        #region Functions called from input system
        //protected override void OnMove(InputAction.CallbackContext context)
        //{
        //    LeftStick(FindInput(context.ReadValue<Vector2>()));
        //}
        #endregion Functions called from input system
    }
}
