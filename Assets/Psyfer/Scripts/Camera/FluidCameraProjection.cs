using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FluidCameraProjection : MonoBehaviour
{
    public float ProjectionChangeTime = 0.5f;
    public bool ChangeProjection = false;

    private Camera camera;
    private bool _changing = false;
    private float _currentT = 0.0f;

    private void Start()
    {
        camera = GetComponent<Camera>();
        camera.orthographicSize = 8;
    }

    public void SwitchViewType()
    {
        ChangeProjection = !ChangeProjection;
    }

    private void Update()
    {
        if (_changing)
        {
            ChangeProjection = false;
        }
        else if (ChangeProjection)
        {
            _changing = true;
            _currentT = 0.0f;
        }
    }

    private void LateUpdate()
    {
        if (!_changing)
        {
            return;
        }

        var currentlyOrthographic = camera.orthographic;
        Matrix4x4 orthoMat, persMat;
        if (currentlyOrthographic)
        {
            orthoMat = camera.projectionMatrix;

            camera.orthographic = false;
            camera.ResetProjectionMatrix();
            persMat = camera.projectionMatrix;
        }
        else
        {
            persMat = camera.projectionMatrix;

            camera.orthographic = true;
            camera.ResetProjectionMatrix();
            orthoMat = camera.projectionMatrix;
        }
        camera.orthographic = currentlyOrthographic;

        if (_currentT <= 1.0f)
        {
            _currentT += (Time.deltaTime / ProjectionChangeTime);
            float t = _currentT;
            t = t * t * t * (t * (6f * t - 15f) + 10f);
            if (currentlyOrthographic)
            {
                camera.projectionMatrix = MatrixLerp(orthoMat, persMat, t);
            }
            else
            {
                camera.projectionMatrix = MatrixLerp(persMat, orthoMat, t);
            }
        }
        else
        {
            _changing = false;
            camera.orthographic = !currentlyOrthographic;
            camera.ResetProjectionMatrix();
        }
    }

    private Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        var newMatrix = new Matrix4x4();
        newMatrix.SetRow(0, Vector4.Lerp(from.GetRow(0), to.GetRow(0), t));
        newMatrix.SetRow(1, Vector4.Lerp(from.GetRow(1), to.GetRow(1), t));
        newMatrix.SetRow(2, Vector4.Lerp(from.GetRow(2), to.GetRow(2), t));
        newMatrix.SetRow(3, Vector4.Lerp(from.GetRow(3), to.GetRow(3), t));
        return newMatrix;
    }
}