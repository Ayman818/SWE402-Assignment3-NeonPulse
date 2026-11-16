# Top-three focused final checklist — SWE 402 Assignment 3

## What actually earns rank
The instructor ranks eligible submissions on the 65 points for (1) reliable feedback and (2) timing/intensity/readability. More effects do not automatically mean higher marks. Top three also requires a complete submission and unchanged game rules.

## Verify directly in Unity 6000.3.2f1
1. Use 16:9 view. Play **Before** for 20–30 seconds: no feedback.
2. Play **After**: a brick must produce **particles + short ring + chime** at the correct location, and increase score.
3. Hit 10+ bricks: particle/ring pools should clear without accumulation; chimes should be audible without clipping.
4. Hit paddle 10+ times: its visual must fully recover; verify the collider does not change.
5. Lose a life deliberately: hear a descending sound, see red screen pulse and lives feedback; no long streak across the reset.
6. Mute audio: game events must still be readable.
7. Open Console: zero compiler errors; capture a screenshot for your own check.
8. Compare original `Before.unity` and gameplay scripts with starter (unchanged).
9. If rings distract, tune Ring Opacity from 0.62 to 0.35 and compare. If screen pulses distract, set Brick Flash Seconds = 0.
10. Ask a **real other person** to play Before and After, collect their observations, make and record at least one actual revision, then test again.

## Demonstration video (2–3 minutes, narrated with game sound)
- 0:00–0:25 Before, silent, identify bland feedback
- 0:25–1:28 After: show paddle squash, brick particles/rings + sounds, score pop, ball trail, and intentional ball loss
- 1:28–1:54 Show `GameFeelController.cs` or `ImpactRingPolish.cs`, name `GameEvents.BrickDestroyed` and tuning parameter `ringDuration = 0.19`
- 1:54–2:18 Explain genuine tester comment and actual change, e.g. Ring Opacity 0.62 to 0.35 **only if you really made it**
- 2:18–2:30 Recap readability and that core gameplay is unchanged

## Submission (do not skip)
- README: your student ID, real tester findings + revision, playable video URL, AI use and testing account.
- GitHub contains `Assets`, `Packages`, `ProjectSettings`, original MIT `LICENSE`; exclude `Library`, `Temp`, `Logs`.
- Submit **one GitHub repository link** to Blackboard before **2026-11-16 14:00 (Riyadh time)**.

## Honesty
This package was checked structurally but cannot be executed within Unity here. No playtest or runtime validation has been performed on your behalf. Top-three placement cannot be promised.
