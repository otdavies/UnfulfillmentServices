using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnableOnStart : MonoBehaviour
{
    public GameObject[] objects;
    public Behaviour[] components;

	void Start ()
    {
        foreach (GameObject g in objects)
        {
            g.SetActive(true);
        }

        foreach (Behaviour c in components)
        {
            c.enabled = true;
        }
	}
}
