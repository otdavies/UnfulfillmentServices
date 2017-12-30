using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxScalar : MonoBehaviour
{
	void OnEnable ()
    {
        transform.localScale = Vector3.one + Random.onUnitSphere * 0.5f;
	}
}
