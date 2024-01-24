using UnityEngine;

namespace Utage {
    public class InputManager : MonoBehaviour {
        private static InputManager instance;
        public static InputManager Instance { get => instance; private set => instance = value; }
        private PlayerInputMap inputActions;

        private void Awake()
        {
            if (instance != null && instance != this)
                Destroy(this.gameObject);
            else
                instance = this;

            inputActions = new PlayerInputMap();
        }
        private void OnEnable()
        {
            inputActions.Enable();
        }
        private void OnDisable()
        {
            inputActions.Disable();
        }
        public Vector2 GetPlayerMovement()
        {
            return inputActions.PlayerControl.Movement.ReadValue<Vector2>();
        }
        public Vector2 GetMouseDelta()
        {
            return inputActions.PlayerControl.Look.ReadValue<Vector2>();
        }
        public bool GetJumpThisFrame()
        {
            return inputActions.PlayerControl.Jump.triggered;
        }
    }
}
