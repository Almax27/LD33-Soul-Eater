using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    #region public variables

    [Header("Component Links")]
	public CharacterController characterController = null;
    public Animator animator = null;

    [Header("Platforming")]
    public float gravity = 9.8f;
	public float groundSpeed = 10; // units per second
	public float airSpeed = 5;
	public float turnRate = 0.3f; // time to turn 180
    public AnimationCurve jumpCurve = new AnimationCurve();
    public int numberOfJumps = 2;
    public float glidingScale = 0.3f;

    [Header("Combat")]
    public float dashAttackDistance = 1.0f;
    public float dashAttackDuration = 0.2f;

    #endregion

    #region unity methods

	// Use this for initialization
	void Start () 
	{
		if (!characterController) 
		{
			characterController = GetComponent<CharacterController>();
		}
        jumpsRemaining = numberOfJumps;
	}
	
	// Update is called once per frame
	void Update () 
	{
        //get input
        float horizontalInput = Input.GetAxis("Horizontal");
        bool tryJump = Input.GetKeyDown("space");
        bool tryGlide = Input.GetKey("space");
        bool tryDashAttack = Input.GetButtonDown("Attack1");

        //update systems
        UpdateRotation(horizontalInput);
        bool didJump = UpdateJump(tryJump);
        UpdateGliding(tryGlide);
        UpdateDashAttack(tryDashAttack, horizontalInput);
        UpdateVelocity(horizontalInput);

        //update animator state
        animator.SetBool("isGrounded", characterController.isGrounded);
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
            animator.SetTrigger("onJump");
        }
        animator.SetBool("isDashAttacking", isDashAttacking);

		// convert velocity to displacement and Move the character:
		characterController.Move(velocity * Time.deltaTime);
	}

    #endregion

    #region Rotation Logic
    bool facingRight = true;
    float yRot = 0;
    float yRotVel = 0;

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
        yRot = Mathf.SmoothDamp(yRot, desiredYRot, ref yRotVel, turnRate);
        transform.localEulerAngles = new Vector3(0, desiredYRot, 0);
    }
    #endregion

    #region Movement Logic
    Vector2 desiredVelocity = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    Vector2 acceleration = Vector2.zero;
    void UpdateVelocity(float horizontalInput)
    {
        //apply input
        if (characterController.isGrounded)
        {
            desiredVelocity.x = horizontalInput * groundSpeed;
        } else
        {
            desiredVelocity.x = horizontalInput * airSpeed;
        }

        //apply attacking
        if (isDashAttacking)
        {
            //override movement
            float dashAttackDirection = facingRight ? 1 : -1;
            velocity.x = dashAttackDirection * (dashAttackDistance / dashAttackDuration);
            print("dashspeed: " + velocity.x);
            velocity.y = 0;
            return;
        }
        
        //clear verical velocity if grounded to avoid gravity building it up too much
        if (characterController.isGrounded)
        {
            desiredVelocity.y = 0;
        }

        //apply gravity, scaled if gliding
        if (isGliding)
        {
            desiredVelocity.y -= gravity * Time.deltaTime * glidingScale;
        } else
        {
            desiredVelocity.y -= gravity * Time.deltaTime;
        }

        //calculate jump velocity
        //We calulate the delta step on the animation curve and then override gravity if not 0
        if (jumpTick >= 0) //ensure tick is > 1 i.e. we've jumped at least once
        {
            jumpTick += Time.deltaTime;
            float newJumpHeight = jumpCurve.Evaluate(jumpTick);
            float jumpDelta = (newJumpHeight - lastJumpHeight);
            if (jumpDelta != 0)
            {
                desiredVelocity.y = jumpDelta / Time.deltaTime;
            }
            lastJumpHeight = newJumpHeight;
        }
        
        //smooth out velocity
        //velocity = Vector2.SmoothDamp(velocity, desiredVelocity, ref acceleration, 0.1f);
        velocity = desiredVelocity;
    }
    #endregion

    #region Jump Logic
    float jumpTick = -1;
    int jumpsRemaining = 0;
    float lastJumpHeight = 0;

    //will return true if a jump was made
    bool UpdateJump(bool tryJump)
    {
        bool didJump = false;
        //reset jumps if we've been grounded for a short time
        if (characterController.isGrounded && jumpTick > 0.1f)
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
    }

    #endregion

    #region Gliding logic

    bool isGliding = false;

    void UpdateGliding(bool tryGlide)
    {
        if (characterController.isGrounded == false)
        {
            isGliding = tryGlide;
        }
    }

    #endregion

    #region Attack Logic
    bool isDashAttacking = false;
    float dashAttackTick = 0;

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
        }
    }

    #endregion
}
