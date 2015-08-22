using UnityEngine;
using System.Collections;

public class SoulCollector : MonoBehaviour {

    public float attractionRadius = 2.0f;
    public float collectionRadius = 0.1f;
    public LayerMask layerMask = new LayerMask();


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        //handle attraction
        Collider[] attractables = Physics.OverlapSphere(transform.position, attractionRadius, layerMask);
        foreach (Collider collider in attractables)
        {
            Soul soul = collider.GetComponent<Soul>();
            if(soul)
            {
                soul.target = this.transform;
            }
        }

        //handle collection
        Collider[] collectables = Physics.OverlapSphere(transform.position, collectionRadius, layerMask);
        foreach (Collider collider in collectables)
        {
            Soul soul = collider.GetComponent<Soul>();
            if(soul && soul.isCollectable && soul.isCollected == false)
            {
                soul.isCollected = true;
                GameManager.instance.score += soul.soulValue;
            }
        }
	}
}
