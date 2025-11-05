using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OneFileDialogue : MonoBehaviour
{
    // ============== 数据：一句台词 ==============
    public enum PortraitSide { Left, Right }

    [System.Serializable]
    public class Line
    {
        public string speaker;
        [TextArea(2, 5)] public string text;
        public Sprite portrait;
        public AudioClip voice;

        [Header("（可选）该句立绘左右站位")]
        public bool overrideSide = false;
        public PortraitSide side = PortraitSide.Left;

        [Header("（可选）该句立绘自定义矩形 (0~1, 面板内归一化)")]
        public bool overridePortraitRect = false;
        public Rect portraitRect01 = new Rect(0f, 0f, 0.25f, 1f); // x,y,width,height ∈ [0,1]
    }

    // ============== 文本样式 ==============
    [System.Serializable]
    public class TextStyle
    {
        public TMP_FontAsset font;
        public int fontSize = 36;
        public Color color = Color.white;
        public TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft;
        public bool wordWrapping = true;
        public float lineSpacing = 0f;
        public FontStyles fontStyle = FontStyles.Normal;
    }

    // ============== 配置 ==============
    [Header("对话内容（按顺序）")]
    public List<Line> lines = new List<Line>();

    [Header("触发设置")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;

    [Header("播放/输入")]
    public float charsPerSecond = 45f;
    public KeyCode advanceKey = KeyCode.Space;
    public KeyCode altAdvanceKey = KeyCode.E;
    public AudioClip advanceClip;
    [Range(0f, 1f)] public float advanceVolume = 0.6f;

    [Header("对话时禁用（拖 PlayerController 等）")]
    public List<Behaviour> disableWhileOpen = new List<Behaviour>();

    [Header("UI 布局（自动生成模式）")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    public Color panelColor = new Color(0f, 0f, 0f, 0.6f);
    [Tooltip("面板位置：左下/右上锚点（0-1）")]
    public Vector2 panelAnchorMin = new Vector2(0.05f, 0.08f);
    public Vector2 panelAnchorMax = new Vector2(0.95f, 0.35f);

    [Header("立绘布局（两种其一）")]
    [Tooltip("关闭=按宽度百分比自动布局；开启=手动输入归一化矩形")]
    public bool useCustomPortraitRect = false;
    [Tooltip("自动布局模式：立绘宽度百分比")]
    [Range(0.1f, 0.6f)] public float portraitWidthPercent = 0.25f;
    public PortraitSide defaultPortraitSide = PortraitSide.Left;
    public bool portraitPreserveAspect = true;
    [Tooltip("自定义布局模式：左/右两套立绘矩形（0~1，面板内归一化）")]
    public Rect leftPortraitRect01 = new Rect(0f, 0f, 0.28f, 1f);
    public Rect rightPortraitRect01 = new Rect(0.72f, 0f, 0.28f, 1f);

    [Header("文本样式")]
    public TextStyle speakerStyle = new TextStyle { fontSize = 40, alignment = TextAlignmentOptions.Left, wordWrapping = false };
    public TextStyle bodyStyle    = new TextStyle { fontSize = 36, alignment = TextAlignmentOptions.TopLeft, wordWrapping = true };

    [Header("下一句提示")]
    public string nextIndicatorText = "▶";
    public TextStyle nextStyle = new TextStyle { fontSize = 40, alignment = TextAlignmentOptions.Center, wordWrapping = false };

    [Header("使用外部 UI（可选：完全手摆）")]
    public bool useExternalUI = false;
    public CanvasGroup extCanvas;
    public Image extPortrait;
    public TMP_Text extSpeaker;
    public TMP_Text extBody;
    public GameObject extNextIndicator;
    public AudioSource extAudio;

    // ===== 静态 UI 引用（单例式，全局共用） =====
    static CanvasGroup sCanvas;
    static Image sPortrait;
    static TMP_Text sSpeaker;
    static TMP_Text sBody;
    static GameObject sNext;
    static AudioSource sAudio;
    static RectTransform sPanelRT, sPortraitRT, sNameRT, sBodyRT, sNextRT;

    static bool sIsOpen;
    static bool sIsTyping;
    static Coroutine sTypingCo;
    static OneFileDialogue sRunner;

    bool _playerIn;

    // ===== 触发区 =====
    void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag(playerTag)) _playerIn = true; }
    void OnTriggerExit2D(Collider2D other)  { if (other.CompareTag(playerTag)) _playerIn = false; }

    void Update()
    {
        if (_playerIn && !sIsOpen && Input.GetKeyDown(interactKey) && lines != null && lines.Count > 0)
            StartDialogue();
    }

    // ===== 开始对话 =====
    public void StartDialogue()
    {
        EnsureUI();
        if (sRunner) sRunner.StopAllCoroutines();
        sRunner = this;
        sRunner.StartCoroutine(CoRunDialogue(lines));
    }

    // ===== 主流程 =====
    IEnumerator CoRunDialogue(List<Line> seq)
    {
        OpenState(true);

        for (int i = 0; i < seq.Count; i++)
        {
            var line = seq[i];
            var side = line.overrideSide ? line.side : defaultPortraitSide;

            LayoutForLine(line, side);  // ★ 根据该句计算立绘矩形 & 文本区域
            BindLineAndType(line);

            bool awaitNext = true;
            while (awaitNext)
            {
                bool pressed = Input.GetKeyDown(advanceKey) || Input.GetKeyDown(altAdvanceKey) || Input.GetMouseButtonDown(0);
                if (pressed)
                {
                    if (sIsTyping) FinishTypingInstant();
                    else { PlayAdvanceSfx(); awaitNext = false; }
                }
                yield return null;
            }
        }

        Show(false);
        OpenState(false);
    }

    // ===== 绑定/打字机 =====
    void BindLineAndType(Line line)
    {
        Show(true);

        if (sPortrait)
        {
            sPortrait.enabled = line != null && line.portrait != null;
            sPortrait.sprite  = line != null ? line.portrait : null;
        }
        if (sSpeaker) sSpeaker.text = line != null ? (line.speaker ?? "") : "";
        if (sBody)
        {
            sBody.text = line != null ? (line.text ?? "") : "";
            sBody.maxVisibleCharacters = 0;
        }
        if (sNext) sNext.SetActive(false);

        if (line != null && line.voice && sAudio) sAudio.PlayOneShot(line.voice);

        if (sRunner && sTypingCo != null) sRunner.StopCoroutine(sTypingCo);
        sTypingCo = sRunner.StartCoroutine(CoType());
    }

    IEnumerator CoType()
    {
        sIsTyping = true;
        int total = sBody ? sBody.text.Length : 0;
        float vis = 0f;

        while (sBody && sBody.maxVisibleCharacters < total)
        {
            vis += Time.deltaTime * Mathf.Max(1f, charsPerSecond);
            sBody.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(vis));
            yield return null;

            if (Input.GetKeyDown(advanceKey) || Input.GetKeyDown(altAdvanceKey) || Input.GetMouseButtonDown(0))
            {
                sBody.maxVisibleCharacters = total;
                break;
            }
        }

        sIsTyping = false;
        if (sNext) sNext.SetActive(true);
    }

    void FinishTypingInstant()
    {
        if (!sBody) return;
        sBody.maxVisibleCharacters = sBody.text.Length;
        sIsTyping = false;
        if (sRunner && sTypingCo != null) sRunner.StopCoroutine(sTypingCo);
        if (sNext) sNext.SetActive(true);
    }

    void PlayAdvanceSfx()
    {
        if (sAudio && advanceClip) sAudio.PlayOneShot(advanceClip, advanceVolume);
    }

    // ===== 显隐/禁用 =====
    void OpenState(bool on)
    {
        sIsOpen = on;
        foreach (var b in disableWhileOpen) if (b) b.enabled = !on;
    }

    void Show(bool on)
    {
        if (!sCanvas) return;
        sCanvas.alpha = on ? 1f : 0f;
        sCanvas.interactable = on;
        sCanvas.blocksRaycasts = on;
        if (!on)
        {
            if (sBody) sBody.text = "";
            if (sSpeaker) sSpeaker.text = "";
            if (sPortrait) sPortrait.sprite = null;
            if (sNext) sNext.SetActive(false);
        }
    }

    // ===== 动态布局：按句子决定立绘矩形 + 文本区域 =====
    void LayoutForLine(Line line, PortraitSide side)
    {
        if (!sPanelRT || !sPortraitRT || !sNameRT || !sBodyRT || !sNextRT) return;

        // 1) 计算立绘归一化 Rect（0~1, 相对面板）
        Rect r01 = GetPortraitRect01(line, side);
        r01 = Clamp01(r01);

        // 2) 套进 RectTransform anchors
        sPortraitRT.anchorMin = new Vector2(r01.xMin, r01.yMin);
        sPortraitRT.anchorMax = new Vector2(r01.xMax, r01.yMax);
        sPortraitRT.offsetMin = sPortraitRT.offsetMax = Vector2.zero;
        if (sPortrait) sPortrait.preserveAspect = portraitPreserveAspect;

        // 3) 文本区域放在“剩余的横向区域”
        const float gap = 0.02f;
        if (side == PortraitSide.Left)
        {
            float xMin = Mathf.Clamp01(r01.xMax + gap);
            sNameRT.anchorMin = new Vector2(xMin, 0.60f);
            sNameRT.anchorMax = new Vector2(0.98f, 0.98f);
            sBodyRT.anchorMin = new Vector2(xMin, 0.05f);
            sBodyRT.anchorMax = new Vector2(0.98f, 0.60f);
        }
        else // Right
        {
            float xMax = Mathf.Clamp01(r01.xMin - gap);
            sNameRT.anchorMin = new Vector2(0.02f, 0.60f);
            sNameRT.anchorMax = new Vector2(xMax, 0.98f);
            sBodyRT.anchorMin = new Vector2(0.02f, 0.05f);
            sBodyRT.anchorMax = new Vector2(xMax, 0.60f);
        }
        sNameRT.offsetMin = sNameRT.offsetMax = Vector2.zero;
        sBodyRT.offsetMin = sBodyRT.offsetMax = Vector2.zero;

        // 下一句指示固定在右下
        sNextRT.anchorMin = new Vector2(0.95f, 0.05f);
        sNextRT.anchorMax = new Vector2(0.98f, 0.18f);
        sNextRT.offsetMin = sNextRT.offsetMax = Vector2.zero;
    }

    Rect GetPortraitRect01(Line line, PortraitSide side)
    {
        // 句子级：若勾选 overridePortraitRect，优先使用
        if (line != null && line.overridePortraitRect)
            return line.portraitRect01;

        // 全局：自定义矩形 or 按宽度百分比
        if (useCustomPortraitRect)
            return side == PortraitSide.Left ? leftPortraitRect01 : rightPortraitRect01;

        // 自动：根据宽度百分比分配左右
        float p = Mathf.Clamp01(portraitWidthPercent);
        if (side == PortraitSide.Left)
            return new Rect(0f, 0f, p, 1f);
        else
            return new Rect(1f - p, 0f, p, 1f);
    }

    static Rect Clamp01(Rect r)
    {
        float xMin = Mathf.Clamp01(r.xMin);
        float yMin = Mathf.Clamp01(r.yMin);
        float xMax = Mathf.Clamp01(r.xMin + r.width);
        float yMax = Mathf.Clamp01(r.yMin + r.height);
        xMax = Mathf.Max(xMin + 0.001f, xMax);
        yMax = Mathf.Max(yMin + 0.001f, yMax);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    // ===== 自动/外部 UI =====
    void EnsureUI()
    {
        if (useExternalUI)
        {
            // 使用外部 UI
            sCanvas   = extCanvas;
            sPortrait = extPortrait;
            sSpeaker  = extSpeaker;
            sBody     = extBody;
            sNext     = extNextIndicator;
            sAudio    = extAudio ? extAudio : (extCanvas ? extCanvas.gameObject.GetComponent<AudioSource>() : null);

            // RectTransform（若为空，不进行布局调整）
            sPanelRT   = extCanvas ? extCanvas.GetComponent<RectTransform>() : null;
            sPortraitRT= extPortrait ? extPortrait.rectTransform : null;
            sNameRT    = extSpeaker ? extSpeaker.rectTransform : null;
            sBodyRT    = extBody ? extBody.rectTransform : null;
            sNextRT    = extNextIndicator ? extNextIndicator.GetComponent<RectTransform>() : null;

            // 样式
            ApplyTextStyle(sSpeaker as TextMeshProUGUI, speakerStyle);
            ApplyTextStyle(sBody as TextMeshProUGUI, bodyStyle);
            if (sNext)
            {
                var t = sNext.GetComponent<TextMeshProUGUI>();
                if (t) { t.text = nextIndicatorText; ApplyTextStyle(t, nextStyle); }
            }
            return;
        }

        if (sCanvas) return; // 自动 UI 已创建过

        // 自动创建 Canvas
        var canvasGO = new GameObject("DialogueCanvas (Auto)");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        sCanvas = canvasGO.AddComponent<CanvasGroup>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // 面板
        var panel = new GameObject("DialoguePanel");
        panel.transform.SetParent(canvasGO.transform, false);
        sPanelRT = panel.AddComponent<RectTransform>();
        sPanelRT.anchorMin = panelAnchorMin;
        sPanelRT.anchorMax = panelAnchorMax;
        sPanelRT.offsetMin = sPanelRT.offsetMax = Vector2.zero;

        var panelBg = panel.AddComponent<Image>();
        panelBg.color = panelColor;

        // 立绘
        var portraitGO = new GameObject("Portrait");
        portraitGO.transform.SetParent(panel.transform, false);
        sPortraitRT = portraitGO.AddComponent<RectTransform>();
        sPortrait = portraitGO.AddComponent<Image>();
        sPortrait.color = Color.white;
        sPortrait.preserveAspect = portraitPreserveAspect;

        // 名字
        var nameGO = new GameObject("Name");
        nameGO.transform.SetParent(panel.transform, false);
        sNameRT = nameGO.AddComponent<RectTransform>();
        var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        sSpeaker = nameTMP;
        ApplyTextStyle(nameTMP, speakerStyle);

        // 正文
        var bodyGO = new GameObject("Body");
        bodyGO.transform.SetParent(panel.transform, false);
        sBodyRT = bodyGO.AddComponent<RectTransform>();
        var bodyTMP = bodyGO.AddComponent<TextMeshProUGUI>();
        sBody = bodyTMP;
        ApplyTextStyle(bodyTMP, bodyStyle);

        // 下一句提示
        var nextGO = new GameObject("NextIndicator");
        nextGO.transform.SetParent(panel.transform, false);
        sNextRT = nextGO.AddComponent<RectTransform>();
        var nextTMP = nextGO.AddComponent<TextMeshProUGUI>();
        nextTMP.text = nextIndicatorText;
        ApplyTextStyle(nextTMP, nextStyle);
        sNext = nextGO;
        sNext.SetActive(false);

        // Audio
        sAudio = canvasGO.AddComponent<AudioSource>();
        sAudio.playOnAwake = false;
        sAudio.spatialBlend = 0f;

        // 初始布局 & 隐藏
        LayoutForLine(null, defaultPortraitSide);
        Show(false);
    }

    void ApplyTextStyle(TextMeshProUGUI t, TextStyle style)
    {
        if (!t || style == null) return;
        if (style.font) t.font = style.font;
        t.fontSize = style.fontSize;
        t.color = style.color;
        t.alignment = style.alignment;
        t.enableWordWrapping = style.wordWrapping;
        t.lineSpacing = style.lineSpacing;
        t.fontStyle = style.fontStyle;
        t.richText = true;
    }
}
