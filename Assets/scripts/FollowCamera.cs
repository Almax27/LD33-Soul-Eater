using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

    public Transform target = null;
    public Vector3 targetOffset = new Vector3(0,0,0);
    public float lookDamp = 0.3f;
    public float followDamp = 0.5f;
    public Vector3 offset = new Vector3(0,2,-10);
    public float followDistance = 5.0f;

    bool isFollowing = true;
    Vector3 followVelocity = Vector3.zero;

    Vector3 desiredPosition = Vector3.zero;
    Vector3 lastTargetPosition = Vector3.zero;

    Vector3 lookAtPosition = Vector3.zero;
    Vector3 lookAtPositionVelocity = Vector3.zero;
    Vector3 lookAheadOffset = Vector3.zero;

	// Use this for initialization
	void Start () 
    {
        if (target)
        {
            desiredPosition = lastTargetPosition = target.position + targetOffset;
        }
	}
	
	// Update is called once per frame
	void LateUpdate () 
    {
        if (target != null)
        {
//            Vector3 targetDeltaStep = (target.position - lastTargetPosition);
//
//            //update following state
//            bool isOutsideRange = (target.position - desiredPosition).sqrMagnitude > followDistance*followDistance;
//            bool isTargetMoving = targetDeltaStep.sqrMagnitude > float.Epsilon;
//            Debug.Log("isTargetMoving: " + isTargetMoving + "  " + (target.position - lastTargetPosition).sqrMagnitude);
//            isFollowing = isTargetMoving && (isOutsideRange || isFollowing);
//
//            //follow target if following
//            if(isFollowing)
//            {
//                if(targetDeltaStep.sqrMagnitude > 0)
//                {
//                    lookAheadOffset = targetDeltaStep.normalized * followDistance * 0.8f;
//                }
//                desiredPosition = target.position + lookAheadOffset;
//            }
//
//            //smooth damp follow the target
//            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition + offset, ref followVelocity, lookDamp);
//            lookAtPosition = Vector3.SmoothDamp(lookAtPosition, desiredPosition, ref lookAtPositionVelocity, lookDamp);
//
//            transform.LookAt(lookAtPosition);
//
//            lastTargetPosition = target.position;

            desiredPosition = target.position + targetOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition + offset, ref followVelocity, followDamp);
            if(lookAtPosition == Vector3.zero)
            {
                lookAtPosition = desiredPosition;
            }
            else
            {
                lookAtPosition = Vector3.SmoothDamp(lookAtPosition, desiredPosition, ref lookAtPositionVelocity, lookDamp);
            }
            transform.LookAt(lookAtPosition);
        }
	}
}
