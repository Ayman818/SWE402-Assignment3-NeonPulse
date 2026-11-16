using System;
using TMPro;
using UnityEngine;

/// <summary>
/// After-scene-only feedback. Listens to gameplay events without touching scoring,
/// colliders, physics, speeds, controls, or the brick layout.
/// </summary>
public sealed class GameFeelController : MonoBehaviour
{
    [Header("Tuning - clean and punchy")]
    [Range(0.05f, 0.4f)] public float paddleSquashSeconds = 0.16f;
    [Range(0.05f, 0.4f)] public float scorePopSeconds = 0.20f;
    [Range(0f, 1f)] public float soundVolume = 0.22f;
    [Range(0.05f, 0.4f)] public float trailTime = 0.17f;
    [Range(1, 20)] public int particlesPerBrick = 11;

    [Header("Bonus polish (purely visual, no gameplay changes)")]
    [Range(0f, 0.3f)] public float brickFlashSeconds = 0.09f;
    [Range(0f, 0.5f)] public float lossFlashSeconds = 0.27f;
    [Range(0f, 1f)] public float cameraShakeStrength = 0f; // 0 = OFF (recommended)
    [Range(0f, 0.3f)] public float cameraShakeSeconds = 0.10f;
    [Range(0f, 1f)] public float paddleRecoilStrength = 0.22f;
    [Range(0f, 1f)] public float scoreHighlightStrength = 0.75f;

    private Transform paddleVisual;
    private Vector3 paddleRestScale;
    private TMP_Text scoreText;
    private Vector3 scoreRestScale;
    private float paddleTimer, scoreTimer;
    private TMP_Text livesText;
    private Color scoreRestColor, livesRestColor;
    private Vector3 livesRestScale;
    private float lifeTimer, flashTimer, shakeTimer;
    private float lastBrickTime = -100f;
    private Camera gameCamera;
    private Vector3 cameraRestPosition;
    private Color cameraRestBackground;
    private readonly Color lossTint = new Color(0.95f, 0.23f, 0.35f, 1f);
    private readonly Color brickTint = new Color(0.18f, 0.78f, 0.92f, 1f);
    private bool lossFlashActive;
    private SpriteRenderer ballVisual;
    private Color ballRestColor;
    private float ballGlowTimer;
    private Vector3 paddleRestPosition;
    private TrailRenderer trail;
    private AudioSource audioSource;
    private AudioClip paddleClip, brickClip, lostClip;
    private AudioClip[] comboBrickClips;
    private ParticleSystem[] particlePool;
    private int poolNext;
    private Material effectMaterial;

    private void Awake()
    {
        Paddle paddle = FindFirstObjectByType<Paddle>();
        Ball ball = FindFirstObjectByType<Ball>();
        HUD hud = FindFirstObjectByType<HUD>();
        if (paddle != null)
        {
            SpriteRenderer sprite = paddle.GetComponentInChildren<SpriteRenderer>();
            if (sprite != null)
            {
                paddleVisual = sprite.transform;
                paddleRestScale = paddleVisual.localScale;
                paddleRestPosition = paddleVisual.localPosition;
            }
        }
        if (hud != null)
        {
            TMP_Text[] texts = hud.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text item in texts)
                if (item.gameObject.name == "ScoreText") { scoreText = item; break; }
            if (scoreText != null)
            {
                scoreRestScale = scoreText.rectTransform.localScale;
                scoreRestColor = scoreText.color;
            }
            foreach (TMP_Text item in texts)
                if (item.gameObject.name == "LivesText") { livesText = item; break; }
            if (livesText != null)
            {
                livesRestScale = livesText.rectTransform.localScale;
                livesRestColor = livesText.color;
            }
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader != null) effectMaterial = new Material(shader) { name = "After Feedback Material (runtime)" };

        gameCamera = Camera.main;
        if (gameCamera != null)
        {
            cameraRestPosition = gameCamera.transform.localPosition;
            cameraRestBackground = gameCamera.backgroundColor;
        }
        if (ball != null)
        {
            ballVisual = ball.GetComponentInChildren<SpriteRenderer>();
            if (ballVisual != null) ballRestColor = ballVisual.color;
            // The visual trail is attached to the moving object, but never affects its body.
            trail = ball.gameObject.AddComponent<TrailRenderer>();
            trail.time = trailTime;
            trail.minVertexDistance = 0.07f;
            trail.startWidth = 0.18f;
            trail.endWidth = 0f;
            trail.numCapVertices = 2;
            trail.sortingOrder = -1;
            trail.startColor = new Color(0.24f, 0.93f, 1f, 0.60f);
            trail.endColor = new Color(0.24f, 0.93f, 1f, 0f);
            if (effectMaterial != null) trail.material = effectMaterial;
            trail.Clear();
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = soundVolume;
        paddleClip = MakeTone("Paddle - soft click", 280f, 440f, 0.085f, 0.55f);
        brickClip = MakeTone("Brick - bright chime", 610f, 980f, 0.13f, 0.48f);
        // Four notes within one restrained musical family: combo feels earned without a new rule.
        comboBrickClips = new AudioClip[4];
        for (int i = 0; i < comboBrickClips.Length; i++)
        {
            float multiplier = Mathf.Pow(1.12246f, i); // two semitones per tier
            comboBrickClips[i] = MakeTone("Combo brick note " + i,
                610f * multiplier, 980f * multiplier, 0.13f, 0.43f);
        }
        lostClip = MakeTone("Loss - descending", 390f, 160f, 0.24f, 0.65f);

        particlePool = new ParticleSystem[10];
        for (int i = 0; i < particlePool.Length; i++) particlePool[i] = MakeParticleEmitter(i);
    }

