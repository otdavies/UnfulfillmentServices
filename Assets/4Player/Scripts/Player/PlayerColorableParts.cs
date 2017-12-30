using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorableParts : MonoBehaviour
{
    public Material PartMaterial
    {
        set
        {
            GetComponent<MeshRenderer>().material = value;
        }
    }
}
