using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAlertLight : MonoBehaviour
{
    public Material lightOn;
    public Material lightOff;
    public MeshRenderer lightCones;
    public Light[] lights;

    private void Start()
    {
        lights = GetComponentsInChildren<Light>();
        lightCones.material = lightOff;
        ToggleLights(false);
    }

    public void ActivateLight()
    {
        StartCoroutine(RotateForTime(6));
        StartCoroutine(TurnOnLightsForTime(6));
    }

    private void ToggleLights(bool state)
    {
        foreach(Light l in lights)
        {
            l.enabled = state;
        }
    }

    IEnumerator TurnOnLightsForTime(float f)
    {
        lightCones.material = lightOn;
        ToggleLights(true);
        yield return new WaitForSeconds(f);
        lightCones.material = lightOff;
        ToggleLights(false);
    }

    IEnumerator RotateForTime(float f)
    {
        float currentTime = 0;
        while (f > currentTime)
        {
            currentTime += Time.deltaTime;
            transform.Rotate(Vector3.up, (Time.deltaTime) * 180);
            yield return new WaitForEndOfFrame();
        }
    }

}