    private void OnEnable()
    {
        GameEvents.BallHitPaddle += OnPaddle;
        GameEvents.BrickDestroyed += OnBrick;
        GameEvents.ScoreChanged += OnScore;
        GameEvents.BallLost += OnLost;
        GameEvents.LivesChanged += OnLives;
        GameEvents.BallHitWall += OnWall;
    }

    private void OnDisable()
    {
        GameEvents.BallHitPaddle -= OnPaddle;
        GameEvents.BrickDestroyed -= OnBrick;
        GameEvents.ScoreChanged -= OnScore;
        GameEvents.BallLost -= OnLost;
        GameEvents.LivesChanged -= OnLives;
        GameEvents.BallHitWall -= OnWall;
        if (paddleVisual != null)
        {
            paddleVisual.localScale = paddleRestScale;
            paddleVisual.localPosition = paddleRestPosition;
        }
        if (scoreText != null)
        {
            scoreText.rectTransform.localScale = scoreRestScale;
            scoreText.color = scoreRestColor;
        }
        if (livesText != null)
        {
            livesText.rectTransform.localScale = livesRestScale;
            livesText.color = livesRestColor;
        }
        if (gameCamera != null)
        {
            gameCamera.transform.localPosition = cameraRestPosition;
            gameCamera.backgroundColor = cameraRestBackground;
        }
        if (ballVisual != null) ballVisual.color = ballRestColor;
        if (trail != null) trail.Clear();
    }

    private void Update()
    {
        if (paddleVisual != null)
        {
            paddleTimer = Mathf.Max(0f, paddleTimer - Time.deltaTime);
            float t = paddleSquashSeconds > 0f ? paddleTimer / paddleSquashSeconds : 0f;
            float strength = Mathf.Sin(t * Mathf.PI); // returns exactly to normal
            paddleVisual.localScale = Vector3.Scale(paddleRestScale,
                new Vector3(1f + paddleRecoilStrength * strength, 1f - paddleRecoilStrength * 1.25f * strength, 1f));
            paddleVisual.localPosition = paddleRestPosition + new Vector3(0f, -0.10f * strength, 0f);
        }
        if (scoreText != null)
        {
            scoreTimer = Mathf.Max(0f, scoreTimer - Time.deltaTime);
            float t = scorePopSeconds > 0f ? scoreTimer / scorePopSeconds : 0f;
            float pop = Mathf.Sin(t * Mathf.PI) * 0.26f;
            scoreText.rectTransform.localScale = scoreRestScale * (1f + pop);
            Color highlight = new Color(1f, 0.90f, 0.42f, scoreRestColor.a);
            scoreText.color = Color.Lerp(scoreRestColor, highlight, Mathf.Clamp01(pop * 3f * scoreHighlightStrength));
        }
        if (livesText != null)
        {
            lifeTimer = Mathf.Max(0f, lifeTimer - Time.deltaTime);
            float lifePulse = lossFlashSeconds > 0f ? Mathf.Sin(Mathf.PI * lifeTimer / Mathf.Max(lossFlashSeconds, 0.001f)) : 0f;
            livesText.rectTransform.localScale = livesRestScale * (1f + 0.17f * lifePulse);
            livesText.color = Color.Lerp(livesRestColor, lossTint, lifePulse * 0.8f);
        }
        if (ballVisual != null)
        {
            ballGlowTimer = Mathf.Max(0f, ballGlowTimer - Time.deltaTime);
            float glow = Mathf.Sin(Mathf.PI * ballGlowTimer / 0.15f);
            ballVisual.color = Color.Lerp(ballRestColor, new Color(0.48f, 1f, 1f, ballRestColor.a), Mathf.Max(0f, glow) * 0.55f);
        }
        if (gameCamera != null)
        {
            flashTimer = Mathf.Max(0f, flashTimer - Time.deltaTime);
            float total = lossFlashActive ? lossFlashSeconds : brickFlashSeconds;
            float intensity = total > 0f ? Mathf.Clamp01(flashTimer / total) : 0f;
            gameCamera.backgroundColor = Color.Lerp(cameraRestBackground, lossFlashActive ? lossTint : brickTint,
                intensity * (lossFlashActive ? 0.18f : 0.065f));
            shakeTimer = Mathf.Max(0f, shakeTimer - Time.deltaTime);
            float amplitude = cameraShakeSeconds > 0f ? (shakeTimer / cameraShakeSeconds) * cameraShakeStrength * 0.035f : 0f;
            gameCamera.transform.localPosition = cameraRestPosition + new Vector3(
                UnityEngine.Random.Range(-amplitude, amplitude), UnityEngine.Random.Range(-amplitude, amplitude), 0f);
        }
        if (trail != null) trail.time = trailTime;
        if (audioSource != null) audioSource.volume = soundVolume;
    }

