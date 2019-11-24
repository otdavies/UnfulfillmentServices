using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateRandom : MonoBehaviour
{
    public GameObject[] activatees;
    public float maxTime = 60;
    public float minimumTime = 20;

    private float currentTarget = 15;
    private float currentTime = 0;

    private void Start()
    {
        foreach(GameObject g in activatees)
        {
            g.SetActive(false);
        }
    }

    void Update ()
    {
        currentTime += Time.deltaTime;
        if(currentTime > currentTarget)
        {
            currentTarget = Random.Range(minimumTime, maxTime);
            currentTime = 0;
            GameObject lastTarget = activatees[Random.Range(0, activatees.Length)];
            lastTarget.SetActive(true);
            lastTarget.SendMessage("Activated", SendMessageOptions.DontRequireReceiver);
            StartCoroutine(DisableAfter(3, lastTarget));
        }
    }

    IEnumerator DisableAfter(float f, GameObject target)
    {
        yield return new WaitForSeconds(f);
        target.SetActive(false);
    }
}
