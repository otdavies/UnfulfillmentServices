using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour {

    public Light directionalLight;
    public Material glowMat;
    public float cycleTime = 300;

    private float currentCycleTime = 90;
    private Vector3 lightDirTarget;

	// Use this for initialization
	void OnEnable ()
    {
        StartCoroutine(UpdateEachSecond());
	}

    IEnumerator UpdateEachSecond()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
        }
    }

    private void FixedUpdate()
    { 
        currentCycleTime += Time.fixedDeltaTime * (60 / cycleTime);
        lightDirTarget.Set(currentCycleTime, -30, 0);
        directionalLight.transform.eulerAngles = lightDirTarget;
        glowMat.SetFloat("_shine", (1 - Mathf.Sin(currentCycleTime * Mathf.Deg2Rad) + 2.4f) * 1.3f);
    }
}
