using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector2 offset = new Vector2(0f, 1f);
    [Tooltip("越小越跟得紧")]
    public float smoothTime = 0.15f;
    [Tooltip("死区，主角在死区内相机不动，避免抖动")]
    public Vector2 deadzone = new Vector2(0.2f, 0.1f);

    [Header("（可选）边界")]
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("缩放")]
    public float zoomSmoothTime = 0.2f;

    private Vector3 v; // SmoothDamp 速度缓存
    private Camera cam;
    private float targetSize;
    private float zoomVel; // SmoothDamp 速度缓存

    void Awake()
    {
        cam = GetComponent<Camera>();
        targetSize = cam.orthographicSize;
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desired = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        Vector3 pos = transform.position;

        if (Mathf.Abs(desired.x - pos.x) > deadzone.x)
            pos.x = Mathf.SmoothDamp(pos.x, desired.x, ref v.x, smoothTime);

        if (Mathf.Abs(desired.y - pos.y) > deadzone.y)
            pos.y = Mathf.SmoothDamp(pos.y, desired.y, ref v.y, smoothTime);

        if (useBounds)
        {
            pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
            pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        }

        transform.position = pos;

        // 平滑缩放
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref zoomVel, zoomSmoothTime);
    }

    // 提供外部调用
    public void SetZoom(float size)
    {
        targetSize = Mathf.Max(0.01f, size);
    }
}
