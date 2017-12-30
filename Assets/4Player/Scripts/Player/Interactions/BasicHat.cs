using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicHat : MonoBehaviour, IUsable
{
    public void Use(Transform target)
    {
        target.gameObject.GetComponent<Player>().visualModel.GetComponentInChildren<HatAnchor>().CurrentHat = gameObject;
    }
}
