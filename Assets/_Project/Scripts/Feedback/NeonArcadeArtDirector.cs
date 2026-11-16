using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// AFTER-only art direction. Decorative renderers only: no colliders, rigidbodies,
/// scoring, input, gameplay time changes or updates to shared prefabs.
/// All temporary feedback is recycled. The Before scene stays untouched.
/// </summary>
[DisallowMultipleComponent]
public sealed class NeonArcadeArtDirector : MonoBehaviour
{
    [Header("Visual identity — NEON PULSE")]
    [Range(0f, 1f)] public float gridOpacity = 0.21f;
    [Range(0f, 1f)] public float brickGlowOpacity = 0.22f;
    [Range(0f, 1f)] public float haloOpacity = 0.30f;
    [Range(0f, 1f)] public float floatingTextOpacity = 0.95f;
    [Range(0.5f, 2f)] public float accentIntensity = 1.0f;
    [Header("Comfort / readability")]
    public bool animateGrid = true;
    public bool showFloatingPoints = true;
    public bool showBrickGlow = true;
    public bool showBallHalo = true;

    private const int FloaterCount = 16;
    private readonly List<LineRenderer> gridLines = new List<LineRenderer>();
    private readonly List<SpriteRenderer> brickGlows = new List<SpriteRenderer>();
    private readonly List<SpriteRenderer> decor = new List<SpriteRenderer>();
    private readonly TextMeshPro[] floaters = new TextMeshPro[FloaterCount];
    private readonly float[] floaterTime = new float[FloaterCount];
    private readonly Vector3[] floaterStart = new Vector3[FloaterCount];
    private readonly Color[] floaterColors = new Color[FloaterCount];
    private int nextFloater;
    private Material lineMaterial;
    private Texture2D radialTexture;
    private Texture2D pixelTexture;
    private Sprite radialSprite;
    private Sprite pixelSprite;
    private Transform ballTransform;
    private SpriteRenderer ballHalo;
    private Transform paddleTransform;
    private SpriteRenderer paddleHalo;
    private Camera cameraRef;
    private float arenaBeat;
    private bool createdBrickGlow;
    private readonly Color cyan = new Color(.16f, .93f, 1f, 1f);
    private readonly Color violet = new Color(.62f, .38f, 1f, 1f);

