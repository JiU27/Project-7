using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoomZone : MonoBehaviour
{
    [Tooltip("进入该区后相机的 Orthographic Size（越小越放大）")]
    public float targetSize = 4.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CameraFollow2D camFollow = Camera.main.GetComponent<CameraFollow2D>();
        if (camFollow != null)
        {
            camFollow.SetZoom(targetSize);
        }
    }
}
