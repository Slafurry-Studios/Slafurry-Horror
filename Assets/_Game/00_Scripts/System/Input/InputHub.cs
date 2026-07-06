using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Slafurry.Core.Abstract;

namespace Slafurry.System.InputHub
{

    public static class Controls
    {
        // public static event Action OnJumpPressed
        // {
        //     add => InputHub.Instance.OnJumpPressed += value;
        //     remove => InputHub.Instance.OnJumpPressed -= value;
        // }
        // public static event Action<Vector2> OnMoveChanged
        // {
        //     add => InputHub.Instance.OnMoveChanged += value;
        //     remove => InputHub.Instance.OnMoveChanged -= value;
        // }
    }

    public class InputHub : GameSystem<InputHub>
    {

        [SerializeField] private InputActionAsset inputActions;

        // public event Action OnJumpPressed;
        // public event Action<Vector2> OnMoveChanged;

        public override IEnumerator Initialize() { yield return null; }
        public override void PostInitialize() { }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();

            // var map = inputActions.FindActionMap("Gameplay");
            // _jumpAction = map.FindAction("Jump");
            // _moveAction = map.FindAction("Move");

            // _jumpAction.performed += ctx => OnJumpPressed?.Invoke();
            // _moveAction.performed += ctx => OnMoveChanged?.Invoke(ctx.ReadValue<Vector2>());
            // _moveAction.canceled += ctx => OnMoveChanged?.Invoke(Vector2.zero);

            // map.Enable();
        }
    }
}