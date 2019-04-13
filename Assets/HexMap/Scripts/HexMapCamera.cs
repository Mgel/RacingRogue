using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapCamera : MonoBehaviour {

    static HexMapCamera instance;

    public HexGrid grid;

    Transform swivel, stick;

    float zoom = 1f;
    float zoomAdjust;
    public float stickMinZoom, stickMaxZoom;
    public float moveSpeedMinZoom, moveSpeedMaxZoom;
    public float maxPitch, minPitch;
    public bool invertPitch;

    float yaw, pitch;

    float rotationAngle;
    public float rotationSpeed;

    void Awake()
    {
        instance = this;
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
    }

    void Update()
    {
        float camDelta = Input.GetAxis("Fire2");
        if (camDelta != 0f)
        {
            float yaw = Input.GetAxis("Mouse X");
            float pitch = Input.GetAxis("Mouse Y");
            if (yaw != 0f || pitch != 0f)
            {
                AdjustYaw(yaw);
                AdjustPitch(pitch);
            }
        }

        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        float rotationDelta = Input.GetAxis("Rotation");
        if (rotationDelta != 0f)
        {
            AdjustYaw(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f)
        {
            AdjustPosition(xDelta, zDelta);
        }
    }

    public static bool Locked
    {
        set { instance.enabled = !value; }
    }

    public static void ValidatePosition()
    {
        instance.AdjustPosition(0f, 0f);
        instance.AdjustZoom(0f);
    }

    #region Zoom/Movement
    void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);
        float zoomAdjust = (grid.cellCountX / 20.0f) / (grid.cellCountX / (float)grid.cellCountZ);

        float distance = Mathf.Lerp(stickMinZoom * zoomAdjust, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        // Adjust angle of camera when zooming
        //float angle = Mathf.Lerp(maxPitch, minPitch, zoom);
        //swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = ClampPosition(position);
    }

    Vector3 ClampPosition(Vector3 position)
    {
        float xMax = (grid.cellCountX - 0.5f) * (2f * HexMetrics.innerRadius);
        position.x = Mathf.Clamp(position.x, 0f, xMax);

        float zMax = (grid.cellCountZ - 1) * (1.5f * HexMetrics.outerRadius);
        position.z = Mathf.Clamp(position.z, 0f, zMax);

        return position;
    }
    #endregion

    #region Rotation
    void AdjustYaw(float delta)
    {
        yaw += delta * rotationSpeed * Time.deltaTime;

        if (yaw < 0f) { yaw += 360f; }
        else if (yaw >= 360f) { yaw -= 360f; }

        transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void AdjustPitch(float delta)
    {        
        if (invertPitch)
        {
            pitch -= delta * rotationSpeed * Time.deltaTime;
        }
        else { pitch += delta * rotationSpeed * Time.deltaTime; }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        swivel.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
    #endregion
}
