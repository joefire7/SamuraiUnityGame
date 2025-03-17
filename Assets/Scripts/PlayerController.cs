using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RpgCore
{
    public class PlayerController : MonoBehaviour
    {
        public float Speed;
        public float RotationSpeed = 3.12f;
        private Vector3 _Movement;
        private Rigidbody _Rigidbody;
        private Quaternion _Rotation;
        public bool isRotation = false;

        // Xbox controller variables
        public float GamepadRotationSpeed = 100f; // Rotation speed for the right stick

        public Animator PlayerAnimator;
        public SpriteRenderer PlayerSpriteRenderer;
        public string LastAnimationState;

        // Jump field/properties
        public float JumpForce = 5f;    // Force applied for jumping
        public float GroundCheckDistance = 0.2f; // Distance for the raycast
        public LayerMask GroundLayer;    // Set this to the layer(s) representing the ground

        [SerializeField]
        private bool _IsGrounded;

        [SerializeField]
        private bool _IsFrontSide;

        [SerializeField]
        private bool _IsBackSide;

        [SerializeField]
        private bool _IsLeftSide;

        [SerializeField]
        private bool _IsRightSide;

        public bool IsAttack
        {
            get => _IsAttack1;
            set => _IsAttack1 = value;
        }
        [SerializeField]
        private bool _IsAttack1;

        [SerializeField]
        private bool _IsIdle;

        [SerializeField]
        private bool _CharacterIsMoving;

        public GameObject VFXSlashLight;

        public ParticleSystem SlashVFXAttack1
        {
            get => slashVFXAttack1;
            set => slashVFXAttack1 = value;
        }
        [SerializeField]
        private ParticleSystem slashVFXAttack1;

        private Vector3 leftSidePosition = new Vector3(-0.28f, -0.2726f, 0);  // Target position for left side
        private Vector3 rightSidePosition = new Vector3(0.37f, -0.2726852f, 0);  // Target position for right side

        private Vector3 leftSideRotation = new Vector3(360, 180, 360);  // Rotation for left side
        private Vector3 rightSideRotation = new Vector3(180, 0, 360);  // Rotation for right side

        public float moveSpeed = 5f;  // Speed at which the particle system moves
        public float rotationSpeed = 200f;  // Speed at which the particle system rotates

        private bool isMoving = false;
        private Vector3 targetPosition;
        private Quaternion targetRotation;

        private float lastHorizontalDirection = 0;

        // Initialize Rigidbody in Awake
        void Awake()
        {
            _Rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            LastAnimationState = "CharacterRunFront";
        }



        void FixedUpdate()
        {
            // Check if player is grounded
            _IsGrounded = IsGrounded();
            PlayerAnimator.SetBool("IsGrounded", _IsGrounded);
            PlayerAnimator.SetBool("IsAttack1", _IsAttack1);

            // Keyboard and Gamepad Movement Input (Left Stick)
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            if (Mathf.Abs(horizontalInput) < 0.2f) horizontalInput = 0;
            if (Mathf.Abs(verticalInput) < 0.2f) verticalInput = 0;

            // For gamepad or keyboard input (left stick)
            _Movement = new Vector3(horizontalInput, 0, verticalInput);
            _Movement.Normalize();

            // Rotation using right stick (for gamepad)
            float rightStickHorizontal = Input.GetAxis("RightStickHorizontal");  // Check Input Manager for exact name
            float rightStickVertical = Input.GetAxis("RightStickVertical");      // Check Input Manager for exact name
                                
            // Check joystick direction and log it
            LogJoystickDirection(horizontalInput, verticalInput);
          

            // Apply rotation based on gamepad right stick
            if (Mathf.Abs(rightStickHorizontal) > 0.1f || Mathf.Abs(rightStickVertical) > 0.1f)
            {
                Vector3 rightStickDirection = new Vector3(rightStickHorizontal, 0, rightStickVertical).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(rightStickDirection);
                //_Rigidbody.MoveRotation(Quaternion.Slerp(_Rigidbody.rotation, targetRotation, Time.fixedDeltaTime * GamepadRotationSpeed));
            }
            else if (_Movement.magnitude > 0 && isRotation)
            {
                // Rotate towards movement direction for keyboard or gamepad (left stick)
                Vector3 desiredForward = Vector3.RotateTowards(
                    transform.forward,
                    _Movement,
                    Time.fixedDeltaTime * RotationSpeed,
                    0f
                );
                _Rotation = Quaternion.LookRotation(desiredForward);
                _Rigidbody.MoveRotation(_Rotation);
            }

            // Move the player based on input
            _Rigidbody.MovePosition(_Rigidbody.position + _Movement * Speed * Time.fixedDeltaTime);

         
        }

        private void Update()
        {
            // Log Xbox controller button inputs
            LogControllerInputs();

            // Log Keyboard and Mouse Inputs
            LogKeyboardMouseInputs();

            // Check which side is active, to move the VFX Particles Slash Attack
            if (_IsLeftSide)
            {
                MoveToPosition(leftSidePosition, leftSideRotation);
            }
            else if (_IsRightSide)
            {
                MoveToPosition(rightSidePosition, rightSideRotation);
            }

            // Move and rotate the particle system smoothly if it's currently moving
            if (isMoving)
            {
                SlashVFXAttack1.transform.localPosition = Vector3.MoveTowards(SlashVFXAttack1.transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);
                SlashVFXAttack1.transform.localRotation = Quaternion.RotateTowards(SlashVFXAttack1.transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Check if it has reached the target position and rotation
                if (SlashVFXAttack1.transform.localPosition == targetPosition && SlashVFXAttack1.transform.localRotation == targetRotation)
                {
                    isMoving = false;  // Stop moving once the particle system has reached its target
                }
            }
        }

        private void MoveToPosition(Vector3 newPosition, Vector3 newRotation)
        {
            targetPosition = newPosition;
            targetRotation = Quaternion.Euler(newRotation);
            isMoving = true;  // Enable the movement
        }

        // KEYBOARDS INPUTS
        private void LogKeyboardMouseInputs()
        {
           
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                // A Button
                if (_IsGrounded)
                {
                    if (Input.GetKeyDown("space"))
                    {
                        Debug.Log("Space Keyboard Button Pressed");
                        // Jump if grounded and jump button is pressed
                        Jump();
                    }
                }

                if (Input.GetMouseButtonDown(0))
                {

                    Debug.Log("Mouse Button 0 Pressed");
                    if (_IsIdle)
                    {
                        if (_IsLeftSide)
                        {
                            PlayerSpriteRenderer.flipX = false;
                        }
                        else
                        {
                            if (_IsRightSide)
                            {
                                PlayerSpriteRenderer.flipX = true;
                            }
                        }
                        _IsAttack1 = true;
                    }


                    //PlayerAnimator.Play("CharacterAttack_1");
                }
            }
        }

        // Method to log the direction of the joystick
        private void LogJoystickDirection(float horizontalInput, float verticalInput)
        {
            bool _isMoving = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;
            _CharacterIsMoving = _isMoving;
            PlayerAnimator.SetBool("CharacterIsMoving", _CharacterIsMoving);

            if (!_isMoving)
            {
                HandleIdle();
                return;
            }

            // Joystick Up
            if (verticalInput > 0.01f && Mathf.Abs(horizontalInput) < 0.01f)
            {
                HandleAnimation(_IsGrounded ? "CharacterRunBack" : "CharacterJumpFallBack", flipX: false);
                UpdateDirectionFlags(false, true, false, false);
            }
            // Joystick Down
            if (verticalInput < -0.01f && Mathf.Abs(horizontalInput) < 0.01f)
            {
                HandleAnimation(_IsGrounded ? "CharacterRunFront" : "CharacterJumpFallFront", flipX: false);
                UpdateDirectionFlags(true, false, false, false);
            }
            // Joystick Left
            if (horizontalInput < -0.01f && Mathf.Abs(verticalInput) < 0.01f)
            {
                HandleAnimation(_IsGrounded ? "CharacterRunSide" : "CharacterJumpFallFront", flipX: true);
                UpdateDirectionFlags(false, false, true, false);
                lastHorizontalDirection = -1f;  // Moving left
            }
            // Joystick Right
            if (horizontalInput > 0.01f && Mathf.Abs(verticalInput) < 0.01f)
            {
                HandleAnimation(_IsGrounded ? "CharacterRunSide" : "CharacterJumpFallFront", flipX: false);
                UpdateDirectionFlags(false, false, false, true);
                lastHorizontalDirection = 1f;   // Moving right
            }

            //================================ DIAGONALS ====================
            // Joystick Up-Right
            if (verticalInput > 0.5f && horizontalInput > 0.5f)
            {
                HandleAnimation(_IsGrounded ? "CharacterRunDiagonalUp" : "CharacterJumpFallBack", flipX: false);
                UpdateDirectionFlags(false, false, false, false);
            }
            // Joystick Up-Left
            if (verticalInput > 0.5f && horizontalInput < -0.5f)
            {
                HandleAnimation(_IsGrounded ? "CharacterRunDiagonalUp" : "CharacterJumpFallBack", flipX: true);
                UpdateDirectionFlags(false, false, false, false);
            }
            // Joystick Down-Right
            if (verticalInput < -0.5f && horizontalInput > 0.5f)
            {
                HandleAnimation(_IsGrounded ? "CharacterRunDiagonalDown" : "CharacterJumpFallFront", flipX: false);
                UpdateDirectionFlags(false, false, false, false);
            }
            // Joystick Down-Left
            if (verticalInput < -0.5f && horizontalInput < -0.5f)
            {
                HandleAnimation(_IsGrounded ? "CharacterRunDiagonalDown" : "CharacterJumpFallFront", flipX: true);
                UpdateDirectionFlags(false, false, false, false);
            }
        }

        // Method to handle animations and set the last animation state
        private void HandleAnimation(string animationName, bool flipX)
        {
            Debug.Log(animationName);
            PlayerAnimator.Play(animationName);
            PlayerSpriteRenderer.flipX = flipX;
            LastAnimationState = animationName;
            VFXSlashLight.SetActive(false);
            _IsIdle = false;
            _IsAttack1 = false;
        }

        // Method to update direction flags
        private void UpdateDirectionFlags(bool front, bool back, bool left, bool right)
        {
            _IsFrontSide = front;
            _IsBackSide = back;
            _IsLeftSide = left;
            _IsRightSide = right;
        }

        // Handle idle state animations based on last movement
        private void HandleIdle()
        {
            _IsIdle = true;
            Debug.Log("Joystick Centered (Idle)");

            if (_IsGrounded && !_IsAttack1)
            {
                string idleAnimation = LastAnimationState switch
                {
                    "CharacterRunBack" => "CharacterIdleBack",
                    "CharacterRunFront" => "CharacterIdle",
                    "CharacterRunSide_FlipX" => "CharacterIdleSide",
                    "CharacterRunSide" => "CharacterIdleSide",
                    "CharacterRunDiagonalUp" => "CharacterIdleBack",
                    "CharacterRunDiagonalUp_FlipX" => "CharacterIdleBack",
                    "CharacterRunDiagonalDown" => "CharacterIdle",
                    "CharacterRunDiagonalDown_FlipX" => "CharacterIdle",
                    _ => "CharacterIdle"
                };

                PlayerAnimator.Play(idleAnimation);

                // Set flipX based on the last horizontal direction
                if (lastHorizontalDirection < 0)
                {
                    PlayerSpriteRenderer.flipX = true; // Face left
                }
                else if (lastHorizontalDirection > 0)
                {
                    PlayerSpriteRenderer.flipX = false; // Face right
                }
            }
        }

        // Method to log Xbox controller button inputs
        private void LogControllerInputs()
        {
            // Left Shoulder (LB)
            if (Input.GetButton("LeftShoulder"))
            {
                Debug.Log("Left Shoulder Pressed");
            }

            // Right Shoulder (RB)
            if (Input.GetButton("RightShoulder"))
            {
                Debug.Log("Right Shoulder Pressed");
            }

           

            // B Button
            if (Input.GetButtonDown("B"))
            {
                Debug.Log("B Button Pressed");
                GameManager.Instance.UIManager.CloseComicView();
            }

            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                // A Button
                if (_IsGrounded)
                {
                    if (Input.GetButtonDown("A"))
                    {
                        Debug.Log("A Button Pressed");
                        // Jump if grounded and jump button is pressed
                        Jump();
                    }
                }

                // X Button
                if (Input.GetButtonDown("X"))
                {

                    Debug.Log("X Button Pressed");
                    if (_IsIdle)
                    {
                        if (_IsLeftSide)
                        {
                            PlayerSpriteRenderer.flipX = false;
                        }
                        else
                        {
                            if (_IsRightSide)
                            {
                                PlayerSpriteRenderer.flipX = true;
                            }
                        }
                        _IsAttack1 = true;
                    }



                    //PlayerAnimator.Play("CharacterAttack_1");
                }
                else if (Input.GetButtonUp("X"))
                {
                    //_IsAttack1 = false;
                }
            }

            // Y Button
            if (Input.GetButtonDown("Y"))
            {
                Debug.Log("Y Button Pressed");
            }

            // Left Trigger
            float leftTrigger = Input.GetAxis("LeftTrigger");
            if (leftTrigger > 0.1f)  // Only log if it's pressed beyond a small threshold
            {
                Debug.Log("Left Trigger Pressed: " + leftTrigger);
            }

            // Right Trigger
            float rightTrigger = Input.GetAxis("RightTrigger");
            if (rightTrigger > 0.1f)  // Only log if it's pressed beyond a small threshold
            {
                Debug.Log("Right Trigger Pressed: " + rightTrigger);
            }
        }


        private bool IsGrounded()
        {
            // Raycast down from the player's position, slightly above the ground
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, GroundCheckDistance, GroundLayer))
            {
                if (!_IsGrounded)
                {
                    // First Hit on the ground with raycast
                    Debug.Log("Player First Ground Hit");
                    switch (LastAnimationState)
                    {
                        case "CharacterRunBack":
                            PlayerAnimator.Play("CharacterIdleBack");
                            PlayerSpriteRenderer.flipX = false;
                            break;
                        case "CharacterRunFront":
                            PlayerAnimator.Play("CharacterIdle");
                            PlayerSpriteRenderer.flipX = false;
                            break;
                        case "CharacterRunSide_FlipX":
                            PlayerAnimator.SetTrigger("CharacterIdleSide");
                            PlayerSpriteRenderer.flipX = true;
                            break;
                        case "CharacterRunSide":
                            PlayerAnimator.SetTrigger("CharacterIdleSide");
                            PlayerSpriteRenderer.flipX = false;
                            break;
                        case "CharacterRunDiagonalUp":
                            PlayerAnimator.Play("CharacterIdleBack");
                            PlayerSpriteRenderer.flipX = false;
                            break;
                        case "CharacterRunDiagonalUp_FlipX":
                            PlayerAnimator.Play("CharacterIdleBack");
                            PlayerSpriteRenderer.flipX = true;
                            break;
                        case "CharacterRunDiagonalDown":
                            PlayerAnimator.Play("CharacterIdle");
                            PlayerSpriteRenderer.flipX = false;
                            break;
                        case "CharacterRunDiagonalDown_FlipX":
                            PlayerAnimator.Play("CharacterIdle");
                            PlayerSpriteRenderer.flipX = true;
                            break;
                    }
                }
                Debug.Log("Player is grounded");
                return true;
            }
            else
            {
                Debug.Log("Player is not grounded");
                //switch (LastAnimationState)
                //{
                //    case "CharacterRunBack":
                //        PlayerAnimator.Play("CharacterJumpFallBack");
                //        PlayerSpriteRenderer.flipX = false;
                //        break;
                //    case "CharacterRunFront":
                //        PlayerAnimator.Play("CharacterJumpFallFront");
                //        PlayerSpriteRenderer.flipX = false;
                //        break;
                //    case "CharacterRunSide_FlipX":
                //        PlayerAnimator.SetTrigger("CharacterJumpFallSide");
                //        PlayerSpriteRenderer.flipX = true;
                //        break;
                //    case "CharacterRunSide":
                //        PlayerAnimator.SetTrigger("CharacterJumpFallSide");
                //        PlayerSpriteRenderer.flipX = false;
                //        break;
                //    case "CharacterRunDiagonalUp":
                //        PlayerAnimator.Play("CharacterJumpFallBack");
                //        PlayerSpriteRenderer.flipX = false;
                //        break;
                //    case "CharacterRunDiagonalUp_FlipX":
                //        PlayerAnimator.Play("CharacterJumpFallBack");
                //        PlayerSpriteRenderer.flipX = true;
                //        break;
                //    case "CharacterRunDiagonalDown":
                //        PlayerAnimator.Play("CharacterJumpFallFront");
                //        PlayerSpriteRenderer.flipX = false;
                //        break;
                //    case "CharacterRunDiagonalDown_FlipX":
                //        PlayerAnimator.Play("CharacterJumpFallFront");
                //        PlayerSpriteRenderer.flipX = true;
                //        break;
                //}
                return false;
            }
        }

        // Method to handle jumping
        private void Jump()
        {
            switch (LastAnimationState)
            {
                case "CharacterRunBack":
                    PlayerAnimator.SetTrigger("CharacterJumpFallBack");
                    PlayerSpriteRenderer.flipX = false;
                    break;
                case "CharacterRunFront":
                    PlayerAnimator.SetTrigger("CharacterJumpFallFront");
                    PlayerSpriteRenderer.flipX = false;
                    break;
                case "CharacterRunSide_FlipX":
                    PlayerAnimator.SetTrigger("CharacterJumpFallSide");
                    // Set flipX based on the last horizontal direction
                    if (lastHorizontalDirection < 0)
                    {
                        PlayerSpriteRenderer.flipX = true; // Face left
                    }
                    else if (lastHorizontalDirection > 0)
                    {
                        PlayerSpriteRenderer.flipX = false; // Face right
                    }
                    //PlayerSpriteRenderer.flipX = true;
                    break;
                case "CharacterRunSide":
                    PlayerAnimator.SetTrigger("CharacterJumpFallSide");
                    //PlayerSpriteRenderer.flipX = false;
                    // Set flipX based on the last horizontal direction
                    if (lastHorizontalDirection < 0)
                    {
                        PlayerSpriteRenderer.flipX = true; // Face left
                    }
                    else if (lastHorizontalDirection > 0)
                    {
                        PlayerSpriteRenderer.flipX = false; // Face right
                    }
                    break;
                case "CharacterRunDiagonalUp":
                    PlayerAnimator.Play("CharacterJumpFallBack");
                    PlayerSpriteRenderer.flipX = false;
                    break;
                case "CharacterRunDiagonalUp_FlipX":
                    PlayerAnimator.Play("CharacterJumpFallBack");
                    PlayerSpriteRenderer.flipX = true;
                    break;
                case "CharacterRunDiagonalDown":
                    PlayerAnimator.Play("CharacterJumpFallFront");
                    PlayerSpriteRenderer.flipX = false;
                    break;
                case "CharacterRunDiagonalDown_FlipX":
                    PlayerAnimator.Play("CharacterJumpFallFront");
                    PlayerSpriteRenderer.flipX = true;
                    break;
            }

            // Apply upward force to the Rigidbody for the jump
            _Rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);
            Debug.Log("Player jumped");
        }


    }
}