    private void OnPaddle(ImpactInfo impact)
    {
        paddleTimer = paddleSquashSeconds;
        ballGlowTimer = 0.15f;
        if (paddleClip != null) audioSource.PlayOneShot(paddleClip, 0.85f);
    }

    private void OnBrick(ImpactInfo impact)
    {
        // A single event combines particles and sound.
        // Short cool-color background pulse reinforces an impact even with audio muted.
        flashTimer = brickFlashSeconds;
        lossFlashActive = false;
        shakeTimer = cameraShakeSeconds;
        // Cap dense overlaps so a multi-brick collision never becomes painfully loud.
        if (comboBrickClips != null && Time.time - lastBrickTime > 0.035f)
        {
            int tier = Mathf.Clamp((impact.Combo - 1) / 2, 0, comboBrickClips.Length - 1);
            audioSource.PlayOneShot(comboBrickClips[tier], 0.72f);
        }
        lastBrickTime = Time.time;
        ParticleSystem ps = particlePool[poolNext];
        poolNext = (poolNext + 1) % particlePool.Length;
        ps.transform.position = new Vector3(impact.Center.x, impact.Center.y, -0.1f);
        ps.Clear(true);
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(impact.Color);
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(impact.Normal.x * 0.65f);
        velocity.y = new ParticleSystem.MinMaxCurve(impact.Normal.y * 0.65f);
        ps.Emit(Mathf.Clamp(particlesPerBrick, 1, 20));
    }

    private void OnScore(int newScore)
    {
        // The original HUD continues to own and update the actual score text.
        // Don't animate when the game resets score to zero after the final life.
        if (newScore > 0) { scoreTimer = scorePopSeconds; }
        else scoreTimer = 0f;
    }

    private void OnLost()
    {
        if (lostClip != null) audioSource.PlayOneShot(lostClip);
        flashTimer = lossFlashSeconds;
        lossFlashActive = true;
        shakeTimer = cameraShakeSeconds;
        // The ball teleports back to launch on loss; suppress a giant trail line.
        if (trail != null) trail.Clear();
    }

    private void OnLives(int newLives)
    {
        lifeTimer = lossFlashSeconds;
    }

    private void OnWall(ImpactInfo impact)
    {
        // Keep wall feedback very subtle; paddle and brick sounds remain distinct.
        ballGlowTimer = 0.10f;
    }

    private ParticleSystem MakeParticleEmitter(int i)
    {
        GameObject go = new GameObject("Brick shards pooled " + i);
        go.transform.SetParent(transform, false);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.35f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.23f, 0.42f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 3.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.19f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 32;
        main.gravityModifier = 0.22f;
        var emission = ps.emission;
        emission.enabled = false;
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.16f;
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 10;
        if (effectMaterial != null) renderer.material = effectMaterial;
        return ps;
    }

    private static AudioClip MakeTone(string label, float startHz, float endHz, float seconds, float gain)
    {
        const int rate = 22050;
        int count = Mathf.CeilToInt(rate * seconds);
        float[] samples = new float[count];
        double phase = 0d;
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)count;
            float frequency = Mathf.Lerp(startHz, endHz, t);
            phase += 2d * Math.PI * frequency / rate;
            float envelope = Mathf.Pow(1f - t, 2f) * Mathf.Min(1f, t * 90f);
            // Original synthesized arcade timbre: fundamental + airy overtones,
            // a tiny transient click and a fast decaying envelope. No imported audio.
            float fundamental = (float)Math.Sin(phase);
            float overtones = 0.24f * (float)Math.Sin(phase * 2.0d)
                + 0.095f * (float)Math.Sin(phase * 3.0d);
            float transient = (i < rate / 170) ? (1f - i / (rate / 170f)) * 0.10f * (i % 2 == 0 ? 1f : -1f) : 0f;
            samples[i] = Mathf.Clamp((fundamental * 0.78f + overtones + transient) * gain * envelope, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create(label, count, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        if (effectMaterial != null) Destroy(effectMaterial);
        if (paddleClip != null) Destroy(paddleClip);
        if (brickClip != null) Destroy(brickClip);
        if (comboBrickClips != null)
            foreach (AudioClip note in comboBrickClips)
                if (note != null) Destroy(note);
        if (lostClip != null) Destroy(lostClip);
    }
}