    private void Awake()
    {
        cameraRef = Camera.main;
        if (cameraRef == null || !cameraRef.orthographic) { enabled = false; return; }
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null) lineMaterial = new Material(shader) { name = "AFTER Neon line (runtime)" };
        BuildTextures();
        BuildBackground();
        BuildGrid();
        BuildRails();
        BuildBallHalo();
        BuildPaddleGlow();
        BuildFloaters();
    }

    private void OnEnable()
    {
        GameEvents.BrickDestroyed += BrickDestroyed;
        GameEvents.BallHitPaddle += PaddleHit;
        GameEvents.BallLost += BallLost;
        GameEvents.ScoreChanged += ScoreChanged;
    }
    private void OnDisable()
    {
        GameEvents.BrickDestroyed -= BrickDestroyed;
        GameEvents.BallHitPaddle -= PaddleHit;
        GameEvents.BallLost -= BallLost;
        GameEvents.ScoreChanged -= ScoreChanged;
        foreach (var floater in floaters) if (floater != null) floater.gameObject.SetActive(false);
    }

    private void Start() { CreateBrickGlows(); }

    private void LateUpdate()
    {
        if (!createdBrickGlow) CreateBrickGlows();
        float t = Time.time;
        float breathe = 0.5f + 0.5f * Mathf.Sin(t * 0.9f);
        for (int i = 0; i < gridLines.Count; i++)
        {
            var lr = gridLines[i];
            if (lr == null) continue;
            float alpha = gridOpacity * (animateGrid ? (0.52f + breathe * 0.28f + 0.1f * Mathf.Sin(t * 0.6f + i * 0.3f)) : 0.74f);
            Color c = i % 5 == 0 ? violet : cyan;
            c.a = Mathf.Clamp01(alpha * accentIntensity);
            lr.startColor = c; lr.endColor = c;
        }
        foreach (var glow in brickGlows)
        {
            if (glow == null) continue;
            var c = glow.color;
            c.a = showBrickGlow ? brickGlowOpacity * (0.75f + breathe * .25f) : 0f;
            glow.color = c;
        }
        if (ballHalo != null && ballTransform != null)
        {
            ballHalo.transform.position = ballTransform.position + new Vector3(0f, 0f, 0.01f);
            float beat = 1f + .10f * Mathf.Sin(t * 9f) + arenaBeat * .30f;
            ballHalo.transform.localScale = Vector3.one * 0.86f * beat;
            Color c = cyan; c.a = showBallHalo ? Mathf.Clamp01(haloOpacity + arenaBeat * .18f) : 0;
            ballHalo.color = c;
        }
        if (paddleHalo != null && paddleTransform != null)
        {
            paddleHalo.transform.position = paddleTransform.position + new Vector3(0, 0, .02f);
            Color c = cyan; c.a = .16f + .17f * arenaBeat;
            paddleHalo.color = c;
        }
        arenaBeat = Mathf.MoveTowards(arenaBeat, 0f, Time.deltaTime * 4.4f);
        for (int i = 0; i < floaters.Length; i++)
        {
            var f = floaters[i];
            if (f == null || !f.gameObject.activeSelf) continue;
            floaterTime[i] += Time.deltaTime;
            float progress = floaterTime[i] / .64f;
            if (progress >= 1f) { f.gameObject.SetActive(false); continue; }
            float lift = 0.63f * (1f - Mathf.Pow(1f - progress, 2f));
            f.transform.position = floaterStart[i] + Vector3.up * lift;
            f.transform.localScale = Vector3.one * (0.7f + 0.4f * (1f - progress));
            Color color = floaterColors[i]; color.a = Mathf.Clamp01((1f - progress) * 1.3f) * floatingTextOpacity;
            f.color = color;
        }
    }

    private void BrickDestroyed(ImpactInfo impact)
    {
        arenaBeat = Mathf.Max(arenaBeat, .55f);
        if (!showFloatingPoints) return;
        // Brick score is 10 in the unmodified starter; do not invent bonus points.
        ShowFloating(impact.Center, "+10", impact.Color);
    }
    private void PaddleHit(ImpactInfo impact) { arenaBeat = Mathf.Max(arenaBeat, .22f); }
    private void BallLost() { arenaBeat = 0f; }
    private void ScoreChanged(int value) { /* Score owned by original HUD. */ }

    private void ShowFloating(Vector2 position, string message, Color color)
    {
        int i = nextFloater;
        nextFloater = (nextFloater + 1) % FloaterCount;
        TextMeshPro f = floaters[i];
        if (f == null) return;
        f.text = message;
        f.transform.position = new Vector3(position.x, position.y + .30f, -.6f);
        floaterStart[i] = f.transform.position;
        floaterColors[i] = Color.Lerp(color, Color.white, .42f);
        floaterTime[i] = 0f;
        f.gameObject.SetActive(true);
    }

    private void BuildTextures()
    {
        radialTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        radialTexture.filterMode = FilterMode.Bilinear;
        radialTexture.wrapMode = TextureWrapMode.Clamp;
        var colors = new Color[64 * 64];
        for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
        {
            float r = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 31.5f;
            float a = Mathf.Pow(Mathf.Clamp01(1f - r), 2.1f);
            colors[y * 64 + x] = new Color(1, 1, 1, a);
        }
        radialTexture.SetPixels(colors); radialTexture.Apply();
        radialSprite = Sprite.Create(radialTexture, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 64f);
        pixelTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        pixelTexture.SetPixel(0, 0, Color.white); pixelTexture.Apply();
        pixelSprite = Sprite.Create(pixelTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1f);
    }

    private SpriteRenderer AddSprite(string name, Sprite sprite, Color tint, int order, Vector3 pos, Vector3 scale, Transform parent = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent == null ? transform : parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite; sr.color = tint; sr.sortingOrder = order;
        decor.Add(sr); return sr;
    }

    private void BuildBackground()
    {
        float h = cameraRef.orthographicSize * 2f;
        float w = h * cameraRef.aspect;
        Vector3 center = new Vector3(cameraRef.transform.position.x, cameraRef.transform.position.y, 2f);
        AddSprite("Midnight ink backdrop", pixelSprite, new Color(.026f, .035f, .105f, 1f), -100, center, new Vector3(w + 4f, h + 4f, 1));
        AddSprite("Left indigo atmospheric glow", radialSprite, new Color(.26f, .08f, .68f, .31f), -96,
            new Vector3(-7.8f, 1f, 1.9f), new Vector3(10f, 10f, 1));
        AddSprite("Right cyan atmospheric glow", radialSprite, new Color(.07f, .71f, .90f, .17f), -96,
            new Vector3(7.9f, -2.6f, 1.9f), new Vector3(9f, 9f, 1));
        AddSprite("Midnight top accent", pixelSprite, new Color(.19f, .8f, 1f, .25f), -80,
            new Vector3(0, 5.70f, 1.8f), new Vector3(18f, .025f, 1));
    }

    private void BuildGrid()
    {
        // Faint editorial grid behind gameplay. No physics components.
        for (int i = -9; i <= 9; i++) AddLine("Grid vertical " + i, new Vector3(i, -5.7f, 1.6f), new Vector3(i, 5.7f, 1.6f), .012f, -75, gridLines);
        for (int i = -5; i <= 5; i++) AddLine("Grid horizontal " + i, new Vector3(-9.7f, i, 1.6f), new Vector3(9.7f, i, 1.6f), .012f, -75, gridLines);
    }
    private void BuildRails()
    {
        // Parallel neon lines sit outside the playable brick grid.
        AddLine("Left neon boundary", new Vector3(-8.5f, -4.45f, 1.5f), new Vector3(-8.5f, 5.15f, 1.5f), .045f, -15, null);
        AddLine("Right neon boundary", new Vector3(8.5f, -4.45f, 1.5f), new Vector3(8.5f, 5.15f, 1.5f), .045f, -15, null);
        AddLine("Upper neon boundary", new Vector3(-8.5f, 5.15f, 1.5f), new Vector3(8.5f, 5.15f, 1.5f), .045f, -15, null);
    }
    private void AddLine(string name, Vector3 a, Vector3 b, float width, int sortingOrder, List<LineRenderer> list)
    {
        var go = new GameObject(name); go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2; lr.useWorldSpace = true;
        lr.SetPosition(0, a); lr.SetPosition(1, b);
        lr.startWidth = width; lr.endWidth = width;
        lr.sortingOrder = sortingOrder;
        lr.numCapVertices = 2;
        lr.startColor = new Color(.12f, .92f, 1f, .32f);
        lr.endColor = lr.startColor;
        if (lineMaterial != null) lr.material = lineMaterial;
        if (list != null) list.Add(lr);
    }

    private void BuildBallHalo()
    {
        Ball ball = FindFirstObjectByType<Ball>();
        if (ball == null) return;
        ballTransform = ball.transform;
        ballHalo = AddSprite("Ball - soft responsive halo", radialSprite, new Color(.2f, 1f, 1f, .3f), -2,
            ballTransform.position, Vector3.one * .85f);
    }
    private void BuildPaddleGlow()
    {
        Paddle paddle = FindFirstObjectByType<Paddle>();
        if (paddle == null) return;
        paddleTransform = paddle.transform;
        float width = paddle.GetComponentInChildren<SpriteRenderer>() != null ? paddle.GetComponentInChildren<SpriteRenderer>().bounds.size.x : 2.4f;
        paddleHalo = AddSprite("Paddle - underglow", radialSprite, new Color(.18f, 1f, 1f, .18f), -3,
            paddleTransform.position, new Vector3(width * 1.4f, .85f, 1f));
    }
    private void CreateBrickGlows()
    {
        Brick[] bricks = FindObjectsByType<Brick>(FindObjectsSortMode.None);
        if (bricks.Length == 0) return;
        foreach (Brick brick in bricks)
        {
            var renderer = brick.GetComponent<SpriteRenderer>();
            if (renderer == null) continue;
            // Attached to each spawned brick, so destruction cleans up the glow automatically.
            Color c = Color.Lerp(renderer.color, Color.white, .2f); c.a = brickGlowOpacity;
            var halo = AddSprite("Visual glow (no collider)", radialSprite, c, renderer.sortingOrder - 1,
                brick.transform.position + new Vector3(0, 0, .015f),
                new Vector3(renderer.bounds.size.x * 1.28f / Mathf.Max(.001f, Mathf.Abs(brick.transform.lossyScale.x)),
                    renderer.bounds.size.y * 2.0f / Mathf.Max(.001f, Mathf.Abs(brick.transform.lossyScale.y)), 1f), brick.transform);
            brickGlows.Add(halo);
        }
        createdBrickGlow = true;
    }
    private void BuildFloaters()
    {
        for (int i = 0; i < FloaterCount; i++)
        {
            var go = new GameObject("Pooled floating hit points " + i);
            go.transform.SetParent(transform, false);
            var f = go.AddComponent<TextMeshPro>();
            f.fontSize = 3.2f;
            f.alignment = TextAlignmentOptions.Center;
            f.fontStyle = FontStyles.Bold;
            f.sortingOrder = 24;
            f.enableWordWrapping = false;
            f.text = "+10";
            f.color = Color.white;
            f.rectTransform.sizeDelta = new Vector2(2f, 1f);
            floaters[i] = f;
            go.SetActive(false);
        }
    }
    private void OnDestroy()
    {
        if (lineMaterial != null) Destroy(lineMaterial);
        if (radialSprite != null) Destroy(radialSprite);
        if (pixelSprite != null) Destroy(pixelSprite);
        if (radialTexture != null) Destroy(radialTexture);
        if (pixelTexture != null) Destroy(pixelTexture);
    }
}
