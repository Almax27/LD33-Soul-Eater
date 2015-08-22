using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public CharacterController characterController = null;
    public Animator animator = null;

	public float groundSpeed = 10; // units per second
	public float airSpeed = 5;

	public float turnRate = 0.3f; // time to turn 180

    public AnimationCurve jumpCurve = new AnimationCurve();
    public int numberOfJumps = 2;

    public float gravity = 9.8f;

	Vector2 velocity = Vector2.zero;
    Vector2 acceleration = Vector2.zero;

    bool facingRight = true;
	float yRot = 0;
	float yRotVel = 0;

    float jumpTick = float.MaxValue;
    int jumpsRemaining = 0;
    float lastJumpHeight = 0;

	// Use this for initialization
	void Start () 
	{
		if (!characterController) 
		{
			characterController = GetComponent<CharacterController>();
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
        //get input
        float horizontalInput = Input.GetAxis("Horizontal");
        bool tryJump = Input.GetKeyDown("space");

        //update systems
        UpdateRotation(horizontalInput);
        bool didJump = UpdateJump(tryJump);
        UpdateVelocity(horizontalInput);

        //update animator state
        animator.SetBool("isGrounded", characterController.isGrounded);
        animator.SetFloat("hSpeed", Mathf.Abs(velocity.x) / groundSpeed);
        animator.SetFloat("vSpeed", velocity.y);
        if (didJump)
        {
            animator.SetTrigger("onJump");
        }

		// convert velocity to displacement and Move the character:
		characterController.Move(velocity * Time.deltaTime);
	}

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
        yRot = Mathf.SmoothDampAngle(yRot, desiredYRot, ref yRotVel, turnRate);
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }

    void UpdateVelocity(float horizontalInput)
    {
        Vector2 desiredVelocity = Vector2.zero;

        //apply input
        if (characterController.isGrounded)
        {
            desiredVelocity.x = horizontalInput * groundSpeed;
        } else
        {
            desiredVelocity.x = horizontalInput * airSpeed;
        }
        
        //clear verical velocity if grounded to avoid gravity building it up too much
        if (characterController.isGrounded)
        {
            desiredVelocity.y = 0;
        }

        //apply gravity
        desiredVelocity.y -= gravity * Time.deltaTime;

        //calculate jump velocity
        //We calulate the delta step on the animation curve and then override gravity if not 0
        jumpTick += Time.deltaTime;
        float newJumpHeight = jumpCurve.Evaluate(jumpTick);
        lastJumpHeight = newJumpHeight;
        float jumpDelta = (lastJumpHeight - lastJumpHeight);
        if (jumpDelta != 0)
        {
            desiredVelocity.y = jumpDelta / Time.deltaTime;
        }
        
        //smooth out velocity
        velocity = Vector2.SmoothDamp(velocity, desiredVelocity, ref acceleration, 0.1f);
    }

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
                jumpTick = 0; //start jump
                OnJump();
                didJump = true;
            }
        }
        return didJump;
    }

    void OnJump()
    {
        velocity = Vector2.zero;
        jumpsRemaining--; //decrement remaining jumps
    }
}
