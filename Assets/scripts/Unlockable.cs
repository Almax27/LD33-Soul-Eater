using UnityEngine;
using System.Collections;

public class Unlockable : MonoBehaviour {
   
    [System.Serializable]
    public enum Type
    {
        FirstWing,
        SecondWing,
        ArmsAndSword
    }
    public Type type = Type.FirstWing;

    void OnTriggerEnter(Collider other)
    {
        FindObjectOfType<GameManager>().Unlock(type);
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject);
    }
}
