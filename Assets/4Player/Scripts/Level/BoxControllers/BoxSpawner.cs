using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    public float minimumTimeBetweenSpawns = 3;
    public float maximumTimeBetweenSpawns = 6;

    private float timePast = 0;
    private float timeThreshold = 0;

	void Start ()
    {
        timeThreshold = Random.Range(minimumTimeBetweenSpawns, maximumTimeBetweenSpawns);
    }
	
	void Update ()
    {
        if (timeThreshold < timePast)
        {
            timeThreshold = Random.Range(minimumTimeBetweenSpawns, maximumTimeBetweenSpawns);
            timePast = 0;
            SpawnBox();
        }
        timePast += Time.deltaTime;
    }

    void SpawnBox()
    {
        PoolableFactory.Instance.Create<BasicPoolable>("Box", transform.position + Random.insideUnitSphere * 0.5f, Quaternion.identity, this.transform);
    }
}
