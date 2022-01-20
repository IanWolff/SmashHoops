using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        // internal
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        public Collider2D collider2d;
        public AudioSource audioSource;
        public Percent percent;
        SpriteRenderer spriteRenderer;
        public Bounds Bounds => collider2d.bounds;

        // audio files
        public AudioClip jumpGroundAudio;
        public AudioClip jumpAirAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        // constants
        public float maxSpeed = 3;
        public float groundJumpSpeed = 12;
        public float airJumpSpeed = 14;

        // flags
        public bool controlEnabled = true;
        public bool canAction = true;
        public bool canDash = true;
        public bool isDashing = false;
        public bool isJumping = false;
        public bool isMoving = false;

        // states
        public MoveState moveState = MoveState.None;
        public JumpState jumpState = JumpState.Grounded;
        public DirectionState directionState = DirectionState.Forward;

        // values
        Vector2 move;
        
        // input buffer
        private List<InputAction> inputBuffer = new List<InputAction>();
       
        void Awake()
        {
            percent = GetComponent<Percent>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Execute KinematicObject velocity updates and update jump states.
        /// </summary>
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            UpdateJumpState();
            UpdateMoveState();
            UpdateDirectionState();
        }

        /// <summary>
        /// Detect horizontal movement and get actions from input buffer.
        /// </summary>
        protected override void Update()
        {
            move.x = Input.GetAxis("Horizontal");
            if (controlEnabled)
            {
                isMoving = Input.GetAxisRaw("Horizontal") != 0;
                BufferInput();
                if (canAction)
                {
                    GetAction();
                }
            }
            else
            {
                move.x = 0;
            }
            base.Update();
        }

        /// <summary>
        /// Convert inputs to actions and add to input buffer.
        /// </summary>
        private void BufferInput()
        {
            foreach (InputAction.ActionItem action in System.Enum.GetValues(typeof(InputAction.ActionItem))) {
                if (Input.GetButtonDown(action.ToString()))
                {
                    inputBuffer.Add(new InputAction(action, Time.time));
                }
            }
        }

        /// <summary>
        /// Removes the oldest input from input buffer and performs action.
        /// </summary>
        private void GetAction()
        {
            if (inputBuffer.Count > 0)
            {
                foreach (InputAction action in inputBuffer.ToArray())
                {
                    inputBuffer.Remove(action);
                    if (action.IsValid())
                    {
                        PerformAction(action);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Perform actions and set jump state.
        /// </summary>
        private void PerformAction(InputAction action)
        {
            if (action.Action == InputAction.ActionItem.Jump)
            {
                switch (jumpState)
                {
                    case JumpState.Grounded:
                        jumpState = JumpState.GroundJump;
                        break;
                    case JumpState.Air:
                        jumpState = JumpState.AirJump;
                        break;
                }
            }
            //canAction = false;
        }
        
        /// <summary>
        /// Check for jump state changes and schedule jump events.
        /// </summary>
        void UpdateJumpState()
        {
            isJumping = false;
            switch (jumpState)
            {
                case JumpState.Grounded:
                    if (!IsGrounded && !isJumping)
                    {
                        jumpState = JumpState.Air;
                    }
                    break;
                case JumpState.GroundJump:
                    isJumping = true;
                    Schedule<PlayerGroundJumped>().player = this;
                    jumpState = JumpState.Air;
                    break;
                case JumpState.Air:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.AirJump:
                    isJumping = true;
                    Schedule<PlayerAirJumped>().player = this;
                    jumpState = JumpState.Freefall;
                    break;
                case JumpState.Freefall:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        /// <summary>
        /// Check for move state changes. 
        /// </summary>
        void UpdateMoveState()
        {
            switch (moveState)
            {
                case MoveState.None:
                    if (velocity.x != 0)
                    {
                        moveState = MoveState.Move;
                    }
                    break;
                case MoveState.Move:
                    if (velocity.x == 0)
                    {
                        moveState = MoveState.None;
                    }
                    break;
            }
        }


        /// <summary>
        /// Update direction state.
        /// </summary>
        void UpdateDirectionState()
        {
            if (spriteRenderer.flipX && velocity.x > 0)
            {
                directionState = DirectionState.Backward;
            } 
            else if (!spriteRenderer.flipX && velocity.x < 0)
            {
                directionState = DirectionState.Backward;
            }
            else
            {
                directionState = DirectionState.Forward;
            }
        }

        /// <summary>
        /// Calculate velocity for grounded movement and jumps.
        /// </summary>
        protected override void ComputeVelocity()
        {
            if (jumpState != JumpState.Freefall && jumpState != JumpState.Air)
            {
                if (move.x > 0.01f)
                {
                    spriteRenderer.flipX = false;
                }
                else if (move.x < -0.01f)
                {
                    spriteRenderer.flipX = true;
                }
            }

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            // Calculate isJumping velocity
            if (isJumping && IsGrounded)
            {
                velocity.y = groundJumpSpeed * model.jumpModifier;
            }
            else if (isJumping)
            {
                velocity.y = airJumpSpeed * model.jumpModifier;
            }

            // Calculate movement velocity
            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            GroundJump,
            Air,
            AirJump,
            Freefall,
            Landed,
        }

        public enum MoveState
        {
            None,
            Move,
            Stun,
            ForwardDash,
            BackDash
        }

        public enum DirectionState
        {
            Forward,
            Backward
        }
    }
}