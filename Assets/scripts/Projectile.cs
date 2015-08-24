using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

    public float speed = 50;
    public int damage = 1;
    public float lifeTime = 1.0f;
    public LayerMask hitMask = new LayerMask();
    public GameObject[] spawnOnHit = new GameObject[0];
    bool dying = false;

    float spawnTime = 0;

	// Use this for initialization
	void Start () 
    {
        spawnTime = Time.time;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        if (Time.time > spawnTime + lifeTime)
        {
            Kill();
        }
        if (dying == false)
        {
            //ray cast where we're moving this frame
            float step = speed * Time.fixedDeltaTime;
            Vector3 direction = transform.forward;
            RaycastHit hitInfo = new RaycastHit();
            if(Physics.Raycast(transform.position, direction, out hitInfo, step, hitMask))
            {
                transform.position = hitInfo.point;
                Kill();
                OnHit(hitInfo);
            }
            else
            {
                transform.position += transform.forward * speed * Time.fixedDeltaTime;
            }
        }
	}

    void Kill()
    {
        Destroy(gameObject, 0.5f);
        dying = true;
    }

    void OnHit(RaycastHit hitInfo)
    {
        //print("Projectile hit: " + hitInfo.collider.gameObject.name);
        hitInfo.collider.gameObject.SendMessageUpwards("OnDamage", damage);
        foreach (GameObject gobj in spawnOnHit)
        {
            GameObject obj = (GameObject)Instantiate(gobj, hitInfo.point, this.transform.rotation);
        }
    }
}
