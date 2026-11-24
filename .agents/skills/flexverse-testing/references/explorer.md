# Harness: explorer

In-game manual / visual scenarios (Source/FlexVerse.Test/Explorer): eyeball
behavior, tweening, layouts. No automated verdict, no evidence — pair with
`uv run Scripts/play` (same container).

The Explorer mod itself is architectural: discovery is declaration-driven
(`[Screen(path)]` on `Widget` subclasses); see `M-EXPLORER` in the frostline
graph. Scenario conventions (where they live, registration, what belongs
here vs [pickle](pickle.md)) — TODO.

## Graph

Explorer source files declare `// parents: @.frostline/graph/m-explorer.xml`.

## Virtual display: `Scripts/play --vm`

The whole UI loop lives INSIDE one container: game + Xvfb + input + capture.
No host display, no host dotool/grim. Full render (not headless/batch).
Manual diagnostics only — never an automated verdict.

```
uv run Scripts/play --vm
  │  build + stage + fresh runs/base (shared with ordinary play)
  └─► docker run -d fv-play-vm-<ts>          session.json in the run dir
        Xvfb :99 1600x900x24 (await xdotool getdisplaygeometry — no sleep)
        pin Prefs.xml to 1600x900 (sed; chown back — sed -i re-roots the file)
        RimWorldLinux -logfile /reports/Player.log
        await window → stability wait → windowsize/windowmove/focus → window-id
live session ◄──► docker exec -e DISPLAY=:99 <c> xdotool / import
uv run Scripts/play --stop <c>   # SIGTERM game → exit-status file → container exits
```

Drive (all inside the container; `$w` = id from `/reports/window-id` or
`xdotool search --onlyvisible --name RimWorld`):

```bash
docker exec -e DISPLAY=:99 "$c" xdotool getwindowgeometry --shell "$w"  # 1600x900 @ (0,0)
docker exec -e DISPLAY=:99 "$c" xdotool mousemove --sync --window "$w" X Y
docker exec -e DISPLAY=:99 "$c" xdotool click 1
docker exec -i -e DISPLAY=:99 "$c" xdotool -   # drag: ONE live process
  mousemove/mousedown ... moves ... mouseup    # ~0.7s pauses between actions
docker exec -e DISPLAY=:99 "$c" xdotool key --clearmodifiers Escape
docker exec -e DISPLAY=:99 "$c" import -window root /reports/screenshots/N-name.png
```

Facts and pitfalls (verified 2026-10-25):

- Run dir is always `.cache/runs/base` (vm and plain play share it); unique
  part is the container id — it is the session id in `session.json`.
- Unity re-asserts Prefs.xml resolution on scene changes; host sessions
  rewrite Prefs to the host monitor. The pre-launch pin is what keeps
  1600x900 — window fitting alone loses the race.
- Window auto-fit holds through boot → menu → Explorer; id also in
  `run/window-id`. After SIGTERM the game saves Prefs 1600x900 back.
- Keyboard: Escape/Space do nothing at the main menu — probe via
  Options → Escape closes. Mouse clicks/drag verified on Explorer
  (retarget TARGET change, tab switch, drag retarget).
- Player.log is live (bind-mounted `/reports`): read it during the session.
- Renderer inside Xvfb: llvmpipe (soft GL). Layout proof only — NOT
  equivalence with host GPU rendering.
- The Steam-API dialog exists in every container run (hidden behind
  Explorer in play); only Pickle scenarios remove it, Explorer does not.
