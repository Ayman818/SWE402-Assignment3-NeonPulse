# SWE402 — Assignment 3: Improving Game Feel

## Student
- **Name:** Ayman Al Johani (confirm spelling)
- **Student ID:** 202016260
- **Unity:** 6000.3.2f1 (Unity 6.3)

## How to run
Open the project folder in Unity Hub using Unity 6000.3.2f1. Open `Assets/_Project/Scenes/Before.unity` and press Play to see the unchanged baseline. Stop Play, open `Assets/_Project/Scenes/After.unity`, and press Play to see feedback. Use mouse movement or A/D or left/right arrow keys to move the paddle. Ball launches automatically. Use the 16:9 Game view. Restart Play for a new board.

## Intended feel
**Neon arcade / crisp, punchy, readable feedback.** Bricks burst crisply, the paddle visibly compresses, light audio communicates three distinct events, the score pops, and a brief cyan trail improves ball tracking. Every effect is short and intentionally restrained. Extra polish includes a gentle camera-background impact tint, loss flash, reactive LIVES display, brief ball highlight, and impact-normal-directed particle motion, a fast-fading elliptical impact ring, and a gently rising chime for successive bricks. Optional camera shake is OFF by default and can be enabled in the Inspector.

## Five implemented improvements
| Improvement | Trigger | Implementation file / object | Purpose |
|---|---|---|---|
| 1. Brick particle impact | `GameEvents.BrickDestroyed` | `GameFeelController.cs` → `OnBrick`, pooled ParticleSystems | Brief colored burst at the destroyed brick, without lingering objects |
| 2. Reactive squash | `GameEvents.BallHitPaddle` | `GameFeelController.cs` → `OnPaddle` and `Update`, Paddle/Visual | Compress paddle visuals on impact then restore scale, without affecting physics |
| 3. Three event sounds | `BallHitPaddle`, `BrickDestroyed`, `BallLost` | `GameFeelController.cs` → `MakeTone`, AudioSource | Different soft click, bright chime and descending loss tone; brick also triggers particles |
| 4. Score pop | `GameEvents.ScoreChanged` | `GameFeelController.cs` → `OnScore` and `Update`, Canvas/ScoreText | Momentarily enlarges score, leaving original HUD responsible for correct text |
| 5. Ball trail | Ball movement, cleared on loss | `GameFeelController.cs` → TrailRenderer attached at runtime | Helps track fast ball without changing physics or layout |

## Extra polish and Inspector tuning
These are **complementary responses** and do not change gameplay or replace the five rubric items:
- Brick event: colored burst + combo-tuned chime + a 0.19 s impact ring + subtle cool background pulse, with particles drifting in the impact-normal direction. The ring is a pooled visual overlay, not a new gameplay mechanic.
- Paddle event: squash/stretch and downward recoil **on the sprite child only**, plus short ball glow and click.
- Wall event: a very brief ball glow; no added game rules.
- Score event: the original HUD updates the number, while the text enlarges and warms to gold, then restores its original color.
- Loss event: descending sound, muted red background flash, LIVES scale/color pulse, and trail reset.
- Rapid-hit handling: fixed pool of ten particle emitters, twelve impact rings, and a 35 ms chime overlap guard.
- **Camera shake**: select `AFTER - Game Feel Effects` → `Game Feel Controller` → `Camera Shake Strength = 0` to disable. Default is 0 (off). If enabling it, use a restrained value such as 0.15, and verify there is no readability loss.

Recommended starting values: Paddle Squash Seconds `0.16`, Score Pop Seconds `0.20`, Sound Volume `0.22`, Trail Time `0.17`, Particles Per Brick `11`, Brick Flash Seconds `0.09`, Loss Flash Seconds `0.27`, Camera Shake Strength `0`, Paddle Recoil Strength `0.22`, Score Highlight Strength `0.75`.

## Competitive polish / top-three considerations
The top-three ranking is based on the **65 marks for feedback implementation and tuning**, not extra effect count. The added `ImpactRingPolish.cs` produces a fast, fading **elliptical** brick ring sized from the actual impact (0.19 seconds by default); it works alongside colored particles and a combo-tuned chime. Tune the `Impact Ring Polish` component in the After scene: reduce `Ring Opacity` to `0.35` if distracting, or set it to `0` to disable. Optional camera shake remains **OFF**. This is a competitive design choice, not a guarantee of a top-three finish.

