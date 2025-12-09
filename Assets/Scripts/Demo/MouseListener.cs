using System;
using System.Numerics;
using NathanThus.PerfectAim.Demo.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace NathanThus.PerfectAim.Demo
{
    public class MouseListener : MonoBehaviour
    {
        InputAction _clickListener = null;
        InputAction _locationListener = null;
        InputActionsDemo _inputActions;

        [SerializeField] private Transform _targettingObject;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Camera _camera;
        [SerializeField] private PerfectAim _perfectAim;
        [SerializeField] private ProjectileLauncher _launcher;
        [SerializeField] private Vector3 _velocityVector;

        void Start()
        {
            _inputActions = new InputActionsDemo();
            
            _clickListener = _inputActions.Player.Attack;
            _clickListener.performed += HandleClick;
            _clickListener.Enable();

            _locationListener = _inputActions.Player.Look;
            _locationListener.Enable();
        }

        void OnDisable()
        {
            _clickListener.performed -= HandleClick;
            _clickListener.Disable();
            _locationListener.Disable();

        }

        void OnDestroy()
        {
            _clickListener.performed -= HandleClick;
            _clickListener.Disable();
            _locationListener.Disable();
        }

        private void HandleClick(InputAction.CallbackContext _)
        {
            Ray ray = _camera.ScreenPointToRay(_locationListener.ReadValue<Vector2>());

            if (!Physics.Raycast(ray, out RaycastHit hit, 50f))
            {
                Debug.Log("Invalid Location!");
                return;
            }

            _targettingObject.position = new Vector3(hit.point.x, 0, hit.point.z);
            _rigidbody.linearVelocity = _velocityVector;

            Vector3 launchVelocity = _perfectAim.CalculateVelocity(_targettingObject.position, _rigidbody.linearVelocity);

            Debug.Log(launchVelocity);
            
            _perfectAim.ShowArc(launchVelocity);
            _launcher.LaunchProjectile(launchVelocity);
        }
    }
}