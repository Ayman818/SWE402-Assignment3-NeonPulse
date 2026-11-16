# 2–3 minute narrated demo script (adapt to your actual tested result)

**0:00–0:08** “This is SWE402 Assignment Three. My goal is a clean, punchy, readable brick-breaker.” Show **BEFORE** label.

**0:08–0:32** Play `Before.unity` (20–30 seconds), show ball hitting paddle, destroying bricks, changing score; say: “The gameplay works, but there are no feedback animations, effects, or sounds.”

**0:32–0:40** Stop Play and open `After.unity`. Show **AFTER** label.

**0:40–1:35** Play After. Point out brick colored particles, paddle squash, three distinct sounds, score scale pop, cyan ball trail, gold score highlight, subtle brick flash and red LIVES loss cue. Intentionally miss the ball once so loss sound and lives update are clear.

**1:35–2:03** Show `Assets/_Project/Scripts/Feedback/GameFeelController.cs` and Inspector on `AFTER - Game Feel Effects`. “The controller subscribes to GameEvents. A brick event triggers both a short particle burst and a chime. `paddleSquashSeconds` controls how quickly the paddle recovers; it changes only the Visual child, not the collider.” Show the parameter and your actual tuned value.

**2:03–2:30** Explain ACTUAL anonymous playtest feedback. Demonstrate the actual Inspector revision you made and its improvement. Example only: lower `trailTime` if tester says the trail is distracting.

**2:30–2:45** Show project folder with `Assets`, `Packages`, `ProjectSettings`, `README.md`, confirm video link; conclude the demonstration.

Important: Only describe testing and revisions actually completed. Record with OBS or another screen recorder with microphone + Unity game audio enabled. Make video accessible by URL.
