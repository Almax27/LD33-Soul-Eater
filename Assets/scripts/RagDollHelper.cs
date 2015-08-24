using UnityEngine;
using System.Collections;

public class RagDollHelper : MonoBehaviour {

    public bool ragDollOnWake = false;

	// Use this for initialization
	void Awake () {
        SetIsRagDoll(ragDollOnWake);
	}

    #region Rag Doll Logic
    
    public void SetIsRagDoll(bool isRagDoll)
    {
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].isKinematic = !isRagDoll;
        }
        Animator[] animators = GetComponentsInChildren<Animator>();
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].enabled = !isRagDoll;
        }
    }
    
    #endregion
}
