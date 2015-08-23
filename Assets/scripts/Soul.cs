using UnityEngine;
using System.Collections;

public class Soul : MonoBehaviour 
{

    public Transform target;

    public int soulValue = 1;

    public float maxSpeed = 20;
    public float accelerationTime = 1.0f;

    public float turningDuration = 1.0f; 
    public float turningRate = 0.3f;

    public float chaseDelay = 2.0f;
    public AnimationCurve snakeAnimation = new AnimationCurve();

    public float collectionDistance = 0.1f;

    public bool isCollected = false;
    public bool isCollectable = false;

    public ParticleSystem[] particleSystems = null;

    float tick = 0;

    float speed = 0;
    float acceleration = 0;

    Vector2 direction = Vector2.up;
    Vector2 desiredDirection = Vector2.up;
    Vector2 directionalVelocity = Vector2.zero;

    float lastSnakeOffset = 0.0f;

	// Use this for initialization
	void Start () 
    {
        direction = desiredDirection = new Vector3(Random.value, Random.value, 0).normalized;
        transform.localScale = Vector3.one * 0.5f;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if(target)
        {
            tick += Time.deltaTime;

            if(tick > chaseDelay)
            {
                //chase target
                desiredDirection = (target.transform.position - this.transform.position).normalized;
            }

            speed = Mathf.SmoothDamp(speed, maxSpeed, ref acceleration, accelerationTime);

            if(tick < turningDuration)
            {
                direction = Vector2.SmoothDamp(direction, desiredDirection, ref directionalVelocity, turningRate);
            }
            else
            {
                isCollectable = true;
                direction = desiredDirection;
            }
                
            this.transform.position += new Vector3(direction.x, direction.y) * speed * Time.deltaTime;

            float snakeOffset = snakeAnimation.Evaluate(tick);
            float deltaSnakeOffset = (snakeOffset - lastSnakeOffset);
            lastSnakeOffset = snakeOffset;
            Vector3 perpToMovement = new Vector3(-direction.y, direction.x);
            transform.position += perpToMovement * deltaSnakeOffset;

        }
        if (isCollected)
        {
            bool allSystemsDead = true;
            foreach(ParticleSystem psys in particleSystems)
            {
                //disable if all before it have finished
                psys.enableEmission = false;
                allSystemsDead &= psys.IsAlive() == false;
            }
            if(allSystemsDead)
            {
                Destroy(gameObject);
            }
        }
	}
}
