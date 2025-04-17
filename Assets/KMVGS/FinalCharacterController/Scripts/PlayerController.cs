using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEditor.ShaderGraph.Internal;
using System;

namespace KMVGS.FinalCharacterController
{
    [DefaultExecutionOrder(-1)]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance {get;private set;}

        #region Class Variables
        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Camera _playerCamera;
        
        [Header("3rd-Person Camera")]
        public bool isThirdPerson = false;
        public float thirdPersonDistance = 5f;
        public float thirdPersonHeight = 2f;
        public float followSmoothness = 5f;
        
        [Header("Basic Movement")]
        public float runAcceleration = 0.25f;
        public float runSpeed = 4f;
        public float drag = 0.1f;
        public float movingThreshold = 0.01f;

        [Header("Camera Settings")]
        public float lookSenseH = 0.1f;
        public float lookSenseV = 0.1f;
        public float lookLimitV = 89f;
        public float firstPersonHeight = 1.6f;
        public float firstPersonDistance = 1f;

        [Header("Jumping Settings")]
        public float jumpHeight = 2f;
        public float gravity = -9.81f;
        public float groundCheckDistance = 0.2f;

        private PlayerLocInput _playerLocInput;
        private PlayerState _playerState;
        private Vector2 _cameraRotation = Vector2.zero;
        private Vector2 _playerTargetRotation = Vector2.zero;
        private Vector3 _velocity;
         private bool _isGrounded;
        #endregion
    
        #region Startup
      private void Awake()
        {
            // Singleton implementation
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _playerLocInput = GetComponent<PlayerLocInput>();
            _playerState = GetComponent<PlayerState>();
            _playerLocInput.Player.SwitchCamera.performed += ctx => ToggleCameraMode();
            
            // Lock and hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        #endregion
    
        #region Update Logic
        private void Update()
        {
            UpdateMovementState();
            HandleLateralMovement();
            HandleJumping();
        }
        #endregion
    
        #region Late Update Logic
        private void LateUpdate()
        {
            // Handle camera rotation
            _cameraRotation.x += lookSenseH * _playerLocInput.LookInput.x;
            _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocInput.LookInput.y, -lookLimitV, lookLimitV);
            _playerTargetRotation.x += lookSenseH * _playerLocInput.LookInput.x;
            transform.rotation = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
            
            // Apply rotation to camera
            _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);

            // Handle camera position
            if (isThirdPerson)
            {
                Vector3 desiredPosition = transform.position - _playerCamera.transform.forward * thirdPersonDistance;
                desiredPosition.y += thirdPersonHeight;

                // Wall collision avoidance
                if (Physics.SphereCast(transform.position, 0.3f, (desiredPosition - transform.position).normalized, 
                    out RaycastHit hit, thirdPersonDistance, ~LayerMask.GetMask("Player")))
                {
                    desiredPosition = hit.point + hit.normal * 0.3f;
                }

                _playerCamera.transform.position = Vector3.Lerp(
                    _playerCamera.transform.position, 
                    desiredPosition, 
                    followSmoothness * Time.deltaTime);
            }
            else // 1st-Person
            {
                _playerCamera.transform.localPosition = new Vector3(0f, firstPersonHeight, firstPersonDistance);
            }
        }    
        #endregion
    
        #region Movement
        private void HandleLateralMovement()
        {
            Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;

            Vector3 movementDirection = cameraRightXZ * _playerLocInput.MovementInput.x + 
                                        cameraForwardXZ * _playerLocInput.MovementInput.y;

            // Calculate target velocity (not multiplied by Time.deltaTime)
            Vector3 targetVelocity = movementDirection * runSpeed;
            
            // Apply acceleration to current velocity
            _velocity.x = Mathf.Lerp(_velocity.x, targetVelocity.x, runAcceleration * Time.deltaTime);
            _velocity.z = Mathf.Lerp(_velocity.z, targetVelocity.z, runAcceleration * Time.deltaTime);
            
            // Apply drag only when no input is being given
            if (_playerLocInput.MovementInput.magnitude < 0.1f)
            {
                _velocity.x *= (1 - drag * Time.deltaTime);
                _velocity.z *= (1 - drag * Time.deltaTime);
                
                // Stop completely when velocity is very small
                if (Mathf.Abs(_velocity.x) < 0.01f) _velocity.x = 0;
                if (Mathf.Abs(_velocity.z) < 0.01f) _velocity.z = 0;
            }

            // Move the character (only multiply by Time.deltaTime here)
            _characterController.Move(_velocity * Time.deltaTime);
        }
        private bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);
            return lateralVelocity.magnitude > movingThreshold;
        }
        #endregion

        #region Jumping
        private void HandleJumping()
        {
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);

            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            if (_playerLocInput.InteractPressed && _isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            _velocity.y += gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }
        #endregion

        #region State Management
        private void UpdateMovementState()
        {
            bool isMovingLaterally = IsMovingLaterally();
            PlayerMovementState lateralState = isMovingLaterally ? 
                PlayerMovementState.Running : PlayerMovementState.Idling;
            _playerState.SetPlayerMovementState(lateralState);
        }
        #endregion

        #region Camera Control
        private void ToggleCameraMode()
        {
            isThirdPerson = !isThirdPerson;
            
            // Reset camera position when switching modes
            if (!isThirdPerson)
            {
                _playerCamera.transform.localPosition = new Vector3(0f, firstPersonHeight, 0f);
            }
        }
        #endregion
    }
}