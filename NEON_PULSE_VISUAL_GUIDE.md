# NEON PULSE — judge-facing visual identity and final polish

This is a materially redesigned AFTER presentation, not a replacement for the five mandatory feedback features.

## New scene presentation (automatically activated in After)
- Navy-black backdrop covering the orthographic 16:9 stage
- Soft magenta and cyan atmospheric glows using original procedural sprite textures
- Dim cyan/violet line grid, gently breathing over time
- Glowing arena frame and non-physical rails
- One subtle decorative halo for each spawned brick, destroyed with its parent brick
- Responsive cyan halo around the ball, with a quiet pulse on paddle/brick events
- Soft paddle underglow
- Floating +10 labels at individual brick destructions, pooled and fading cleanly
- Richer synthesized sound tone, no downloaded or copyrighted samples

## Inspect and tune
Open `Assets/_Project/Scenes/After.unity`. In the Hierarchy select `AFTER - Game Feel Effects`.

### Neon Arcade Art Director
- Grid Opacity: 0.21 — try 0.08 if too busy
- Brick Glow Opacity: 0.22 — try 0.14 for less bloom
- Halo Opacity: 0.30 — if the ball seems too large, lower to 0.20
- Floating Text Opacity: 0.95 — disable via Show Floating Points if distracting
- Accent Intensity: 1.0 — avoid >1.2 unless legibility is clearly preserved
- Animate Grid: true — can be turned off for a cleaner experience

### Game Feel Controller
- Camera Shake Strength = 0 is the safe default and explicit disable setting
- Sound Volume = 0.22; playtest with headphones and speakers
- Score Pop Seconds = 0.20, Paddle Squash Seconds = 0.16, Trail Time = 0.17

### Impact Ring Polish
- Ring Opacity = 0.62, Duration = 0.19. If crowded, test 0.35 and document the before/after in README.

## Test gates before submission
1. Enter Play in Before and After separately. Baseline remains completely unchanged.
2. In After, ensure that **the ball, paddle, and bricks are readable** above any background art.
3. Check particle + ring + chime + floating +10 at brick events; confirm each label fades and reuses its pool.
4. Hit paddle 10 times. Squash must always return to the initial visual dimensions.
5. Destroy bricks quickly; no pileup of effects; no volume clipping; the score must match game logic.
6. Deliberately lose a life: red cue, descending sound, normal ball reset, no giant trail streak.
7. Temporarily mute sound and confirm the loss and brick destruction remain understandable.
8. Have a real tester compare scenes and offer critical feedback, make a real tuning revision, then verify again.
9. Check Console: zero compile/runtime errors. Address any font/shader compatibility warnings.
10. Record 2–3 minute narration with game audio, add actual student ID and URL, commit Assets/Packages/ProjectSettings, submit GitHub link.

## Important
This package was generated and structurally checked outside the Unity Editor. Its appearance, timing, and editor compatibility still require **your real Unity playthrough**. A top-three ranking cannot be guaranteed by code volume or extra effects.