## Tested / verification checklist
**Complete these checks in the Unity Editor; package generation is not a Unity runtime test.**
- [ ] Before plays and has no feedback; After shows all five effects
- [ ] Every destroyed brick emits particles and audio; particles disappear
- [ ] Repeated paddle contacts do not permanently stretch the paddle
- [ ] Paddle/brick/loss sounds are distinct and balanced
- [ ] Score changes correctly during rapid hits and resets correctly
- [ ] Ball trail is short and does not streak across the screen on loss
- [ ] Muted audio still leaves brick and loss states understandable
- [ ] No errors in Console and no gameplay change

## Playtest and revision — REQUIRES A REAL TESTER
**Tester observations:** [Ask another person to play both Before and After. Record their actual answers about preference, clarity, distraction. Do not give the tester's name.]

**Revision made after feedback:** [Record a REAL changed setting, such as reducing `trailTime` from 0.17 to 0.12, and why. Do not claim this until done.]

**Observed result after revision:** [What the tester noticed on re-test.]

## Demonstration video
**Public/unlisted accessible link:** (https://youtu.be/tb8j12gfrCc)

Record 2–3 minutes with narration and game audio, as described in `VIDEO_SCRIPT.md`.

## AI use and testing
**Tools:** OpenAI ChatGPT (GPT-6).

**Contribution:** Helped inspect the provided dead-only starter, implement the After-only GameFeelController in C#, configure a separate After scene, design procedural audio, feedback timing, and documentation.

**Developer review/testing:** [Describe your actual Unity runs, fixes, tuning, and playtest here; do not claim automated generation was runtime-tested.]

## Credits
- Original starter: [JuiceLab](https://github.com/serkoceyhan/JuiceLab) by Serkan Ceyhan, MIT; see `LICENSE` and `SOURCE.md`.
- TextMesh Pro / Unity package assets are retained from the starter.
- New audio is programmatically synthesized at runtime by the new C# script (no external audio files).
- New feedback code and the enhanced visual polish were drafted with ChatGPT. All feedback requires in-editor testing and honest documentation of a real playtest.

## NEON PULSE edition — additional presentation layer (After only)
The `NeonArcadeArtDirector.cs` component on `AFTER - Game Feel Effects` adds a new visual identity to the actual project (not just a checklist): dark navy backdrop, indigo/cyan atmospheric gradients, a dim animated arena grid, neon court rails, per-brick halo sprites, a responsive ball halo, soft paddle underglow, and pooled `+10` floating hit labels. All are decorative objects created at runtime without colliders or scripts that modify gameplay. `NeonArcadeArtDirector.cs` is separate from the event-based five graded improvements in `GameFeelController.cs`.

**Suggested comfort settings in the Inspector**: Grid Opacity 0.21, Brick Glow Opacity 0.22, Halo Opacity 0.30, Floating Text Opacity 0.95, Accent Intensity 1. If the grid interferes with seeing the ball, set Grid Opacity to 0.08 or untick Animate Grid. If labels distract, untick Show Floating Points. All changes affect After only. The ball's trail should be kept short; camera shake is off by default.

**Important verification**: The procedural world-space presentation has not been inspected in a Unity game viewport in this environment. In 16:9 verify that decoration stays behind the bricks and HUD, and that every spawned brick retains its exact original collider and coordinates. Verify the new `+10` labels match the original `Brick` scoreValue of 10. When changing the starter's brick score values, update/remove the decorative label accordingly. Test on Windows and inspect Console for missing shader/font warnings. Document any tuning you actually performed.

**Additional AI use**: ChatGPT drafted `NeonArcadeArtDirector.cs` (procedural gradient sprites, decorative lines, pooled world-space text) and refined the synthesized arcade sound timbre. These are generated in C# at runtime and use no third-party art or music. All gameplay QA, readability tuning, human playtesting, and documentation of revisions remain the student's responsibility.
