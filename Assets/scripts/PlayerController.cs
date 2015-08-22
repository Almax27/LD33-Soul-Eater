using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public CharacterController characterController;
    public Animator animator;

	public float groundSpeed = 10; // units per second
	public float airSpeed = 5;

	public float turnRate = 0.3f; // time to turn 180

    public AnimationCurve jumpCurve;
    public int numberOfJumps = 2;

    public float gravity = 9.8f;

	Vector2 velocity = Vector2.zero;
    Vector2 desiredVelocity = Vector2.zero;
    Vector2 acceleration = Vector2.zero;

    bool facingRight = true;
	float yRot = 0;
	float yRotVel = 0;

    float jumpTick = 0;
    int jumpsRemaining = 0;

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
        bool doJump = Input.GetKeyDown("space");

        //rotate the plate to face direction of movement
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

        //apply movement
        if (characterController.isGrounded)
        {
            desiredVelocity.x = horizontalInput * groundSpeed;
        } else
        {
            desiredVelocity.x = horizontalInput * airSpeed;
        }

		//clear verical velocity if grounded
        if (characterController.isGrounded)
        {
            desiredVelocity.y = 0; 
        }

        //reset jumps if we've been grounded for a short time
        if (characterController.isGrounded && jumpTick > 0.1f)
        {
            jumpsRemaining = numberOfJumps;
        }
        if (jumpsRemaining > 0 || numberOfJumps == -1)
        {
            if (doJump){
                OnJump();
            }
        }

        //apply gravity
        desiredVelocity.y -= gravity * Time.deltaTime;

        float lastHeight = jumpCurve.Evaluate(jumpTick);
        jumpTick += Time.deltaTime;
        float newHeight = jumpCurve.Evaluate(jumpTick);
        float jumpDelta = (newHeight - lastHeight);
        if (jumpDelta != 0)
        {
            desiredVelocity.y = jumpDelta / Time.deltaTime;
        }

        //smooth out velocity
        velocity = Vector2.SmoothDamp(velocity, desiredVelocity, ref acceleration, 0.1f);

        Vector2 actualVelocity = velocity + new Vector2(0, jumpDelta) / Time.deltaTime;
        animator.SetBool("isGrounded", characterController.isGrounded);
        animator.SetFloat("hSpeed", Mathf.Abs(actualVelocity.x) / groundSpeed);
        animator.SetFloat("vSpeed", actualVelocity.y);

		// convert vel to displacement and Move the character:
		characterController.Move(velocity * Time.deltaTime);


	}

    void OnJump()
    {
        desiredVelocity.y = 0; //reset y velocity
        jumpTick = 0; //start jump
        jumpsRemaining--; //decrement remaining jumps
        print(jumpsRemaining);
        animator.SetTrigger("onJump");
    }
}
