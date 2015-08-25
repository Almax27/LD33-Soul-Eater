using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class GlobalLightController : MonoBehaviour {

    public float smoothDamp = 0.5f;

    public Light targetLight = null;
    Color colorVel = Color.black;
    float intensityVel = 0;

	void SetTargetLight(Object obj)
    {
        if (obj.GetType() == typeof(Light))
        {
            targetLight = (Light)obj;
        } else if (obj.GetType() == typeof(GameObject))
        {
            targetLight = ((GameObject)obj).GetComponent<Light>();
        }
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (targetLight != null)
        {
            Light light = GetComponent<Light>();

            //smooth color
            Color color = light.color;
            color.r = Mathf.SmoothDamp(color.r, targetLight.color.r, ref colorVel.r, smoothDamp);
            color.g = Mathf.SmoothDamp(color.g, targetLight.color.g, ref colorVel.g, smoothDamp);
            color.b = Mathf.SmoothDamp(color.b, targetLight.color.b, ref colorVel.b, smoothDamp);
            light.color = color;

            //smooth intensity
            light.intensity = Mathf.SmoothDamp(light.intensity, targetLight.intensity, ref  intensityVel, smoothDamp);
        }
	}
}
