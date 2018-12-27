using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScreenSpaceCamera : MonoBehaviour, Observer<Player[]>
{
    private Player[] players;
    private Camera gameCamera;

	private void Start()
    {
        players = FindObjectsOfType<Player>();
        PlayerManager.Instance.AddObserver(this);
        gameCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (players.Length < 1) return;

        foreach (Player p in players)
        {
            Vector3 playerPosition = (Vector2)gameCamera.WorldToViewportPoint(p.transform.position);
        }
    }

    private void LateUpdate()
    {

    }

    public void Notified(Player[] data)
    {
        players = data;
    }
}
