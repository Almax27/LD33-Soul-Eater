using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

    public Transform target = null;
    public float lookDamp = 0.3f;
    public float fixedZ = -10.0f;

    Vector3 followVelocity = Vector3.zero;

    bool snapOnce = true;

	// Use this for initialization
	void Start () 
    {
	}
	
	// Update is called once per frame
	void LateUpdate () 
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position;

            //smooth damp follow the target
            Vector3 pos = Vector3.SmoothDamp(transform.position, targetPosition, ref followVelocity, lookDamp);
            pos.z = fixedZ;
            transform.position = pos;

            transform.LookAt(target);
        }
	}
}
