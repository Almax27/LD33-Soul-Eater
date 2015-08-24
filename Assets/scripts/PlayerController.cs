using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    #region public variables

    [Header("Component Links")]
	public CharacterController characterController = null;
    public Animator animator = null;
    public Animator wingAnimator = null;
    public RagDollHelper ragDollHelper = null;
    public Transform chestNode = null;

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
    public GameObject[] spawnOnAttack = new GameObject[0];
    public GameObject[] spawnOnDashAttack = new GameObject[0];

    #endregion

    #region unity methods

    Vector3 initialPosition = Vector3.zero;

    void Awake()
    {
    }

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
        //get input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        bool tryJump = Input.GetKeyDown("space");
        bool tryGlide = Input.GetKey("space");

        AttackState desiredAttackState = AttackState.None;
        if (Input.GetButtonDown("Attack1"))
        {
            desiredAttackState = AttackState.Attacking;
        } else if (Input.GetButtonDown("Attack2"))
        {
            desiredAttackState = AttackState.DashAttacking;
        }

        //update systems
        UpdateGrounded();
        UpdateRotation(horizontalInput, verticalInput < 0);
        bool didJump = UpdateJump(tryJump);
        UpdateGliding(tryGlide);
        AttackState changedToAttackState = UpdateAttack(desiredAttackState);
        UpdateVelocity(horizontalInput);

        //update animator state
        if(changedToAttackState != AttackState.None)
        {
            animator.SetTrigger("onAttack");
        }
        animator.SetBool("isDashing", attackState == AttackState.DashAttacking);
        animator.SetBool("isGrounded", isGrounded);
        wingAnimator.SetBool("isOpen", isGliding); 
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
                wingAnimator.SetTrigger("onFlap");
            }
        }

		// convert velocity to displacement and Move the character:
		characterController.Move(velocity * Time.deltaTime);
        Vector3 pos = characterController.transform.position;
        pos.z = 0;
        characterController.transform.position = pos;
	}

    #endregion

    #region Rotation Logic
    bool facingRight = true;
    Vector3 rot = Vector3.zero;
    Vector3 rotVel = Vector3.zero;

    void UpdateRotation(float horizontalInput, bool tryLookForward)
    {
        //rotate the player to face direction of movement
        bool updateLooking = true;
        float desiredYRot = facingRight ? 90 : -90;
        if (horizontalInput > 0 && facingRight == false)
        {
            desiredYRot = 90;
            facingRight = true;
        } else if (horizontalInput < 0 && facingRight == true)
        {
            facingRight = false;
            desiredYRot = -90;
        } else if (tryLookForward && horizontalInput == 0)
        {
            desiredYRot = 180;
        } else
        {
            updateLooking = false;
        }

        rot.y = Mathf.SmoothDampAngle(rot.y, desiredYRot, ref rotVel.y, turnRate);

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
            desiredZRot = Mathf.Clamp(desiredZRot, -20, 20);
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

        //apply dashing
        if (attackState == AttackState.DashAttacking)
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
        if (characterController.isGrounded && jumpTick > 0.1f)
        {
            jumpsRemaining = numberOfJumps;
        }
        if (jumpsRemaining > 0)
        {
            if (tryJump){
                OnJump();
                print(jumpsRemaining);
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
    enum AttackState
    {
        None,
        Attacking,
        DashAttacking
    }
    AttackState attackState = AttackState.None;

    Vector2 dashDirection = Vector2.zero;
    float endAttackTime = 0;

    AttackState UpdateAttack(AttackState desiredState)
    {
        if (Time.time > endAttackTime)
        {
            if(attackState == AttackState.DashAttacking)
            {
                OnDashAttackEnd();
            }
            attackState = AttackState.None;
        }
        AttackState oldState = attackState;
        if (attackState == AttackState.None)
        {
            switch (desiredState)
            {
                case AttackState.Attacking:
                {
                    attackState = AttackState.Attacking;
                    endAttackTime = Time.time + 0.2f;
                    OnAttack();
                    return AttackState.Attacking;
                    //break;
                }
                case AttackState.DashAttacking:
                {
                    attackState = AttackState.DashAttacking;
                    dashDirection = new Vector2(facingRight ? 1 : -1, 0);
                    endAttackTime = Time.time + dashAttackDuration;
                    OnDashAttack();
                    return AttackState.DashAttacking;
                    //break;
                }
            }
        }
        return AttackState.None;
    }

    void OnAttack()
    {
        inputDrag = 0.9f;
        foreach (GameObject gobj in spawnOnAttack)
        {
            GameObject instance = (GameObject)Instantiate(gobj);
            instance.transform.parent = animator.transform;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
        }
    }

    void OnDashAttack()
    {
        CancelJump();
        gameObject.layer = LayerMask.NameToLayer("PlayerDashing");
        foreach (GameObject gobj in spawnOnDashAttack)
        {
            GameObject instance = (GameObject)Instantiate(gobj);
            instance.transform.parent = animator.transform;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
        }
    }

    void OnDashAttackEnd()
    {
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    #endregion

    #region Death Logic

    void OnDeath()
    {
        print("Player died!");
        ragDollHelper.SetIsRagDoll(true);
        this.enabled = false;
        characterController.enabled = false;
        Destroy(gameObject, 5);
    }

    #endregion
}
