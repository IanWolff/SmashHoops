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
        public AudioClip jumpGroundAudio;
        public AudioClip jumpAirAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 6;
        /// <summary>
        /// Initial jump velocity at the start of a short jump.
        /// </summary>
        public float shortJumpSpeed = 6;
        /// <summary>        
        /// Initial jump velocity at the start of a full jump.
        /// </summary>
        public float fullJumpSpeed = 12;
        /// <summary>
        /// Initial jump velocity at the start of an air jump.
        /// </summary>
        public float airJumpSpeed = 14;

        public JumpState jumpState = JumpState.Grounded;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Percent percent;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            percent = GetComponent<Percent>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            UpdateJumpState();
        }

        /// <summary>
        /// Detect a jump input and set the initial jump state.
        /// </summary>
        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = Input.GetAxis("Horizontal");
                if (Input.GetButtonDown("Jump"))
                {
                    switch (jumpState){
                        case JumpState.Grounded:
                            jumpState = JumpState.GroundJump;
                            break;
                        case JumpState.Air:
                            jumpState = JumpState.AirJump;
                            break;
                    }
                }
            }
            else
            {
                move.x = 0;
            }
            base.Update();
        }

        /// <summary>
        /// Check for jump state changes and schedule jump events 
        /// </summary>
        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.Grounded:
                    if (!IsGrounded && !jump)
                    {
                        jumpState = JumpState.Air;
                    }
                    break;
                case JumpState.GroundJump:
                    jump = true;
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
                    jump = true;
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
        /// Calculate velocity for grounded movement and jump states.
        /// </summary>
        protected override void ComputeVelocity()
        {
            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            // Calculate jump velocity
            if (jump && IsGrounded)
            {
                velocity.y = fullJumpSpeed * model.jumpModifier;
            }
            else if (jump)
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
            Landed
        }
    }
}