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

        private PlayerLocInput _playerLocInput;
        private PlayerState _playerState;
        private Vector2 _cameraRotation = Vector2.zero;
        private Vector2 _playerTargetRotation = Vector2.zero;
        #endregion
    
        #region Startup
        private void Awake()
        {
            _playerLocInput = GetComponent<PlayerLocInput>();
            _playerState = GetComponent<PlayerState>();
            _playerLocInput.Player.SwitchCamera.performed += ctx => ToggleCameraMode();
            
            // Lock and hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        #endregion
    
        #region Update Logic
        private void Update()
        {
            UpdateMovementState();
            HandleLateralMovement();
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
                _playerCamera.transform.localPosition = new Vector3(0f, firstPersonHeight, 0f);
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
            
            Vector3 movementDelta = movementDirection * runAcceleration * Time.deltaTime;
            Vector3 newVelocity = _characterController.velocity + movementDelta;

            // Apply drag
            Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
            newVelocity = (newVelocity.magnitude > currentDrag.magnitude) ? 
                newVelocity - currentDrag : Vector3.zero;
            
            newVelocity = Vector3.ClampMagnitude(newVelocity, runSpeed);
            _characterController.Move(newVelocity * Time.deltaTime);
        }

        private bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);
            return lateralVelocity.magnitude > movingThreshold;
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