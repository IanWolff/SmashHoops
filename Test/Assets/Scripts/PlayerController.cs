using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Components
    Rigidbody2D rb2D;

    // Player
    float walkSpeed = 4f;
    float speedLimiter = 0.7f;
    float inputHorizontal;
    float inputVertical;

    // Animations and States
    Animator animator;
    string currentAnimationState;
    const string PLAYER_IDLE = "Player_Idle";
    const string PLAYER_WALK_LEFT = "Player_Walk_Left";
    const string PLAYER_WALK_RIGHT = "Player_Walk_Right";
    const string PLAYER_CROUCH = "Player_Crouch";
    const string PLAYER_JUMP = "Player_Jump";
    const string PLAYER_FALL_LEFT = "Player_Fall_Left";
    const string PLAYER_FALL_RIGHT = "Player_Fall_Right";
    const string PLAYER_LAND = "Player_Land";

    // Start is called before the first frame update
    void Start()
    {
        rb2D = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");

        // Animation
        if (inputHorizontal > 0) 
        {
            ChangeAnimationState(PLAYER_WALK_RIGHT);
        }
        else if (inputHorizontal < 0)
        {
            ChangeAnimationState(PLAYER_WALK_LEFT);
        }
    }

    private void FixedUpdate()
    {
        if ( inputHorizontal != 0 || inputVertical != 0 )
        {
            if (inputHorizontal != 0 && inputVertical != 0)
            {
                inputHorizontal *= speedLimiter;
                inputVertical *= speedLimiter;
            }

            rb2D.velocity = new Vector2(inputHorizontal * walkSpeed, inputVertical * walkSpeed);
        }
        else
        {
            rb2D.velocity = new Vector2(0f,0f);
            ChangeAnimationState(PLAYER_IDLE);
        }
    }

    // Animation state changer
    void ChangeAnimationState(string nextAnimationState)
    {
        // Stop animation from interrupting itself
        if (currentAnimationState == nextAnimationState) return;

        // Play new animation
        animator.Play(nextAnimationState);
        currentAnimationState = nextAnimationState;
    }
}
