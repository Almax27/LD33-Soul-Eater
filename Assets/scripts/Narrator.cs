using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Narrator : MonoBehaviour {

    public Text text = null;
    public float readRate = 0.3f;
    public float delayBetweenNarations = 2.0f;
    public float radius = 1.0f;
    public LayerMask layerMask = new LayerMask();

    
    List<string> pendingNarrations = new List<string>();
    string currentText = "";
    int currentCharacter = -1;
    float tick = 0;
    bool finishedReading = true;
    bool readingFinal = false;
	
    void Awake()
    {
        text.text = currentText;
    }

	// Update is called once per frame
	void Update () 
    {
        tick += Time.deltaTime;
        ReadText();
        TryReadNext();
	}

    void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask);
        foreach (Collider col in colliders)
        {
            Narrate(col.GetComponent<Narration>());
        }
    }

    void ReadText()
    {
        while (tick > readRate && currentCharacter < currentText.Length)
        {
            tick = 0;
            currentCharacter++;
            text.text = currentText.Substring(0, currentCharacter);
        }
    }

    void TryReadNext()
    {
        bool finishedReading = currentCharacter >= currentText.Length - 1;
        if (finishedReading && tick > delayBetweenNarations)
        {
            if (pendingNarrations.Count > 0)
            {
                currentText = pendingNarrations[0];
                pendingNarrations.RemoveAt(0);
                currentCharacter = -1;
            }
            else
            {
                text.text = "";
                if(readingFinal)
                {
                    //reload
                    Application.LoadLevel(Application.loadedLevel);
                }
            }
        }

    }

    void Narrate(Narration narration)
    {
        if (narration)
        {
            for(int i = 0; i < narration.content.Length; i++)
            {
                string text = narration.content[i];
                text = text.Replace("\\n", "\n");
                Debug.Log("Narrating: " + text);
                pendingNarrations.Add(text);
            }
            readingFinal = narration.isFinal;
            Destroy(narration);
        }
    }
}
