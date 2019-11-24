using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berry : MonoBehaviour
{
    private Rigidbody berry;
    private Light glowLight;
    private Color initialColor;
    private float initialIntensity;

    void Start ()
    {
        berry = GetComponent<Rigidbody>();
        berry.Sleep();

        glowLight = GetComponentInChildren<Light>();
        initialColor = glowLight.color;
        initialIntensity = glowLight.intensity;

    }

    void OnPressurePlateDown()
    {
        glowLight.color = Color.green;
        glowLight.intensity = initialIntensity * 0.75f;
    }

    void OnPressurePlateUp()
    {
        glowLight.color = initialColor;
        glowLight.intensity = initialIntensity;
    }
}
