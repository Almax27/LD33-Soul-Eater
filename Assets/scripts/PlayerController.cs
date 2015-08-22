using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public CharacterController characterController;

	public float groundSpeed = 10; // units per second
	public float airSpeed = 5;

	public float turnRate = 0.3f; // time to turn 180

    public AnimationCurve jumpCurve;
    public int numberOfJumps = 2;

    public float gravity = 9.8f;

	Vector2 velocity = Vector2.zero;
    Vector2 desiredVelocity = Vector2.zero;
    Vector2 acceleration = Vector2.zero;

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
        float desiredYRot = 0;
        if (horizontalInput > 0)
        {
            desiredYRot = 1;
        } 
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

		//handle jump
		if (characterController.isGrounded)
		{
            jumpsRemaining = numberOfJumps;
			desiredVelocity.y = 0; 
		}
        if (jumpsRemaining > 0 || numberOfJumps == -1)
        {
            if (doJump){
                desiredVelocity.y = 0; //reset y velocity
                jumpTick = 0; //start jump
                jumpsRemaining--; //decrement remaining jumps
            }
        }

        float lastHeight = jumpCurve.Evaluate(jumpTick);
        jumpTick += Time.deltaTime;
        float newHeight = jumpCurve.Evaluate(jumpTick);
        float jumpDelta = (newHeight - lastHeight);

		// apply gravity if we're no longer jumping
        if(jumpDelta == 0)
        {
            desiredVelocity.y -= gravity * Time.deltaTime;
        }

        //smooth out velocity
        velocity = Vector2.SmoothDamp(velocity, desiredVelocity, ref acceleration, 0.1f);

		// convert vel to displacement and Move the character:
		characterController.Move((velocity * Time.deltaTime) + new Vector2(0, jumpDelta));
	}
}
