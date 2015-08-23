﻿using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    #region public variables

    [Header("Component Links")]
	public CharacterController characterController = null;
    public Animator animator = null;
    public Animator wingAnimator = null;

    [Header("Platforming")]
    public float gravity = 9.8f;
	public float groundSpeed = 10; // units per second
	public float airSpeed = 5;
	public float turnRate = 0.3f; // time to turn 180
    public AnimationCurve jumpCurve = new AnimationCurve();
    public AnimationCurve airJumpCurve = new AnimationCurve();
    public int numberOfJumps = 2;
    public float glidingScale = 0.3f;
    public float groundedStateTimeout = 0.1f; //time isGrounded remains true after characterController reports false
    public LayerMask floorCastMask = new LayerMask();

    [Header("Combat")]
    public float dashAttackDistance = 1.0f;
    public float dashAttackDuration = 0.2f;

    #endregion

    #region unity methods

    Vector3 initialPosition = Vector3.zero;

	// Use this for initialization
	void Start () 
	{
		if (!characterController) 
		{
			characterController = GetComponent<CharacterController>();
		}
        jumpsRemaining = numberOfJumps;
        initialPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () 
	{
        HandleDebugInput();

        //get input
        float horizontalInput = Input.GetAxis("Horizontal");
        bool tryJump = Input.GetKeyDown("space");
        bool tryGlide = Input.GetKey("space");
        bool tryDashAttack = Input.GetButtonDown("Attack1");

        //update systems
        UpdateGrounded();
        UpdateRotation(horizontalInput);
        bool didJump = UpdateJump(tryJump);
        UpdateGliding(tryGlide);
        UpdateDashAttack(tryDashAttack, horizontalInput);
        UpdateVelocity(horizontalInput);

        //update animator state
        animator.SetBool("isGrounded", isGrounded);
        wingAnimator.SetBool("isGliding", isGliding);
        if (velocity.x != 0)
        {
            animator.SetFloat("hSpeed", Mathf.Abs(velocity.x) / groundSpeed);
        } else
        {
            animator.SetFloat("hSpeed", 0.01f);
        }
        animator.SetFloat("vSpeed", velocity.y);
        if (didJump)
        {
            if(isGrounded)
            {
                animator.SetTrigger("onJump");
            }
            else
            {
                animator.SetTrigger("onAirJump");
                wingAnimator.SetTrigger("onAirJump");
            }
        }
        animator.SetBool("isDashAttacking", isDashAttacking);

		// convert velocity to displacement and Move the character:
		characterController.Move(velocity * Time.deltaTime);
        Vector3 pos = characterController.transform.position;
        pos.z = 0;
        characterController.transform.position = pos;
	}

    void HandleDebugInput()
    {
        if (Input.GetKey("r"))
        {
            transform.position = initialPosition;
        }
    }

    #endregion

    #region Rotation Logic
    bool facingRight = true;
    Vector3 rot = Vector3.zero;
    Vector3 rotVel = Vector3.zero;

    void UpdateRotation(float horizontalInput)
    {
        //rotate the player to face direction of movement
        if (horizontalInput > 0 && facingRight == false)
        {
            facingRight = true;
        } else if (horizontalInput < 0 && facingRight == true)
        {
            facingRight = false;
        }
        float desiredYRot = facingRight ? 90 : -90;
        rot.y = Mathf.SmoothDamp(rot.y, desiredYRot, ref rotVel.y, turnRate);

        float desiredZRot = 0;
        if(isGrounded)
        {
            if(floorTangent.y > 0) //right incline
            {
                desiredZRot = Vector3.Angle(Vector3.right, floorTangent);
            }
            else //left incline
            {
                desiredZRot = -Vector3.Angle(Vector3.right, floorTangent);
            }
            desiredZRot *= facingRight ? -1 : 1;
            if(velocity.x < 1.0f)
            {
                desiredZRot *= 0.6f;
            }
        }
        rot.x = Mathf.SmoothDamp(rot.x, desiredZRot, ref rotVel.x, 0.2f);

        animator.transform.localEulerAngles = rot;
    }
    #endregion

    #region Movement Logic
    Vector2 desiredVelocity = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    Vector2 acceleration = Vector2.zero;
    float accumulatedGravity = 0; 
    float lastGroundedTime = 0;
    bool isGrounded = false;
    float inputDrag = 0.0f;

    Vector3 floorTangent = Vector3.right;
    
    void UpdateVelocity(float horizontalInput)
    {
        RaycastHit hitInfo = new RaycastHit();
        Ray ray = new Ray(this.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out hitInfo, 1.0f, floorCastMask))
        {
            floorTangent = Vector3.Cross(hitInfo.normal, Vector3.forward);
            floorTangent.z = 0;
        } else
        {
            floorTangent = Vector3.right;
        }

        Debug.DrawLine(ray.origin, ray.GetPoint(1.0f), Color.red);

        Debug.DrawLine(this.transform.position, this.transform.position + floorTangent * 2.0f, Color.red);


        //apply input
        if (isGrounded)
        {
            desiredVelocity =  floorTangent * horizontalInput * groundSpeed;
        } else
        {
            desiredVelocity = floorTangent * horizontalInput * airSpeed;
        }
        if (inputDrag > 0)
        {
            inputDrag -= inputDrag * Time.deltaTime / 0.3f;
            desiredVelocity *= Mathf.Clamp01(1-inputDrag);
        }

        //apply attacking
        if (isDashAttacking)
        {
            //override movement
            desiredVelocity = velocity = dashDirection * (dashAttackDistance / dashAttackDuration);
            accumulatedGravity = 0;
            return;
        }
        
        //clear verical velocity if grounded to avoid gravity building it up too much
        if (characterController.isGrounded)
        {
            accumulatedGravity = 0;
        }

        //apply gravity, scaled if gliding
        if (isGliding)
        {
            accumulatedGravity -= gravity * Time.deltaTime * glidingScale;
        } else
        {
            accumulatedGravity -= gravity * Time.deltaTime;
        }
        desiredVelocity.y += accumulatedGravity;

        //calculate jump velocity
        //We calulate the delta step on the animation curve and then override gravity if not 0
        if (jumpTick >= 0) //ensure tick is > 1 i.e. we've jumped at least once
        {
            jumpTick += Time.deltaTime;
            float newJumpHeight = wasLastJumpGrounded ? jumpCurve.Evaluate(jumpTick) : airJumpCurve.Evaluate(jumpTick);
            float jumpDelta = (newJumpHeight - lastJumpHeight);
            if (jumpDelta != 0)
            {
                desiredVelocity.y = jumpDelta / Time.deltaTime;
                accumulatedGravity = 0;
            }
            lastJumpHeight = newJumpHeight;
        }
        
        //smooth out velocity
        //velocity = Vector2.SmoothDamp(velocity, desiredVelocity, ref acceleration, 0.1f);
        velocity = desiredVelocity;
    }

    void UpdateGrounded()
    {
        //update last grounded time
        if (characterController.isGrounded)
        {
            if(Time.time > lastGroundedTime + 0.2f)
            {
                inputDrag = 0.9f;
            }
            lastGroundedTime = Time.time;
            isGrounded = true;
        }
        isGrounded = Time.time < lastGroundedTime + groundedStateTimeout;
    }

    #endregion

    #region Jump Logic
    float jumpTick = -1;
    int jumpsRemaining = 0;
    float lastJumpHeight = 0;
    bool wasLastJumpGrounded = true;

    //will return true if a jump was made
    bool UpdateJump(bool tryJump)
    {
        bool didJump = false;
        //reset jumps if we've been grounded for a short time
        if (isGrounded && (jumpTick < 0 || jumpTick > 0.1f))
        {
            jumpsRemaining = numberOfJumps;
        }
        if (jumpsRemaining > 0 || numberOfJumps == -1)
        {
            if (tryJump){
                OnJump();
                didJump = true;
            }
        }
        return didJump;
    }

    void OnJump()
    {
        jumpTick = 0; //start jump
        lastJumpHeight = 0;
        velocity *= 0.5f;
        jumpsRemaining--; //decrement remaining jumps
        wasLastJumpGrounded = isGrounded;
    }

    void CancelJump()
    {
        jumpTick = -1;
    }

    #endregion

    #region Gliding logic

    bool isGliding = false;

    void UpdateGliding(bool tryGlide)
    {
        isGliding = tryGlide && isGrounded == false && velocity.y < 0;
    }

    #endregion

    #region Attack Logic
    bool isDashAttacking = false;
    float dashAttackTick = 0;
    Vector2 dashDirection = Vector2.zero;

    //returns true if we're currently in a state of attacking
    void UpdateDashAttack(bool tryAttack, float horizontalInput)
    {
        if (dashAttackTick < dashAttackDuration)
        {
            dashAttackTick += Time.deltaTime;
            if(dashAttackTick > dashAttackDuration)
            {
                dashAttackTick = dashAttackDuration;
                isDashAttacking = false;
                print("End dash");
            }
        }
        if (tryAttack == true && isDashAttacking == false)
        {
            print("dash attack");
            isDashAttacking = true;
            dashAttackTick = 0;
            dashDirection = new Vector2(facingRight ? 1 : -1, 0);
            OnDashAttack();
        }
    }

    void OnDashAttack()
    {
        CancelJump();
    }

    #endregion
}
