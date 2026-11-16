# Enhanced Edition — Unity playtest and polish checklist

## Start
1. Open in **Unity 6000.3.2f1** and wait for imports to finish.
2. Open `Assets/_Project/Scenes/Before.unity`. 16:9 Game view, press Play. The scene should stay plain.
3. Stop Play. Open `Assets/_Project/Scenes/After.unity`, then press Play.
4. Select `AFTER - Game Feel Effects` in Hierarchy to inspect all tweakable settings.

## Verify each improvement
- **Brick:** particles appear at the hit brick, color follows that brick, and clean up without stacking; subtle background pulse and a chime accompany them.
- **Paddle:** visual squashes/recoils and fully returns on repeated hits. Collider stays unmodified.
- **Audio:** listen for click, chime, and falling loss tone. Turn computer audio down and up; avoid harsh overlap.
- **Score:** text updates accurately from HUD, grows and turns gold briefly, then returns to baseline.
- **Additional:** cyan ball trail follows movement, fades quickly, and clears after loss.
- **Loss:** LIVES pulse and red background flash. Test with muted audio.
- **Before:** re-open original scene and confirm no effects.

## Important testing edge cases
- Hit several bricks in rapid sequence: particles should not permanently accumulate or cause painful audio peaks.
- Lose the final life: the starter resets score and lives but does not rebuild the brick layout, by design.
- Repeated paddle contacts: no permanent scale or position change.
- Disable this component during Play mode: visual scales, colors and camera background should be restored.
- Set `Camera Shake Strength = 0` to disable camera shake. Default is off.

## Real playtest (not yet performed)
Ask a person to play both scenes. Ask: (1) Which feels more satisfying and why? (2) Which effects make events clearer? (3) What is distracting? Write their responses (without name) in the README, change one inspector value and run the After test again. Don't claim this happened until it actually has.

## Suggested adjustments (choose based on real feedback)
- Trail blocks the ball → reduce Trail Time 0.17 → 0.12.
- Paddle response is too subtle → increase Paddle Recoil Strength 0.22 → 0.27.
- Background tint is too noticeable → reduce Brick Flash Seconds 0.09 → 0.05.
- Sound is overpowering → reduce Sound Volume 0.22 → 0.16.

## Submission
Add your real student ID, anonymous playtest findings, actual revision, and accessible 2–3 minute narrated video link to README. Upload Assets, Packages, ProjectSettings, README, LICENSE, SOURCE and project docs to GitHub. Do not upload Library, Temp, Logs. Submit the repository link in Blackboard.

**Status:** Code and Unity scene integration are prepared, but no Unity Editor run or live human playtest has been performed here. You must verify before submission.
