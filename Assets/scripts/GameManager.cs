using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;

    public int score = 0;
    public PlayerController playerPrefab = null;
    public FollowCamera followCamera = null;
    public SoulCollector playerSoul = null;
    public Transform currentSpawnPoint = null;

    public Transform firstWingSpawn = null;
    public Transform secondWingSpawn = null;
    public Transform armsAndSwordSpawn = null;

    public PlayerController player = null;

    [Header("UI")]
    public Text heartText = null;
    public Text soulText = null;

    void Awake()
    {
        Debug.Assert(FindObjectsOfType<GameManager>().Length == 1, "There should only be one GameManager!");
        SpawnPlayer();
    }

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        UpdateUI();
        HandleDebugInput();
        if (player == null)
        {
            SpawnPlayer();
        }
	}

    void UpdateUI()
    {
        if (heartText)
        {
            string heartString = "-";
            if(player)
            {
                Health playerHealth = player.GetComponent<Health>();
                if(playerHealth)
                {
                    heartString = string.Format("{0}/{1}", playerHealth.currentHealth, playerHealth.maxHealth);
                }
            }
            heartText.text = heartString;
        }
        if (soulText)
        {
            string soulString = "-";
            if(player)
            {
                soulString = score.ToString();
            }
            soulText.text = soulString;
        }
    }

    public void SpawnPlayer()
    {
        if (player)
        {
            player.SendMessage("OnDeath");
        }
        if (currentSpawnPoint)
        {
            GameObject gobj = (GameObject)Instantiate(playerPrefab.gameObject, currentSpawnPoint.position, currentSpawnPoint.rotation);
            player = gobj.GetComponent<PlayerController>();
        }

        if (player)
        {
            playerSoul.target = player.chestNode.transform;

            if (followCamera)
            {
                followCamera.target = player.transform;
            }

            player.firstWing.SetUnlockedState(firstWingUnlocked);
            player.secondWing.SetUnlockedState(secondWingUnlocked);
            player.armsAndSword.SetUnlockedState(armsAndSwordUnlocked);
        }
    }

    bool firstWingUnlocked = false;
    bool secondWingUnlocked = false;
    bool armsAndSwordUnlocked = false;
    public void Unlock(Unlockable.Type type)
    {
        switch (type)
        {
            case Unlockable.Type.FirstWing:
            {
                firstWingUnlocked = true;
                if(player)
                {
                    player.firstWing.SetUnlockedState(true);
                }
                currentSpawnPoint = firstWingSpawn;
                break;
            }
            case Unlockable.Type.SecondWing:
            {
                secondWingUnlocked= true;
                if(player)
                {
                    player.secondWing.SetUnlockedState(true);
                }
                currentSpawnPoint = secondWingSpawn;
                break;
            }
            case Unlockable.Type.ArmsAndSword:
            {
                armsAndSwordUnlocked = true;
                if(player)
                {
                    player.armsAndSword.SetUnlockedState(true);
                }
                currentSpawnPoint = armsAndSwordSpawn;
                break;
            }
        }
    }

    void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SpawnPlayer();
        }
        if(player)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                player.firstWing.SetUnlockedState(true);
                firstWingUnlocked = true;
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                player.secondWing.SetUnlockedState(true);
                secondWingUnlocked = true;
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                player.armsAndSword.SetUnlockedState(true);
                armsAndSwordUnlocked = true;
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                Application.LoadLevel("game");
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Application.LoadLevel("testScene");
            }
        }
    }
}
