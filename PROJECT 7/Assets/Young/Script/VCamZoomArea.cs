using System.Collections;
using UnityEngine;
using Cinemachine;

public class VCamZoomArea : MonoBehaviour
{
    [Header("引用")]
    public CinemachineVirtualCamera vcam;   // 拖你的基础 vCam（Follow=Player 的那台）
    public string playerTag = "Player";

    [Header("进入本区域后的相机正交尺寸（越小越放大）")]
    public float insideSize = 4.5f;

    [Header("过渡时长（秒）")]
    public float zoomDuration = 0.35f;

    float _prevSize;
    Coroutine _running;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag) || vcam == null) return;
        _prevSize = vcam.m_Lens.OrthographicSize;             // 记住进入前的尺寸
        StartZoom(insideSize);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag) || vcam == null) return;
        StartZoom(_prevSize);                                  // 离开恢复
    }

    void StartZoom(float target)
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(CoZoomTo(target, zoomDuration));
    }

    IEnumerator CoZoomTo(float size, float dur)
    {
        var lens = vcam.m_Lens;
        float start = lens.OrthographicSize;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / dur);
            lens.OrthographicSize = Mathf.Lerp(start, size, k);
            vcam.m_Lens = lens; // 写回
            yield return null;
        }
        lens.OrthographicSize = size;
        vcam.m_Lens = lens;
    }
}
