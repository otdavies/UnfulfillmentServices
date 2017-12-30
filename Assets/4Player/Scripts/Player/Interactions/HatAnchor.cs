using UnityEngine;

public class HatAnchor : MonoBehaviour
{
    private GameObject currentHat;
    private Rigidbody currentHatRigid;
    private Collider currentHatCollider;
    private AntiJitter antiJitter;

    public GameObject CurrentHat
    {
        set
        {
            if (currentHat != null)
            {
                currentHat.transform.parent = null;
                currentHatCollider.enabled = true;
                currentHatRigid.isKinematic = false;
                currentHatRigid.AddForce((Vector3.up + Random.onUnitSphere) * 10, ForceMode.Impulse);
                antiJitter.enabled = true;
            }


            currentHat = value;

            currentHatCollider = currentHat.GetComponent<Collider>();
            currentHatRigid = currentHat.GetComponent<Rigidbody>();
            antiJitter = currentHat.GetComponent<AntiJitter>();

            currentHatRigid.isKinematic = true;
            currentHatCollider.enabled = false;
            antiJitter.ResetPosition();
            antiJitter.enabled = false;

            currentHat.transform.SetParent(transform, true);
            currentHat.transform.localPosition = Vector3.zero;
            currentHat.transform.localRotation = Quaternion.identity;
        }
    }
}
