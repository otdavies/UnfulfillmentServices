using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertLight : MonoBehaviour
{
    public GameObject target;

    public void Activated()
    {
        target.SendMessage("ActivateLight");
    }
}
