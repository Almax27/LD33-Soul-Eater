using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

    public int maxHealth = 10;
    public int currentHealth = 10;

    public GameObject[] spawnOnDeath = new GameObject[0];

    bool isDead = false;

    void Start()
    {
        Reset();
    }

    void Reset()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

	void OnDamage(int damage)
    {
        currentHealth -= damage;
        if (!isDead && currentHealth < 0)
        {
            currentHealth = 0;
            isDead = true;
            SendMessage("OnDeath");
        }
    }

    void OnDeath()
    {
        foreach (GameObject gobj in spawnOnDeath)
        {
            GameObject instance = (GameObject)Instantiate(gobj, this.transform.position, this.transform.rotation);
        }
    }
}
