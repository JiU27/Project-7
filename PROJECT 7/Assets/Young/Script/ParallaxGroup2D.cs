using System;
using UnityEngine;

/// 放在 Main Camera（含 CinemachineBrain）之后执行，拿到最终相机位置
[DefaultExecutionOrder(1000)]
public class ParallaxGroup2D : MonoBehaviour
{
    [Serializable]
    public class ParallaxLayer
    {
        [Header("层根节点（拖你的层物体）")]
        public Transform layer;

        [Header("视差系数（远小近大）：X=横向  Y=纵向")]
        [Tooltip("0=跟相机一起动；1=几乎不动（天空更远可用更小，如0.1~0.3）；>1=前景略快")]
        public Vector2 factor = new Vector2(0.5f, 0f);

        [Header("可选：无限横向滚动")]
        public bool tileX = false;

        [Tooltip("该层一张贴图的世界宽度；留空或<=0会自动从SpriteRenderer.bounds尝试检测")]
        public float tileWidth = 0f;
    }

    [Header("相机（不填则自动找 MainCamera）")]
    public Camera cam;

    [Header("是否启用纵向视差（平台跳跃可先关）")]
    public bool enableY = false;

    [Header("缩放补偿（区域放大/缩小时保持手感更一致）")]
    public bool compensateForZoom = false;
    public float zoomReferenceSize = 6f;  // 你的默认 Orthographic Size

    [Header("视差层列表（想加多少就加多少）")]
    public ParallaxLayer[] layers;

    private Vector3 _lastCamPos;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        _lastCamPos = cam ? cam.transform.position : Vector3.zero;

        // 自动检测 tileWidth
        for (int i = 0; i < (layers?.Length ?? 0); i++)
        {
            var e = layers[i];
            if (!e.layer) continue;
            if (e.tileX && e.tileWidth <= 0f)
            {
                var sr = e.layer.GetComponentInChildren<SpriteRenderer>();
                if (sr) e.tileWidth = sr.bounds.size.x;
            }
        }
    }

    void LateUpdate()
    {
        if (!cam || layers == null) return;

        Vector3 camPos = cam.transform.position;
        Vector3 delta = camPos - _lastCamPos;

        // 缩放补偿
        float zoomScale = 1f;
        if (compensateForZoom && cam.orthographicSize > 0.0001f)
            zoomScale = zoomReferenceSize / cam.orthographicSize;

        for (int i = 0; i < layers.Length; i++)
        {
            var e = layers[i];
            if (!e.layer) continue;

            float fx = e.factor.x * zoomScale;
            float fy = enableY ? e.factor.y * zoomScale : 0f;

            e.layer.position += new Vector3(delta.x * fx, delta.y * fy, 0f);

            // 无限横向滚动（整层搬移一个 tile 的距离）
            if (e.tileX && e.tileWidth > 0.0001f)
            {
                float dist = camPos.x - e.layer.position.x;
                if (Mathf.Abs(dist) >= e.tileWidth)
                {
                    float shift = Mathf.Floor(dist / e.tileWidth) * e.tileWidth;
                    e.layer.position += new Vector3(shift, 0f, 0f);
                }
            }
        }

        _lastCamPos = camPos;
    }

    // 在 Inspector 的三点菜单里可以手动点这个来自动填宽度
    [ContextMenu("Auto Fill Tile Widths")]
    void AutoFillTileWidths()
    {
        for (int i = 0; i < (layers?.Length ?? 0); i++)
        {
            var e = layers[i];
            if (!e.layer) continue;
            var sr = e.layer.GetComponentInChildren<SpriteRenderer>();
            if (sr) e.tileWidth = sr.bounds.size.x;
        }
    }
}
