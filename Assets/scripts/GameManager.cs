using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;

    public int score = 0;
    public PlayerController playerPrefab = null;
    public FollowCamera followCamera = null;
    public SoulCollector playerSoul = null;

    public PlayerController player = null;

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
        SpawnPlayer();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("r"))
        {
            SpawnPlayer();
        }
	
	}

    void SpawnPlayer()
    {
        if (player)
        {
            player.SendMessage("OnDeath");
        }
        GameObject gobj = (GameObject)Instantiate(playerPrefab.gameObject);
        player = gobj.GetComponent<PlayerController>();

        playerSoul.target = player.chestNode.transform;

        if (followCamera)
        {
            followCamera.target = player.transform;
        }
    }
}
