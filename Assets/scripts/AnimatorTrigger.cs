using UnityEngine;
using System.Collections;

public class AnimatorTrigger : MonoBehaviour {

    public Animator animator = null;
    public string[] triggersOnEnter = new string[0];
    public string[] triggersOnExit = new string[0];

	// Use this for initialization
	void Start () 
    {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (animator)
        {
            foreach (string trigger in triggersOnEnter)
            {
                animator.SetTrigger(trigger);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (animator)
        {
            foreach (string trigger in triggersOnExit)
            {
                animator.SetTrigger(trigger);
            }
        }
    }
}
