using UnityEngine;
using System.Collections.Generic;

public class DamageArea : MonoBehaviour {

    public int damage = 0;
    public float damagePerSecond = 0;
    public float damageRadius = 1.0f;
    public LayerMask damageMask = new LayerMask();
    public GameObject[] spawnOnDamage = new GameObject[0];

    List<Collider> damaged = new List<Collider>();
    float tick = 0;

	// Use this for initialization
	void Start () { 

	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (damagePerSecond > 0)
        {
            tick += Time.fixedTime;
            float damageInverval = 1.0f / damagePerSecond;
            while (tick > damageInverval)
            {
                tick -= damageInverval;
                ApplyDamage(1, true);
            }
        }
        ApplyDamage(damage, false);
	}

    void ApplyDamage(int damage, bool allowRepeats)
    {
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, damageRadius, damageMask);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if(!allowRepeats && !damaged.Contains(collider))
            {
                damaged.Add(collider);
            }
            else
            {
                continue;
            }
            collider.SendMessageUpwards("OnDamage", damage);
            foreach (GameObject gobj in spawnOnDamage)
            {
                GameObject instance = (GameObject)Instantiate(gobj, collider.bounds.center, Quaternion.identity);
            }
        }
    }

    void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, damageRadius);
    }

}
