using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using System;

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
        Rigidbody2D rb;
        public Bounds Bounds => collider2d.bounds;

        // audio files
        public AudioClip jumpGroundAudio;
        public AudioClip jumpAirAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        // constants
        public float maxSpeed = 6;
        private float groundJumpSpeed = 12;
        private float airJumpSpeed = 12;
        private float dashSpeed = 18;

        // frame data
        private float dashCooldown = 0.5f;

        // flags
        public bool controlEnabled = true;

        public bool canMove = true;
        public bool canAction = true;
        public bool canDash = true;
        public bool canJump = true;
        public bool canPunch = true;
        public bool canKick = true;
        public bool canSpecial = true;

        public bool isBusy = false;
        public bool isDashing = false;
        public bool isJumping = false;
        public bool isMoving = false;

        // states
        public MoveState moveState = MoveState.None;
        public JumpState jumpState = JumpState.Grounded;
        public ActionState actionState = ActionState.None;
        public DirectionState directionState = DirectionState.Forward;

        // timers
        float dashTimer = 0f;
        float dashCooler = 0f;

        // values
        Vector2 move;
        
        // input buffer
        private List<InputAction> inputBuffer = new List<InputAction>();

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
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
            UpdateActionState();
            UpdateJumpState();
            UpdateMoveState();
            UpdateDirectionState();
        }

        /// <summary>
        /// Detect horizontal movement and get actions from input buffer.
        /// </summary>
        protected override void Update()
        {
            if (canMove)
                move.x = Input.GetAxisRaw("Horizontal");
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
            decrementTimers();
            base.Update();
        }

        private void decrementTimers()
        {
            if (dashCooler > 0)
                dashCooler -= Time.deltaTime;
            else
                dashCooler = dashCooldown;
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
                    if (action.IsValid() && canAction)
                    {
                        PerformAction(action);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Perform actions.
        /// </summary>
        private void PerformAction(InputAction action)
        {
            if (action.Action == InputAction.ActionItem.Punch && canPunch)
                actionState = ActionState.Punch;
            else if (action.Action == InputAction.ActionItem.Kick && canKick)
                actionState = ActionState.Kick;
            else if (action.Action == InputAction.ActionItem.Special && canSpecial)
                actionState = ActionState.Special;
            else if (action.Action == InputAction.ActionItem.Dash && canDash)
                actionState = ActionState.Dash;
            else if (action.Action == InputAction.ActionItem.Jump && canJump)
                actionState = ActionState.Jump;
            else
                actionState = ActionState.None;
            //canAction = false;
        }
        
        /// <summary>
        /// Check for jump state changes and schedule jump events.
        /// </summary>
        void UpdateJumpState()
        {
            isJumping = false;
            if (actionState == ActionState.Jump)
            {
                if (IsGrounded)
                    jumpState = JumpState.GroundJump;
                else
                    jumpState = JumpState.AirJump;
            }
            switch (jumpState)
            {
                case JumpState.Grounded:
                    if (!IsGrounded && !isJumping)
                        jumpState = JumpState.Air;
                    break;
                case JumpState.GroundJump:
                    isJumping = true;
                    Schedule<PlayerGroundJumped>().player = this;
                    jumpState = JumpState.Air;
                    break;
                case JumpState.AirJump:
                    isJumping = true;
                    Schedule<PlayerAirJumped>().player = this;
                    jumpState = JumpState.Freefall;
                    break;
                case JumpState.Air:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Freefall:
                    canJump = false;
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    canJump = true;
                    canDash = true;
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
        float UpdateDirectionState()
        {
            if ((spriteRenderer.flipX && velocity.x > 0) || (!spriteRenderer.flipX && velocity.x < 0))
            {
                directionState = DirectionState.Backward;
                return -1;
            } 
            else
            {
                directionState = DirectionState.Forward;
                return 1;
            }
        }

        /// <summary>
        /// Update action state.
        /// </summary>
        void UpdateActionState()
        {
            switch (actionState)
            {
                case ActionState.Dash:
                    if (dashCooler > 0f)
                    {
                        if (directionState == DirectionState.Forward)
                            actionState = ActionState.ForwardDash;
                        else
                            actionState = ActionState.BackDash;
                    }
                    break;
                case ActionState.ForwardDash:
                    isDashing = true;
                    dashTimer = dashCooldown;
                    canDash = IsGrounded;
                    actionState = ActionState.None;
                    break;
                case ActionState.BackDash:
                    isDashing = true;
                    dashTimer = dashCooldown;
                    canDash = IsGrounded;
                    actionState = ActionState.None;
                    break;
                case ActionState.Jump:
                    if (isJumping)
                    {
                        actionState = ActionState.None;
                        canDash = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// Calculate velocity for grounded movement and jumps.
        /// </summary>
        protected override void ComputeVelocity()
        {
            
            // Flip sprite when not jumping
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

            // Calculate jump velocity
            if (isJumping && IsGrounded)
            {
                velocity.y = groundJumpSpeed * model.jumpModifier;
            }
            else if (isJumping)
            {
                velocity.y = airJumpSpeed * model.jumpModifier;
            }

            // Calculate dash velocity
            if (isDashing)
            {
                if (!IsGrounded)
                    velocity.y = model.jumpModifier;
                velocity.x = UpdateDirectionState() * dashSpeed;
                dashTimer -= Time.deltaTime * 8;
                maxSpeed = dashSpeed;
                canMove = false;
                if (dashTimer <= 0f)
                {
                    isDashing = false;
                    maxSpeed = 6;
                    canMove = true;
                }
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
        }

        public enum ActionState
        {
            None,
            Punch,
            Kick,
            Special,
            Dash,
            ForwardDash,
            BackDash,
            Jump
        }

        public enum DirectionState
        {
            Forward,
            Backward
        }
    }
}