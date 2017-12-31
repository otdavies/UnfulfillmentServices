using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

[RequireComponent(typeof(Collider))]
public class AddPlayerPoint : MonoBehaviour
{
    public XboxController controllerPointReceiver;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("dynamic"))
        {
            if (other.gameObject.GetComponent<BoxScalar>())
            {
                Player pointReceiver = PlayerManager.Instance.GetPlayerByController(controllerPointReceiver);
                if (pointReceiver != null)
                {
                    GameManager.Instance.AddScore(pointReceiver, 1);
                    other.gameObject.GetComponent<IPoolable>().Destroy();
                }
            }
        }
    }
}
