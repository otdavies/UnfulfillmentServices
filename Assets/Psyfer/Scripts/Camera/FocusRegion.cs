using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

[RequireComponent(typeof(PostProcessingBehaviour))]
public class FocusRegion : MonoBehaviour
{
    public GameObject focusTarget;
    private Camera postProcessingCamera;
    private PostProcessingBehaviour pp;
    private DepthOfFieldModel.Settings model;

    private void Start ()
    {
        postProcessingCamera = GetComponent<Camera>();
        pp = GetComponent<PostProcessingBehaviour>();
        model = pp.profile.depthOfField.settings;
    }

    private void Update ()
    {
        Vector3 playerPosition = ProjectPointOnPlane(this.transform.forward, this.transform.position, focusTarget.transform.position);
        model.focusDistance = Mathf.Lerp(model.focusDistance, Vector3.Distance(focusTarget.transform.position, playerPosition) - (postProcessingCamera.orthographic ? 9 : 0), Time.deltaTime * 5);
        pp.profile.depthOfField.settings = model;
    }

    private Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
    {

        float distance;
        Vector3 translationVector;

        //First calculate the pullDistance from the point to the plane:
        distance = SignedDistancePlanePoint(planeNormal, planePoint, point);

        //Reverse the sign of the pullDistance
        distance *= -1;

        //Get a translation vector
        translationVector = SetVectorLength(planeNormal, distance);

        //Translate the point to form a projection
        return point + translationVector;
    }

    private float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
    {

        return Vector3.Dot(planeNormal, (point - planePoint));
    }

    private Vector3 SetVectorLength(Vector3 vector, float size)
    {

        //normalize the vector
        Vector3 vectorNormalized = Vector3.Normalize(vector);

        //scale the vector
        return vectorNormalized *= size;
    }
}
