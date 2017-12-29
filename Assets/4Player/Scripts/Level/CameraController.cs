using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float cameraTrackBoundsPercent = 1;
    public float cameraFollowLerpSpeed = 20;
    public float cameraTrackBoundsCorrectionStrength = 4;
    public float cameraSmallestRestingDistance = 30;
    private Player[] players;
    private Vector3 startPosition;
    private Vector3 moveDirection = Vector3.zero;
    private Bounds trackingBox;
    private Camera camera;

    private Vector3 boxCenter;
    private float size = 1;

	// Use this for initialization
	private void Start ()
    {
        camera = GetComponent<Camera>();
        players = FindObjectsOfType<Player>();
        trackingBox.center = Vector3.one * 0.5f;
        trackingBox.extents = Vector3.Scale(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(cameraTrackBoundsPercent, cameraTrackBoundsPercent, 1));
        boxCenter = Vector3.zero;
    }
	
	// Update is called once per frame
	private void Update ()
    {
        moveDirection = Vector3.zero;

        float maxPlayerSeperation = 0;

        foreach (Player p in players)
        {
            Vector3 playerPosition = ((Vector2)camera.WorldToViewportPoint(p.transform.position));

            if (!trackingBox.Contains(playerPosition))
            {
                moveDirection += TranslateMovementVector(playerPosition - (trackingBox.ClosestPoint(playerPosition)));
            }

            foreach (Player p2 in players)
            {
                Vector3 player2Position = ((Vector2)camera.WorldToViewportPoint(p2.transform.position));
                float d = Vector2.Distance(player2Position, playerPosition);
                if (maxPlayerSeperation < d) maxPlayerSeperation = d;
            }
        }

        size = Mathf.Clamp(Mathf.Lerp(size, (1 / Mathf.Clamp01(1 - maxPlayerSeperation) - 1) * 0.25f, Time.deltaTime * 3), 1, 3);
        boxCenter += (moveDirection / players.Length) * cameraTrackBoundsCorrectionStrength * size;
    }

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, boxCenter - (transform.forward * cameraSmallestRestingDistance * size), Time.smoothDeltaTime * cameraFollowLerpSpeed);
    }

    private Vector3 TranslateMovementVector(Vector2 v2)
    {
        return v2.x * transform.right + v2.y * Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
    }
}
