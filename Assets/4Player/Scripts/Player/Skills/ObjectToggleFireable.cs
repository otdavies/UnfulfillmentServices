using UnityEngine;

[CreateAssetMenu(fileName = "ObjectToggleFireable", menuName = "Skills/Visuals/ObjectToggleFireable", order = 1)]
public class ObjectToggleFireable : Fireable
{
    [Header("Specialized Properties")]
    public GameObject prefabToToggle;
    protected GameObject fireableObject;
    public override void Setup(Player player)
    {
        fireableObject = Instantiate(prefabToToggle);
        fireableObject.transform.parent = player.transform;
        fireableObject.transform.localPosition = firePoint;
        fireableObject.transform.localEulerAngles = fireDirection;
        fireableObject.transform.localScale = Vector3.one * fireScale;
    }

    public override void Start()
    {
        fireableObject.SetActive(true);
    }

    public override void End()
    {
        fireableObject.SetActive(false);
    }
}
