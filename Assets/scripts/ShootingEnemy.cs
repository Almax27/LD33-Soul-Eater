using UnityEngine;
using System.Collections;

public class ShootingEnemy : MonoBehaviour {
    [Header("Component Links")]
    public Transform projectileSpawnNode = null;
    public Projectile projectilePrefab = null;
    public Transform target = null;
    public Animator animator = null;
    public RagDollHelper ragDollHelper = null;
    public CharacterController characterController = null;

    [Header("Movement")]
    public float gravity = 30;

    [Header("Shooting")]
    public float range = 10;
    public LayerMask visionMask = new LayerMask(); //layers that block vision
    public LayerMask targetMask = new LayerMask(); //layers targets exist in
    public float startShootingDelay = 1.0f;
    public float stopShootingDelay = 1.0f;

    public float rateOfFire = 2; //shots per second
    public float reloadTime = 1.0f;
    public int maxAmmo = 5;

    Vector3 aimDirection = Vector3.right;

    float shootTick = 0;
    int ammoRemaining = 0;
    float reloadTick = 0;
    float lastFoundTargetTime = 0;
    float lastLostTargetTime = 0;

	// Use this for initialization
	void Start () 
    {
        ammoRemaining = maxAmmo;
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        FindTarget();
        UpdateAim();
        if ((target && Time.time > lastFoundTargetTime + startShootingDelay) || 
            (!target && Time.time < lastLostTargetTime + stopShootingDelay))
        {
            TryShoot();
        }
        UpdateGravity(Time.deltaTime);
        Vector3 velocity = new Vector3(0, accumulatedGravity, 0);
        characterController.Move(velocity * Time.deltaTime);
    }

    float lastTargetCheck = 0;
    void FindTarget()
    {
        if (Time.time > lastTargetCheck + 0.2f)
        {
            lastTargetCheck = Time.time;
            if (target != null)
            {
                Vector3 targetDir = target.position - projectileSpawnNode.position;
                bool isVisionObstructed = Physics.Raycast(projectileSpawnNode.position, targetDir.normalized, targetDir.magnitude, visionMask);
                if (isVisionObstructed)
                {
                    target = null;
                    lastLostTargetTime = Time.time;
                }
            } else
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, range, targetMask);
                if (colliders.Length > 0)
                {
                    Transform potentialTarget = colliders [0].transform.FindChild("ProjectileTarget");
                    if (potentialTarget == null)
                    {
                        potentialTarget = colliders [0].transform;
                    }
                    Vector3 targetDir = potentialTarget.position - projectileSpawnNode.position;
                    if (!Physics.Raycast(projectileSpawnNode.position, targetDir.normalized, targetDir.magnitude, visionMask))
                    {
                        target = potentialTarget;
                        lastFoundTargetTime = Time.time;
                    }
                }
            }
        }
    }

    float tLook = 0;
    float tLookVel = 0;

    float yRot = 0;
    float yRotVel = 0;

    void UpdateAim()
    {
        //constant look angles assumed from model
        const float minAngle = -40;
        const float maxAngle = 50;

        float desiredTLook = 0.5f;
        if (target)
        {
            Vector3 targetDir = (target.position - projectileSpawnNode.position).normalized;
            float angle = 0; //from horizontal
            if(targetDir.x > 0) //looking right
            {
                angle = Vector3.Angle(Vector3.right, targetDir);
            }
            else
            {
                angle = Vector3.Angle(Vector3.left, targetDir);
            }
            if(targetDir.y < 0)
            {
                angle = -angle;
            }

            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            desiredTLook = Mathf.InverseLerp(minAngle, maxAngle, angle);

            aimDirection = targetDir;
        }

        //update look direction
        float desiredYRot = aimDirection.x > 0 ? 90 : -90;
        yRot = Mathf.SmoothDampAngle(yRot, desiredYRot, ref yRotVel, 0.1f);
        if (yRot != desiredYRot)
        {
            animator.transform.eulerAngles = new Vector3(0, yRot, 0);
        }

        //update animator
        tLook = Mathf.SmoothDamp(tLook, desiredTLook, ref tLookVel, 0.2f);
        if (tLook != desiredTLook)
        {
            animator.SetFloat("tLook", 1 - tLook);
        }
    }

    float accumulatedGravity = 0;
    void UpdateGravity(float deltaTime)
    {
        if (characterController.isGrounded)
        {
            accumulatedGravity = 0;
        }
        accumulatedGravity -= gravity * deltaTime;
    }

    void TryShoot()
    {
        //try reload
        if (ammoRemaining == 0)
        {
            reloadTick += Time.deltaTime;
            if(reloadTick > reloadTime)
            {
                reloadTick = 0;
                ammoRemaining = maxAmmo;
            }
        }

        //try shoot
        shootTick += Time.deltaTime;
        if (ammoRemaining > 0 && shootTick > 1.0f / rateOfFire)
        {
            shootTick = 0;
            ammoRemaining--;
            if(projectilePrefab)
            {
                GameObject projectile = (GameObject)GameObject.Instantiate(projectilePrefab.gameObject, projectileSpawnNode.position, Quaternion.LookRotation(aimDirection));
                animator.SetTrigger("OnShoot");
            }
        }
    }

    void OnDeath()
    {
        ragDollHelper.SetIsRagDoll(true);
        this.enabled = false;
        characterController.enabled = false;
        Destroy(gameObject, 5);
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(projectileSpawnNode.position, aimDirection * range);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
