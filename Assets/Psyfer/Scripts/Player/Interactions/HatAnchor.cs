using UnityEngine;

public class HatAnchor : MonoBehaviour
{
    private GameObject currentHat;
    private Rigidbody currentHatRigid;
    private Collider[] currentHatColliders;
    private AntiJitter antiJitter;

    public GameObject CurrentHat
    {
        set
        {
            if (currentHat != null)
            {
                currentHat.transform.parent = null;
                ToggleColliders(true);
                currentHatRigid.isKinematic = false;
                currentHatRigid.AddForce((Vector3.up + Random.onUnitSphere) * 10, ForceMode.Impulse);
                antiJitter.enabled = true;
            }


            currentHat = value;

            currentHatColliders = currentHat.GetComponents<Collider>();
            currentHatRigid = currentHat.GetComponent<Rigidbody>();
            antiJitter = currentHat.GetComponent<AntiJitter>();

            currentHatRigid.isKinematic = true;
            ToggleColliders(false);
            antiJitter.ResetPosition();
            antiJitter.enabled = false;

            currentHat.transform.SetParent(transform, true);
            currentHat.transform.localPosition = Vector3.zero;
            currentHat.transform.localRotation = Quaternion.identity;
        }
    }

    private void ToggleColliders(bool state)
    {
        for(int i = 0; i < currentHatColliders.Length; i++)
        {
            currentHatColliders[i].enabled = state;
        }
    }
}
