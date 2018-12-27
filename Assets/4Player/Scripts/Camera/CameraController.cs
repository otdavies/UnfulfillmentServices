using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour, Observer<Player[]>
{
    [Header("Bounds Params")]
    public float cameraTrackBoundsPercent = 1;
    public float cameraTrackBoundsCorrectionStrength = 4;
    public float cameraSmallestRestingDistance = 30;
    public float cameraMaxZoomIn = 1;
    public float cameraMaxZoomOut = 3;
    public float cameraZoomBoundsPercent = 1;

    [Header("Speed Params")]
    public float cameraFollowLerpSpeed = 20;
    public float cameraZoomLerpSpeed = 3;
    public float cameraBoxCenterLerpSpeed = 1;

    private Player[] players;
    private Vector3 startPosition;
    private Bounds trackingBox;
    private Camera cam;

    private Vector3 boxCenter = Vector3.zero;
    private float size = 1;

	private void Start ()
    {
        cam = GetComponent<Camera>();
        players = FindObjectsOfType<Player>();

        PlayerManager.Instance.AddObserver(this);

        // Setup tracking extent options
        trackingBox.center = Vector3.one * 0.5f;
        trackingBox.extents = Vector3.Scale(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(cameraTrackBoundsPercent, cameraTrackBoundsPercent, 1));
        boxCenter = Vector3.zero;
    }
	
	private void Update ()
    {
        if (players.Length < 1) return;


        Vector3 moveDirection = Vector3.zero;
        Vector3 playerCenter = Vector3.zero;
        float maxPlayerSeperation = 0;

        foreach (Player p in players)
        {
            playerCenter += p.transform.position;

            Vector3 playerPosition = ((Vector2)cam.WorldToViewportPoint(p.transform.position));
            if (!trackingBox.Contains(playerPosition))
            {
                moveDirection += TranslateMovementVector(playerPosition - (trackingBox.ClosestPoint(playerPosition)));
            }

            foreach (Player p2 in players)
            {
                Vector3 player2Position = ((Vector2)cam.WorldToViewportPoint(p2.transform.position));
                float d = Vector2.Distance(player2Position, playerPosition);
                if (maxPlayerSeperation < d) maxPlayerSeperation = d;
            }
        }

        size = Mathf.Clamp(Mathf.Lerp(size, (1 / Mathf.Clamp(cameraZoomBoundsPercent - maxPlayerSeperation, 0.01f, 1) - 1), Time.deltaTime * cameraZoomLerpSpeed), cameraMaxZoomIn, cameraMaxZoomOut);
        boxCenter += moveDirection / players.Length * cameraTrackBoundsCorrectionStrength * size;
        boxCenter = Vector3.Lerp(boxCenter, boxCenter + ((playerCenter / players.Length) - boxCenter), Time.deltaTime * cameraBoxCenterLerpSpeed);
    }

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, boxCenter - (transform.forward * cameraSmallestRestingDistance * size), Time.smoothDeltaTime * cameraFollowLerpSpeed);
    }

    private Vector3 TranslateMovementVector(Vector2 v2)
    {
        return v2.x * transform.right + v2.y * Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
    }

    public void Notified(Player[] data)
    {
        players = data;
    }
}
