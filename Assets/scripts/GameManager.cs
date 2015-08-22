using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;

    public int score = 0;

    void Awake()
    {
        Debug.Assert(instance == null, "There can only be one GameManager!");
        if (instance != null)
        {
            Destroy(this);
        } else
        {
            instance = this;
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
