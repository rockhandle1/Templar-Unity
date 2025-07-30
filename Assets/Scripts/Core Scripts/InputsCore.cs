using UnityEngine;
using UnityEngine.InputSystem;

namespace Templar.Inputs
{
    public class InputsCore : MonoBehaviour
    {
        protected static InputSystem_Actions _inputActions { get; private set; }
        bool _initialized = false;

        private void OnEnable()
        {
            #region Initializer
            if (!_initialized)
            {
                _inputActions = _inputActions ?? new InputSystem_Actions();

                _inputActions.Player.Move.performed += OnMove;
                _inputActions.Player.Move.canceled += OnMove;
                _inputActions.Player.Sprint.performed += OnSprint;
                _inputActions.Player.Interact.performed += OnInteract;
                _inputActions.Player.Attack.performed += OnAttack;

                _initialized = true;
            }
            #endregion Initializer

            _inputActions.Player.Enable();

            EnabledTasks();
        }
        private void OnDisable()
        {
            _inputActions.Player.Disable();

            DisabledTasks();
        }

        #region Replacement enable/disable functions
        protected virtual void EnabledTasks() { }
        protected virtual void DisabledTasks() { }
        #endregion Replacement enable/disable functions

        #region Functions called from input system
        protected virtual void OnInteract(InputAction.CallbackContext context) { }
        protected virtual void OnAttack(InputAction.CallbackContext context) { }
        protected virtual void OnMove(InputAction.CallbackContext context) { }
        protected virtual void OnSprint(InputAction.CallbackContext context) { }
        #endregion Functions called from input system
    }

}