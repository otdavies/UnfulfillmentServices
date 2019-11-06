using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable
{
    void Instantiate(Vector3 position, Quaternion rotation);
    void Destroy();
    void SetResourceBase(string resource);
    string GetResourceBase();
}
