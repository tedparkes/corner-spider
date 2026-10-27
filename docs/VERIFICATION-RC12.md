# Verification for 1.0.0-rc.12

Local Windows verification, October 23, 2026. RC11 source, executable, packages
and Git tag were preserved before changing ground travel.

## Automated behavior

The Windows .NET Framework build and all deterministic suites pass. See
[behavior output](behavior-checks.txt). The new motion checks run 184 routes:
all 20 skins, 20–180% size, simulated 100–200% DPI, 16/33/100 ms frames,
eight travel directions and all four hard desktop edges.

Measured moving bouts, including final tiny adjustments, span 2.4–411.7 logical
pixels; peak speed reaches 358.3 logical pixels/second. 139 routes include real
sideways motion with the body facing away from the velocity vector. Checks require
substantial runs, stationary pauses, path deviation, direction-correct leg strides,
arrival at the original target, fixed joint lengths and full-body edge clearance.
Raised-foreleg checks across every skin also verify that turning gait cannot pull
the greeting pose sideways.
The deliberately seeded scenarios are regression fixtures, not a frequency survey
of biological behavior. Motion is a stylized interpretation.

The previous test requiring every burst to last less than 0.8 seconds was replaced
with a requirement for a sustained run alongside the existing short-step/pause
checks. Other bounds were retained. Return-speed and partial-monitor-seam failures
found during implementation were fixed without relaxing the nesting checks.

All existing suites pass, including 4,574 pounce hunts, 56 complete Portia web
hunts, cursor attention and boarding, species anatomy, settings persistence,
multiple-monitor routing, nest entry/exit and interruption cleanup. Resting near
corners remains the dominant activity; travel variation applies when moving.

## Native rendering and packages

`JumpingSpider.exe --travel-preview <folder>` exports consecutive native WPF
frames at 25 fps and a [movement trace](travel-trace.csv). The chosen seeded trip
includes longer runs, sideways steps, pauses and arrival. Its frames and enlarged
leg detail were visually inspected. An animated preview was encoded from those
frames at their original timing. See [path](travel-path.png) and
[sideways pose](travel-sideways.png). Portia encounter previews were also refreshed
from the final behavior. The initial preview seed had no sideways event, so a
seed exercising all requested motions was selected rather than forcing an event.

Both release ZIPs are verified entry by entry. Extracted source rebuilds with the
complete suite. The extracted portable executable reproduces the travel and
encounter previews. Source bytes match the local Git repository. The installed
RC12 executable matches the portable package, and the Desktop shortcut points
to that build. Release verification records a short normal-launch resource sample.

## Scope and manual follow-up

The full native overlay, tray, settings, cursor-restoration and focus suite last
passed on RC10; [those results](VERIFICATION-RC10.md) and
[Windows diagnostics](windows-checks.txt) remain available. It was not rerun in
full for RC12. [RC11 verification](VERIFICATION-RC11.md) documents the prior
anchored-side plucking fix. A long RC12 resource soak has not been performed.

Watch a long corner-to-corner patrol at your preferred size: look for distinct
short probes and longer runs, occasional sideways adjustments and planted pauses.
Hold the cursor nearby midway through a trip and confirm it still gets attention.
Then call the spider home and check its deliberate entry through the hammock end.
Physical mixed-DPI changes, unplug/reconnect and sleep/resume remain manual checks.

The hosted download is private to the user's ChatGPT account. No public GitHub
release has been published.
