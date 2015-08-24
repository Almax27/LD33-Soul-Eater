using UnityEngine;
using System.Collections;

public class RandomColourHelper : MonoBehaviour {

    [System.Serializable]
    public class RendererSet
    {
        public Renderer[] renderers = null;
        public Color[] colors = {Color.white};
    }
    public RendererSet[] rendererSets = new RendererSet[0];

	// Use this for initialization
	void Start () {
        foreach (RendererSet set in rendererSets)
        {
            if(set.colors.Length > 0)
            {
                Color color = set.colors[Random.Range(0, set.colors.Length)];
                foreach(Renderer renderer in set.renderers)
                {
                    renderer.material.color = color;
                }
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
