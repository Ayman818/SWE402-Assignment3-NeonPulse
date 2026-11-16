using UnityEngine;

/// <summary>
/// After-only decorative impact rings. Uses the existing brick event; never touches
/// ball movement, score, colliders, lives, or destroyed bricks.
/// Reuses a fixed pool and returns every renderer to its invisible state.
/// </summary>
public sealed class ImpactRingPolish : MonoBehaviour
{
    [Header("Optional brick shockwave: set opacity to zero to disable")]
    [Range(0f, 1f)] public float ringOpacity = 0.62f;
    [Range(0.08f, 0.4f)] public float ringDuration = 0.19f;
    [Range(0.6f, 2.0f)] public float ringExpansion = 1.26f;
    [Range(0.01f, 0.13f)] public float ringWidth = 0.045f;

    private const int PoolSize = 12;
    private const int Points = 32;
    private readonly LineRenderer[] rings = new LineRenderer[PoolSize];
    private readonly float[] ages = new float[PoolSize];
    private readonly float[] durations = new float[PoolSize];
    private readonly Vector2[] sizes = new Vector2[PoolSize];
    private readonly Color[] colors = new Color[PoolSize];
    private int cursor;
    private Material ringMaterial;

    private void Awake()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader != null) ringMaterial = new Material(shader) { name = "After Ring Material (runtime)" };
        for (int i = 0; i < PoolSize; i++)
        {
            var go = new GameObject("Reusable impact ring " + i);
            go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = Points;
            lr.numCapVertices = 2;
            lr.numCornerVertices = 2;
            lr.textureMode = LineTextureMode.Stretch;
            lr.sortingOrder = 12;
            lr.startWidth = ringWidth;
            lr.endWidth = ringWidth;
            if (ringMaterial != null) lr.material = ringMaterial;
            lr.enabled = false;
            rings[i] = lr;
        }
    }

    private void OnEnable() => GameEvents.BrickDestroyed += OnBrick;

    private void OnDisable()
    {
        GameEvents.BrickDestroyed -= OnBrick;
        for (int i = 0; i < PoolSize; i++)
            if (rings[i] != null) rings[i].enabled = false;
    }

    private void OnBrick(ImpactInfo impact)
    {
        if (ringOpacity <= 0f) return;
        int i = cursor;
        cursor = (cursor + 1) % PoolSize;
        LineRenderer ring = rings[i];
        if (ring == null) return;
        ages[i] = 0f;
        durations[i] = ringDuration;
        sizes[i] = new Vector2(Mathf.Max(0.35f, impact.Size.x), Mathf.Max(0.25f, impact.Size.y));
        colors[i] = impact.Color;
        ring.transform.position = new Vector3(impact.Center.x, impact.Center.y, -0.15f);
        ring.enabled = true;
        DrawRing(i, 0f);
    }

    private void Update()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            var lr = rings[i];
            if (lr == null || !lr.enabled) continue;
            ages[i] += Time.deltaTime;
            float t = Mathf.Clamp01(ages[i] / Mathf.Max(0.01f, durations[i]));
            if (t >= 1f) { lr.enabled = false; continue; }
            DrawRing(i, t);
        }
    }

    private void DrawRing(int i, float progress)
    {
        LineRenderer line = rings[i];
        // Quick ease-out: fast response, soft finish, no persistent rings.
        float ease = 1f - Mathf.Pow(1f - progress, 3f);
        float scale = Mathf.Lerp(0.38f, ringExpansion, ease);
        float rx = Mathf.Min(sizes[i].x, 2.1f) * 0.5f * scale;
        float ry = Mathf.Min(sizes[i].y, 1.2f) * 0.5f * scale;
        for (int k = 0; k < Points; k++)
        {
            float angle = k * Mathf.PI * 2f / Points;
            line.SetPosition(k, new Vector3(Mathf.Cos(angle) * rx, Mathf.Sin(angle) * ry, 0f));
        }
        Color c = colors[i];
        c.a = Mathf.Clamp01(ringOpacity * Mathf.Pow(1f - progress, 1.6f));
        line.startColor = c;
        line.endColor = c;
        line.startWidth = ringWidth * (1f - progress * 0.55f);
        line.endWidth = line.startWidth;
    }

    private void OnDestroy()
    {
        if (ringMaterial != null) Destroy(ringMaterial);
    }
}
